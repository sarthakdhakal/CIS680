
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NewDotnet.Code;
using NewDotnet.Context;
using NewDotnet.DataLayer;
using System.Diagnostics.Metrics;
using static System.Collections.Specialized.BitVector32;

namespace NewDotnet.Models
{

    public partial class OODBModel
    {

        private readonly OODBModelContext _context;
        private readonly IMemoryCache _cache;


        public OODBModel(OODBModelContext context)
        {
            _context = context;
            _cache = new MemoryCache(new MemoryCacheOptions());
        }



        internal int ExecuteStoredProcedure(string spName, string parameters)
        {

            return _context.Database.ExecuteSql($"EXEC {spName} {parameters}");
        }




        public FullPlaylistItem[] GetExtendedPlaylist(int id)
        {
            // Fetch the entire playlist and related content details in a single query
            // First, get the necessary content IDs separately if they will be reused.
            var playlistContentIds = _context.Playlists
       .Where(x => x.PlaylistId == id && x.ItemType == "c")
       .Select(x => x.ItemId);

            // Perform an explicit join between Contents and the Playlist IDs.
            var contents = _context.Contents
        .Where(c => playlistContentIds.Contains(c.ContentId))
        .Include(c => c.Section)
        .GroupBy(c => c.ContentId) // Group by ContentId
        .Select(g => g.FirstOrDefault()) // Select the first content of each group
        .ToDictionary(c => c.ContentId, c => new { c.SectionId, SectionTitle = c.Section.SectionTitle });

            // Now fetch your playlist items and project them safely.
            var playlistItems = _context.Playlists
                .Where(x => x.PlaylistId == id)
                .OrderBy(x => x.PlaylistOrder)
                .Select(x => new FullPlaylistItem
                {
                    ItemId = x.ItemId,
                    ItemType = x.ItemType,
                    PlaylistOrder = x.PlaylistOrder,
                    PlaylistId = x.PlaylistId,
                    // Use the pre-fetched content details safely
                    SectionId = x.ItemType == "c" ? (contents.ContainsKey(x.ItemId) ? contents[x.ItemId].SectionId : -1) : -1,
                    SectionTitle = x.ItemType == "c" ? (contents.ContainsKey(x.ItemId) ? contents[x.ItemId].SectionTitle : null) : null
                }).ToArray();

            // Enhance items with isFirst and isLast flags
            if (playlistItems.Any())
            {
                playlistItems[0].IsFirst = 1;
                playlistItems[playlistItems.Length - 1].IsLast = 1;
            }

            // Handle special cases for item types 'q' and 's'
            foreach (var item in playlistItems)
            {
                switch (item.ItemType)
                {
                    case "q":
                        // Adjust SectionId or other properties as needed
                        item.SectionId = item.ItemId; // or other logic as required
                        item.SectionTitle = _context.Contents.Include(c => c.Section).FirstOrDefault(c => c.ContentId == item.ItemId)?.Section?.SectionTitle;
                        break;
                    case "s":
                        item.SectionId = -1; // or other default/error value as needed
                        item.SectionTitle = ""; // default or error text
                        break;
                    default:
                        if (item.ItemType != "c")
                            throw new ArgumentException($"Invalid playlist item type {item.ItemType} found.");
                        break;
                }
            }

            return playlistItems;
        }

        // Additional methods like GetUserSpecificPlaylist should follow the similar pattern.

        // Settings class for configuration values

