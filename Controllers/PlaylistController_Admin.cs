using Microsoft.AspNetCore.Mvc;
using NewDotnet.Code;
using NewDotnet.Models;
using Newtonsoft.Json;
using System.Net;

namespace NewDotnet.Controllers
{
    public partial class PlaylistController
    {
        [HttpGet]
        [Route("getEntirePlaylist")]
        public IActionResult getEntirePlaylist()
        {
            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(User);
            var m = new OODBModel(_context);

            Dictionary<int, string> contentTitles = _context.Contents.Select(t => new { t.ContentId, t.ContentTitle }).ToDictionary(t => t.ContentId, t => t.ContentTitle);

            // Only show the current user's playlist.
            var entirePlaylist = m.GetExtendedPlaylist(tc.Playlist);

            var finalPlaylist = from x in entirePlaylist
                                select new
                                {
                                    x.PlaylistOrder,
                                    x.ItemType,
                                    x.ItemId,
                                    x.SectionId,
                                    x.SectionTitle,
                                    contentTitle = x.ItemType == "c" ? contentTitles[x.ItemId] : "",
                                    refCount = _context.Playlists.Where(y => y.ItemId == x.ItemId).Count(y => y.ItemType == "c")
                                };
            // now turn it into some arrays
            var groupedFinalPlaylist = finalPlaylist.GroupBy(t => t.SectionId).Select(t => t.ToArray()).ToArray();

            return Ok( new { error = 0, data = groupedFinalPlaylist });
        }

        [HttpPost]
        [Route("replacePlayList")]
        public IActionResult replacePlaylist(Playlist[] legacyNewPlaylist)
        {

            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(User);
            var m = new OODBModel(_context);

            Playlist[] newPlaylist = legacyNewPlaylist.Select(x => new Playlist
            {
                PlaylistOrder = x.PlaylistOrder,
                PlaylistId = tc.Playlist,
                ItemType = x.ItemType,
                ItemId = x.ItemId
            }).ToArray();

            // validate the submission
            foreach (Playlist p in newPlaylist)
            {
                if (p.ItemType.Length != 1)
                {
                    return BadRequest( new { error = 400, message = "Invalid playlist entries found. Please resubmit." });
                }
            }

            // sort by the playlist ID
            newPlaylist = (from x in newPlaylist orderby x.PlaylistOrder select x).ToArray();

            // insert the current and new playlists into the audit log
            Playlist[] currentPlaylist = (from x in _context.Playlists orderby x.PlaylistOrder select x).ToArray();
            string currentPlaylistString = JsonConvert.SerializeObject(currentPlaylist);
            string newPlaylistString = JsonConvert.SerializeObject(newPlaylist);
            m.LogAuditEvent("playlist/edit", tc.StarId, "replaced playlist", currentPlaylistString, newPlaylistString, false);

            // now the big guns
            _context.Playlists.RemoveRange(_context.Playlists);

            // now insert all new playlist records.
            _context.Playlists.AddRange(newPlaylist);

            // renumber just in case
            m.ExecuteStoredProcedure("renumberPlaylist", tc.Playlist.ToString());

            // and commit.
            _context.SaveChanges();

            return Ok( new { error = 0, data = new { playlistSize = newPlaylist.Length } });
        }
    }
}
