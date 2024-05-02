using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using NewDotnet.Code;
using NewDotnet.Context;
using NewDotnet.Models;



namespace NewDotnet.Controllers
{
    [ApiController]
    [Route("api/playlist")]
    public partial class PlaylistController : ControllerBase
    {
        private readonly OODBModelContext _context;

        // constructor will create new connection to model
        public PlaylistController(OODBModelContext context)
        {
           _context = context;
        }



        [HttpGet]
        
        [Route("listPlaylists")]
        public IActionResult listPlaylists()
        {
            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal((System.Security.Claims.ClaimsPrincipal)User);
            var m = new OODBModel(_context);

            // Guest code
            if (tc.GuestId != "")
            {
                var allPlaylists = _context.Playlists1.Select(x => new { id = x.PlaylistId, title = x.PlaylistTitle });
                m.LogAuditEvent("playlist/list", "guest:" + tc.GuestId, "listing all playlists.", true);
                return Ok( new
                {
                    error = 0,
                    data = allPlaylists
                });
            }

            var allPlaylistsForUser = from assignments in  _context.Assignments
                                      join playlist in _context.Playlists1 on assignments.AssignedPlaylist equals playlist.PlaylistId
                                      where assignments.StarId == tc.StarId
                                      select new
                                      {
                                          id = playlist.PlaylistId,
                                          title = playlist.PlaylistTitle
                                      };

            m.LogAuditEvent("playlist/list", tc.StarId, "listing all assigned playlists.", true);
            return Ok( new
            {
                error = 0,
                data = allPlaylistsForUser
            });

        }
        
       
    }
}