        public FullPlaylistItem[] GetUserSpecificPlaylist(string userId, int id)
        {
            UpdateCaches(_context);

            // Start with the complete extended playlist
            var extendedPlaylist = GetExtendedPlaylist(id).ToList();

            // List for indices that need to be removed
            var indicesToRemove = new List<int>();

            for (int i = 0; i < extendedPlaylist.Count; i++)
            {
                var item = extendedPlaylist[i];
                if (item.ItemType == "c")
                {
                    // Check if user should see this item
                    if (!ShouldDisplaySection(userId, (int)item.SectionId) || !ShouldDisplayContent(userId, item.ItemId))
                    {
                        indicesToRemove.Add(i);
                    }
                }
                else if (item.ItemType == "q" && !ShouldDisplaySection(userId, item.ItemId))
                {
                    indicesToRemove.Add(i);
                }
            }

            // Remove items from the playlist starting from the end to avoid shifting indices
            foreach (var index in indicesToRemove.OrderByDescending(i => i))
            {
                extendedPlaylist.RemoveAt(index);
            }

            // Reset flags for 'first' and 'last' items in the playlist
            if (extendedPlaylist.Any())
            {
                foreach (var item in extendedPlaylist)
                {
                    item.IsFirst = 0;
                    item.IsLast = 0;
                }
                extendedPlaylist[0].IsFirst = 1;
                extendedPlaylist[extendedPlaylist.Count - 1].IsLast = 1;
            }

            return extendedPlaylist.ToArray();
        }
        public bool ShouldDisplayContent(string userId, int contentId)
        {
            UpdateCaches(_context);

            if (!_cache.TryGetValue("contentFlags", out List<Tuple<int, int>> contentFlagCache) ||
                   !_cache.TryGetValue("accountFlags", out List<Tuple<string, int>> accountFlagCache))
            {
                // If cache retrieval fails, return false
                return false;
            }

            // If the user should see everything, always return true.
            if (HasUserFlag(userId, "showall")) return true;

            // Get all flags for the specified content
            var allFlagsForContent = contentFlagCache.Where(x => x.Item1 == contentId).Select(x => x.Item2).ToArray();

            // If content has no flags, assume always allow everyone to view.
            if (!allFlagsForContent.Any()) return true;

            // Get all flags for the user
            var allFlagsForUser = accountFlagCache.Where(x => x.Item1 == userId).Select(x => x.Item2).ToArray();

            // Check if there is at least one common flag between content flags and user flags
            return allFlagsForContent.Intersect(allFlagsForUser).Any();
        }

        public bool ShouldDisplaySection(string userId, int sectionId)
        {
            // TODO: Allow for different kinds of flags (must-have-all versus must-have-one)
            // Assume must-have-one for this

            UpdateCaches(_context);

            if (!_cache.TryGetValue("sectionFlags", out List<Tuple<int, int>> sectionFlagCache) ||
          !_cache.TryGetValue("accountFlags", out List<Tuple<string, int>> accountFlagCache))
            {
                // If cache retrieval fails, return false
                return false;
            }

            // If the user should see everything, always return true.
            if (HasUserFlag(userId, "showall")) return true;

            // Get all flags for the specified section
            var allFlagsForContent = sectionFlagCache.Where(x => x.Item1 == sectionId).Select(x => x.Item2).ToArray();

            // If content has no flags, assume always allow everyone to view.
            if (!allFlagsForContent.Any()) return true;

            // Get all flags for the user
            var allFlagsForUser = accountFlagCache.Where(x => x.Item1 == userId).Select(x => x.Item2).ToArray();

            // Check if there is at least one common flag between content flags and user flags
            return allFlagsForContent.Intersect(allFlagsForUser).Any();
        }


        public bool HasUserFlag(string user, string flagId)
        {
            List<int> thisAccountFlags = new List<int>();
            Dictionary<string, object> filteredFlags = new Dictionary<string, object>();

            UpdateCaches(_context);

            // Try to get account flags from cache
            if (_cache.TryGetValue("accountFlags", out List<Tuple<string, int>> accountFlagCache))
            {
                thisAccountFlags = accountFlagCache.Where(x => x.Item1 == user).Select(x => x.Item2).ToList();
            }

            // Try to get flags from cache
            if (_cache.TryGetValue("flags", out List<Tuple<string, string, int>> flagCache))
            {
                // Example LINQ query to filter the flags cache
                var flag = flagCache.FirstOrDefault(x => x.Item2 == "user" && x.Item1 == flagId);

                if (flag != null)
                {
                    return thisAccountFlags.Contains(flag.Item3);
                }
            }

            return false;
        }

        public bool HasUserFlags(string user, string[] flagIds, bool mustHaveAll)
        {
            var hasFlags = flagIds.Select(flagId => HasUserFlag(user, flagId));

            return mustHaveAll ? hasFlags.All(x => x) : hasFlags.Any(x => x);
        }

        public bool HasUserFlags(string user, string[] flagIds)
        {
            return HasUserFlags(user, flagIds, false);
        }

        public bool IsAdmin(string user)
        {
            return HasUserFlags(user, new[] { "siteadmin", "admin" });
        }

        public bool IsComplete(BearerTokenContents btc)
        {
            return HasUserFlag(btc.StarId, "complete");
        }

