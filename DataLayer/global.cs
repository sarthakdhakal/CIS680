using NewDotnet.Context;
using NewDotnet.Models;

namespace NewDotnet.DataLayer
{
    public partial class Database
    {
        public readonly OODBModelContext _context;
        public  OODBModel m;
    
        public Database(OODBModelContext oodbModelContext)
        {

            _context = oodbModelContext;
            m = new OODBModel(_context);
        }

        public void RenumberPlaylist(int playlistId)
        {
            var currentPlaylist = _context.Playlists.Where(x => x.PlaylistId == playlistId).OrderBy(x => x.PlaylistOrder);
            var currentPlaylistStore = currentPlaylist.ToArray();

            _context.Playlists.RemoveRange(currentPlaylist);

            int orderCounter = 100;
            foreach (var cpi in currentPlaylistStore)
            {
                _context.Playlists.Add(new Playlist
                {
                    ItemId = cpi.ItemId,
                    PlaylistId = playlistId,
                    ItemType = cpi.ItemType,
                    PlaylistOrder = orderCounter
                });
                orderCounter += 100;
            }
            _context.SaveChanges();

        }
        public void CreateUser(string userId, string firstName, string lastName)
        {

            Account a = new Account
            {
                StarId = userId,
                FirstName = firstName,
                LastName = lastName
            };

          

            // Add default cohort
            // Try to get the cohort
            string defaultCohort = "x";
            Flag f = _context.Flags.Where(x => x.FlagName == defaultCohort).FirstOrDefault();
            if (f != null)
            {
                a.Flags.Add(f);
            }

            _context.Accounts.Add(a);

            _context.SaveChanges();
        }

        // locally defined functions
        public void LogAuditEvent(string source, string starId, string message, string before, string after)
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
            _context.SaveChanges();
        }
        public string GetConfigurationValue(string valueId)
        {
            Configuration c = _context.Configurations.FirstOrDefault(x => x.OptionShortName == valueId);
            if (c == null) return null;
            return c.OptionValue;
        }
        public void SetConfigurationValue(string valueId, string value)
        {
            Configuration c = _context.Configurations.FirstOrDefault(x => x.OptionShortName == valueId);
            if (c == null)
            {
                // Option doesn't exist, add it.
                _context.Configurations.Add(new Configuration
                {
                    OptionKind = "string",
                    OptionShortName = valueId,
                    OptionDescription = "(Setting automatically added by database engine)",
                    OptionValue = value,
                });
            }
            else
            {
                c.OptionValue = value;
            }
            _context.SaveChanges();
        }

        public void LogAuditEvent(string source, string starId, string message) { LogAuditEvent(source, starId, message, null, null); }
        public void LogAuditEvent(string source, string starId, string message, bool commitNow) { LogAuditEvent(source, starId, message, null, null); }

    }
}
