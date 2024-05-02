using Microsoft.AspNetCore.Mvc;
using NewDotnet.Code;
using NewDotnet.Context;
using NewDotnet.DataLayer;
using NewDotnet.Models;
using System.Net;


namespace NewDotnet.Controllers
{
    [ApiController]
    [Route("api/content")]
    public partial class ContentController : ControllerBase
    {
        // Provides access to the data model

        private readonly Database db;
        public readonly OODBModelContext _context;
        private readonly OODBModel m;
        public ContentController(OODBModelContext context)
        {
            // Make a connection to the database and store it in the class variable.
           _context = context;
            m = new OODBModel(_context);
            db = new Database(_context);
        }
        [HttpGet]

        [Route("getOneContent/{id:int}")]
        public IActionResult getOneContentPage(int id)
        {
            
            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(User);

            ContentResponse cr;

            // If user is admin, allow ANY content retrieval.
            if (m.IsAdmin(tc.StarId))
            {
                var a_content = db.GetContent(id);
                if (a_content == null)
                {
                    return NotFound( new { error = 404, message = "No such content." });
                }
                return Ok( new { error = 0, data = a_content });
            }

            // If the user is a guest...
            if (tc.GuestId != "")
            {
                // Test if the guest exists.
                var guest = _context.Guests.Where(x => x.GuestId == tc.GuestId).FirstOrDefault();
                if (guest == null)
                {
                    return Unauthorized(new{ error = 401, message = "No such guest ID." });
                }

                // Guests can view anything--just make sure this content is actually in the playlist.
                var ep = m.GetExtendedPlaylist(tc.Playlist);

                if (ep.Any(x => x.ItemType == "c" && x.ItemId == id))
                {
                    // Return the content.
                    cr = db.GetContent(id);
                    var content = ep.First(x => x.ItemType == "c" && x.ItemId == id);
                    cr.isFirst = (content.IsFirst == 1);
                    cr.isLast = (content.IsLast == 1);
                    guest.CurrentPosition = content.PlaylistOrder;
                    guest.LastAction = DateTime.Now;
                    _context.SaveChanges();
                    return Ok( new { error = 0, data = cr });
                }
                else
                {
                    m.LogAuditEvent("content/get", (tc.GuestId == "" ? tc.StarId : "guest:" + tc.GuestId), $"denied request for content {id} because it does not exist.", true);
                    return NotFound( new { error = 404, error_message = "No such content." });
                }
            }

            Assignment playlistAssignment = _context.Assignments.Where(x => x.StarId == tc.StarId).Where(x => x.AssignedPlaylist == tc.Playlist).Select(x => x).FirstOrDefault();
            if (playlistAssignment == null)
            {
                return Unauthorized( new { error = 401, message = "No assignment for specified playlist." });
            }

            // Find this content in the playlist
            var item = m.GetUserSpecificPlaylist(tc.StarId, tc.Playlist)
                .Where(x => x.ItemType == "c" && x.PlaylistId == tc.Playlist && x.ItemId == id)
                .OrderBy(x => x.PlaylistOrder)
                .FirstOrDefault();

            if (item == null)
            {
                // This content item does not exist in the current user's playlist.
                // Return a 404 and log the incident.
                m.LogAuditEvent("content/get", tc.StarId, $"request for content {id} which exists but is not in playlist {tc.Playlist}.", true);
                return NotFound( new { error = 404, message = "No such content." });
            }

            var isFirst = item.IsFirst;
            var isLast = item.IsLast;

            int playlistId = item.PlaylistId;

            // If the user is NOT allowed to view everything, perform appropriate checks.
            var forceSequential = (db.GetConfigurationValue("forceSequential." + tc.Playlist.ToString()) ?? "1") == "1" ? true : false;
            if (!m.IsComplete(tc)) // OK to ignore guests because all guests should always be allowed to view all content.
            {

                // If we have a user, determine if that user can access any content, (View only users and admins)
                if (!m.HasUserFlags(tc.StarId, new[] { "siteadmin", "admin", "noprogress" }) && forceSequential)
                {

                    // ... the user can't, so... have they reached this point in the orientation?
                    if (playlistAssignment.CurrentProgress < playlistId)
                    {
                        // ... forbid the user from viewing this content.
                        m.LogAuditEvent("content/get", (tc.GuestId == "" ? tc.StarId : "guest:" + tc.GuestId), $"denied request for content {id} because it is at playlist position {playlistId} and user is only at {playlistAssignment.CurrentProgress}.", true);
                        return  BadRequest(new { error = 403, message = "You are not yet allowed to access this content." });
                    }
                }
            }

            // Everything has checked out so far. Let's actually retrieve the content now.
            // Get a reference to this content object.
            Content c = _context.Contents.Where(x => x.ContentId == id).Select(x => x).First();

            // If display flags says this user shouldn't see this content, block it (pretend it doesn't exist)
            if (!m.ShouldDisplayContent(tc.StarId, c.ContentId))
            {
                m.LogAuditEvent("content/get", (tc.GuestId == "" ? tc.StarId : "guest:" + tc.GuestId), $"denied request for content {id} because it should not be shown to the user based on their cohort memberships.", true);
                return NotFound( new { error = 404, error_message = "No such content." });
            }

            cr = db.GetContent(id);
            cr.isFirst = (isFirst == 1);
            cr.isLast = (isLast == 1);

            // update the user's current *position* (not progress)
            if (tc.GuestId == "")
            {
                playlistAssignment.CurrentPosition = playlistId;
            }
            else
            {
                var guestData = _context.Guests.FirstOrDefault(x => x.GuestId == tc.GuestId);
                if (guestData == null)
                {
                    m.LogAuditEvent("content/get", (tc.GuestId == "" ? tc.StarId : "guest:" + tc.GuestId), "request by unknown guest user, ignoring.", true);
                    return NotFound( new { error = 401, error_message = "Authentication required." });
                }
                guestData.CurrentPosition = playlistId;
                guestData.LastAction = DateTime.Now;
            }

            m.LogAuditEvent("content/get", (tc.GuestId == "" ? tc.StarId : "guest:" + tc.GuestId), string.Format("retrieved content for slide {0}: \"{1}\".", new object[] { c.ContentId, c.ContentTitle }));

            _context.SaveChanges();

            // Return the result
            return Ok(new { error = 0, data = cr });
        }
    }
}