        public string GetConfigurationValue(int playlistId, string key)
        {
            string optionKey = playlistId < 0 ? key : $"{key}.{playlistId}";
            return _context.Configurations.FirstOrDefault(x => x.OptionShortName == optionKey)?.OptionValue;
        }

        public string GetConfigurationValue(string key)
        {
            return GetConfigurationValue(-1, key);
        }

        public bool HasPassedQuiz(string user, int sectionId)
        {
            return _context.AccountQuizzes.Count(x => x.StarId == user && x.SectionId == sectionId && x.AccountPassed) > 0;
        }

        public int GetFlagIdByName(string flagName)
        {
            int? flagId = _context.Flags
           .Where(f => f.FlagName.Equals(flagName))
           .Select(f => f.FlagId)
           .FirstOrDefault();

            return flagId ?? -1;
        }

        public bool SetUserFlag(string username, string flagName)
        {
            int flagId = GetFlagIdByName(flagName);
            if (flagId == -1) return false;

            var user = _context.Accounts.FirstOrDefault(x => x.StarId == username);
            if (user == null) return false;

            if (user.Flags.All(x => x.FlagId != flagId))
            {
                user.Flags.Add(_context.Flags.First(x => x.FlagId == flagId));
                _context.SaveChanges();
            }
            return true;
        }

        public bool UnsetUserFlag(string username, string flagName)
        {
            int flagId = GetFlagIdByName(flagName);
            if (flagId == -1) return false;

            var user = _context.Accounts.FirstOrDefault(x => x.StarId == username);
            if (user == null) return false;

            var flagToRemove = user.Flags.FirstOrDefault(x => x.FlagId == flagId);
            if (flagToRemove != null)
            {
                user.Flags.Remove(flagToRemove);
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public void LogAuditEvent(string source, string starId, string message, string before, string after, bool commitNow)
        {
            _context.AuditLogs.Add(new AuditLog()
            {
                EntryTime = DateTime.Now,
                EntrySource = source,
                EntryStarid = starId,
                EntryText = message,
                ContentBefore = before,
                ContentAfter = after
            });
            if (commitNow) ;
        }
        public void LogAuditEvent(string source, string starId, string message) { LogAuditEvent(source, starId, message, null, null, false); }
        public void LogAuditEvent(string source, string starId, string message, bool commitNow) { LogAuditEvent(source, starId, message, null, null, commitNow); }

        private void UpdateCaches(OODBModelContext _context)
        {
            // Should reduce major queries per instance to four.
            /*  if (!_cache.TryGetValue("sectionFlags", out List<Tuple<int, int>> sfc))
              {
                  var query = _context.Sections.Where(x => x.Flags.Any()).Select(x => new { x.SectionId, x.Flags }).ToList();
                  sfc = new List<Tuple<int, int>>();
                  foreach (var c in query)
                  {
                      foreach (var d in c.Flags)
                      {
                          sfc.Add(new Tuple<int, int>(c.SectionId, d.FlagId));
                      }
                  }
                  _cache.Set("sectionFlags", sfc, new MemoryCacheEntryOptions
                  {
                      // Set any cache options here, such as expiration, etc.
                      AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1600) // Example expiration time
                  });
              }

              if (!_cache.TryGetValue("accountFlags", out List<Tuple<string, int>> afc))
              {
                  var query = _context.Accounts.Where(x => x.Flags.Any()).Select(x => new { x.StarId, x.Flags }).ToList();
                  afc = new List<Tuple<string, int>>();

                  foreach (var c in query)
                  {
                      foreach (var d in c.Flags)
                      {
                          afc.Add(new Tuple<string, int>(c.StarId.Trim(), d.FlagId));
                      }
                  }
                  _cache.Set("accountFlags", afc, new MemoryCacheEntryOptions
                  {
                      AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1600)
                      // Set any cache options here
                  });
              }

              if (!_cache.TryGetValue("flags", out List<Tuple<string, string, int>> f))
              {
                  var query = _context.Flags.Select(x => new { id = x.FlagId, type = x.FlagType, name = x.FlagName });
                  f = new List<Tuple<string, string, int>>();

                  foreach (var c in query)
                  {
                      f.Add(new Tuple<string, string, int>(c.name, c.type, c.id));
                  }
                  _cache.Set("flags", f, new MemoryCacheEntryOptions
                  {
                      AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1600)
                      // Set any cache options here
                  });
              }
          }*/
        }
    }
}


