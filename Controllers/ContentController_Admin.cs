using Microsoft.AspNetCore.Mvc;
using NewDotnet.Code;
using NewDotnet.Models;
using Newtonsoft.Json;


namespace NewDotnet.Controllers
{
   
    public partial class ContentController
    {

        [HttpPost]
       
        [Route("updateContent/{id:int}")]
        public IActionResult UpdateContent(int id, [FromBody] Content c)
        {

            // The client should pass a value of "-1" for id if this content is a new slide.
            var m = new OODBModel(_context);

            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(User);

            // check to make sure the desired section exists
            var checkForSection = from x in _context.Sections where x.SectionId == c.SectionId select x;
            if (!checkForSection.Any())
            {
                return NotFound( new
                {
                    error = 400,
                    message =
                    $"Content belongs to section ID {c.SectionId}, which does not exist found."
                });
            }

            // are we changing an existing ID?
            if (id == -1)
            {
                m.LogAuditEvent("content/new", tc.StarId, "adding new content ID " + c.ContentId.ToString(), null, JsonConvert.SerializeObject(c), false);
                c.HeaderImage = "assets/images/default.png";
                // insert the new slide into the database
                _context.Contents.Add(c);
                _context.SaveChanges();
            }
            else
            {
                // test to see if content exists
                var checkForContent = from x in   _context.Contents where x.ContentId == id select x;
                if (!checkForContent.Any())
                {
                    return NotFound( new
                    {
                        error = 404,
                        message =
                        $"No content with ID {id} found."
                    });
                }

                // Content does exist.
                // Get the current content stored in the database into an object.
                var newContent = checkForContent.First();

                // Store existing content JSON data into the audit log
                string beforeContent = JsonConvert.SerializeObject(newContent);

                // set new content variables from submission
                newContent.SectionId = c.SectionId;
                newContent.ContentTitle = c.ContentTitle;
                newContent.ContentData = c.ContentData;
                //newContent.headerImage = c.headerImage;

                // Store item in audit log and update database
                m.LogAuditEvent("content/edit", tc.StarId, "edited content ID " + newContent.ContentId, beforeContent, JsonConvert.SerializeObject(newContent), false);
                _context.SaveChanges();
            }

            // We've completed inserting the content. Return the new data to the frontend.
            var result = new
            {
                error = 0,
                data = c
            };

            return Ok( result);

        }
        [HttpPut]
        
        [Route("newContent")]
        public IActionResult NewContent([FromBody] Content c)
        {
            // Calls the update content method with an id of -1 to indicate a new slide is to be created.
            return UpdateContent(-1, c);
        }
        [HttpDelete]

        [Route("deleteContent/{id:int}")]
        public IActionResult DeleteContent(int id)
        {
            // WARNING: deleting content has no confirmation at the backend!
            // The frontend should provide appropriate confirmation before invoking this API!
            var m = new OODBModel(_context);

            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal((System.Security.Claims.ClaimsPrincipal)User);

            // Select this entry from the playlist.
            var playlistEntry =
                from x in _context.Playlists
                where x.ItemType == "c" && x.PlaylistId == tc.Playlist && x.ItemId == id
                select x;

            if (playlistEntry.Any())
            {
                // It should not happen that we have more than one copy of an entry.
                // But, hey, may as well do this just in case.
                foreach (var pe in playlistEntry)
                {
                    _context.Playlists.Remove(pe);
                }
            }

            // Ok, now it's time to get this particular content entry from the database.
            var contentEntry = from x in  _context.Contents where x.ContentId == id select x;
            if (!contentEntry.Any())
            {
                // The content entry requested wasn't found.
                return NotFound( new
                {
                    error = 404,
                    message =
                    $"No content with ID {id} found."
                });
            }

            // We're about to delete the content, so, log its current JSON representation first.
            m.LogAuditEvent("content/delete", tc.StarId, "deleted content " + id.ToString(), JsonConvert.SerializeObject(contentEntry.First()), null, false);

            // Remove the content!
            _context.Contents.Remove(contentEntry.First());
            _context.SaveChanges();

            // Let's also renumber the playlist at this point.
            m.ExecuteStoredProcedure("renumberPlaylist", tc.Playlist.ToString());
            return Ok( new { error = 0 });

        }

        // Method to get ALL content as admin
        [HttpGet]
        [Route("getAllContents")]
        public IActionResult GetAllContent()
        {
            IEnumerable<Content> allContent;
            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal((System.Security.Claims.ClaimsPrincipal)User);
            try
            {


                 allContent = _context.Contents
        .Join(
            _context.Playlists.Where(p => p.PlaylistId == tc.Playlist && p.ItemType == "c"),
            content => content.ContentId,
            playlist => playlist.ItemId,
            (content, playlist) => content)
        .OrderBy(c => c.SectionId)
        .ThenBy(c => c.ContentId)
        .Select(c => c);
            }
            catch (Exception ex)
            {
               return BadRequest( new
               {
                error = ex.Message
               });
            }
            {

            }
            return Ok( new { error = 0, data = allContent });

        }
    }
  
}
