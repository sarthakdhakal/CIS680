using Microsoft.AspNetCore.Mvc;
using NewDotnet.Code;
using NewDotnet.Context;
using NewDotnet.DataLayer;
using NewDotnet.Models;



namespace NewDotnet.Controllers
{
    [ApiController]
    [Route("api/section")]
    public partial class SectionController : ControllerBase
    {
        private readonly OODBModelContext _context;
        // Constructor
        public SectionController(OODBModelContext context)
        {
            _context = context;
        }

        [HttpGet]
        // This method retrieves a list of all sections. It does not include the "id" parameter, so it is executed if the caller does not specify an ID.
        public IActionResult GetAllSectionsFromPlaylist()
        {
            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(User);
            Assignment ass = null;
            var m = new OODBModel(_context);

            List<FullPlaylistItem> entirePlaylist;

            // If a guest is requesting, return all sections from entire playlist.
            // Also, if the playlist has the "forceSequential" flag set to false, do the same.
            var forceSequential = (new Database(_context).GetConfigurationValue("forceSequential." + tc.Playlist.ToString()) ?? "1") == "1" ? true : false;
            if (tc.GuestId != "" || !forceSequential)
            {
                entirePlaylist = m.GetExtendedPlaylist(tc.Playlist)
                .Where(x => x.PlaylistId == tc.Playlist)
                .Where(x => x.ItemType == "c")
                .OrderBy(x => x.PlaylistOrder)
                .ToList();
            }
            else
            {
                // Get every content ID from the user's custom playlist 
                // This will take into account cohorts and admin status.
                entirePlaylist = m.GetUserSpecificPlaylist(tc.StarId, tc.Playlist)
                    .Where(x => x.PlaylistId == tc.Playlist)
                    .Where(x => x.ItemType == "c")
                    .OrderBy(x => x.PlaylistOrder)
                    .ToList();
                // get user assignment - this will be used to populate the "allowed" field in the return values
                ass = _context.Assignments.FirstOrDefault(x => x.StarId == tc.StarId && x.AssignedPlaylist == tc.Playlist);
            }

            // Now, get section IDs into a list, one per content slide
            // Filter the list to eliminate sequences of section IDs.
            // If a section ID appears more than once we have a playlist inconsistency, by the way.
            var sectionIds = entirePlaylist.Select(x => (int)x.SectionId).ToList().ExcludeConsecutiveDuplicates().ToArray();

            // Now, get all the sections.
            List<Section> sections = new List<Section>();
            foreach (int i in sectionIds)
            {
                sections.Add(_context.Sections.Where(x => x.SectionId == i).First());
            }

            // For each item in the section list, determine the first playlist position and the first content ID.
            // Add that to a new object list for return to the user.
            List<object> returnList = new List<object>();

            foreach (Section s in sections)
            {
                var firstItem = entirePlaylist
                    .Where(x => x.SectionId == s.SectionId)
                    .Where(x => x.ItemType == "c")
                    .OrderBy(x => x.PlaylistOrder)
                    .First();
                int firstContent = firstItem.ItemId;
                int firstOrder = firstItem.PlaylistOrder;
                returnList.Add(new
                {
                    s.SectionId,
                    s.SectionTitle,
                    allowed = (ass != null && ass.CurrentProgress < firstOrder ? 0 : 1),
                    firstContent,
                    firstPlaylist = firstOrder
                });
            }
            m.LogAuditEvent("section/list", tc.GuestId != "" ? $"guest:{tc.GuestId}" : tc.StarId, "retrieved all section IDs.", true);

            return Ok( new{ error = 0, data = returnList });
        }


        [HttpGet]
      
        [Route("getOneSection/{id:int}")]
        // This method retrieves the content IDs for a single section. It is executed if the user provides an ID (e.g. /api/section/1)
        public IActionResult GetOneSection(int id)
        {
            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(User);
            Assignment ass = null;
            var m = new OODBModel(_context);

            // New code 3/8/2018 - Much more efficient and less messy.

            // Get user full playlist
            FullPlaylistItem[] ep;

            var forceSequential = (new Database(_context).GetConfigurationValue("forceSequential." + tc.Playlist.ToString()) ?? "1") == "1" ? true : false;
            if (tc.GuestId != "" || !forceSequential)
            {
                ep =  m.GetExtendedPlaylist(tc.Playlist);
            }
            else
            {
                ep = m.GetUserSpecificPlaylist(tc.StarId, tc.Playlist);
                ass =_context.Assignments.First(x => x.StarId == tc.StarId && x.AssignedPlaylist == tc.Playlist);
            }

            // Get user information

            // Filter the playlist by section ID
            var ep_filtered = ep.Where(x => x.SectionId == id).Where(x => x.ItemType == "c").OrderBy(x => x.PlaylistOrder).Join(_context.Contents, x => x.ItemId, y => y.ContentId, (p, c) => new
            {
                id = c.ContentId,
                title = c.ContentTitle,
                allowed = ass != null && p.PlaylistOrder > ass.CurrentProgress ? 0 : 1
            });

            // Test for a quiz
            bool has_quiz = _context.Questions.Any(x => x.SectionId == id);

            m.LogAuditEvent("section/get", tc.GuestId != "" ? $"guest:{tc.GuestId}" : tc.StarId, string.Format("retrieved content IDs for section {0}.", new object[] { id }), true);

            // Return the content IDs and whether a quiz exists to the caller.
            return Ok(new {
                error = 0,
                data = new
                {
                    contents = ep_filtered,
                    has_quiz = (has_quiz ? "yes" : "no")
                }
            });
        }
    }


}
