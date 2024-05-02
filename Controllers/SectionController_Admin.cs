using Microsoft.AspNetCore.Mvc;
using NewDotnet.Code;
using NewDotnet.Models;
using Newtonsoft.Json;

namespace NewDotnet.Controllers
{
    public partial class SectionController
    {
        [HttpGet]
        [Route("ListAll")]

     
        // This method retrieves a list of all sections. It does not include the "id" parameter, so it is executed if the caller does not specify an ID.
        public IActionResult ListAll()
        {

            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(User);

            // Perform the query.
            var allContentIdsInPlaylist = _context.Playlists.Where(x => x.PlaylistId == tc.Playlist).Where(x => x.ItemType == "c").Select(x => x.ItemId);
            var allSectionIdsInPlaylist = _context.Contents.Where(x => allContentIdsInPlaylist.Contains(x.ContentId)).Select(x => x.SectionId).Distinct().ToArray();

            var allSections = (from x in _context.Sections where allSectionIdsInPlaylist.Contains(x.SectionId) select x).ToList();

            var allQuestions = from x in _context.Questions where allSectionIdsInPlaylist.Contains(x.SectionId) select x.SectionId;

            var allQuestionIds = allQuestions.Distinct();

            var allSectionsWithQuizzes = from x in allSections
                                         select new
                                         {
                                             x.SectionId,
                                             x.SectionTitle,
                                             hasQuiz = (allQuestionIds.Contains(x.SectionId) ? 1 : 0)
                                         };

            // return the list to the client.
            return Ok(new { error = 0, data = allSectionsWithQuizzes });
        }

        [HttpPost]
        [Route("addnewSection")]

       
        public IActionResult NewSection([FromBody] Section s)
        {
            return UpdateSection(-1, s);
        }

        [HttpPost]
        [Route("updateSection/{id:int}")]

        public IActionResult UpdateSection(int id, [FromBody] Section s)
        {
            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(User);
            var m = new OODBModel(_context);

            // are we changing an existing ID?
            if (id == -1)
            {

                // insert the new section into the database
                _context.Sections.Add(s);

                // insert the new section into the auditlog
                m.LogAuditEvent("section/new", tc.StarId, "created new section " + s.SectionId.ToString(), null, JsonConvert.SerializeObject(s), false);
                _context.SaveChanges();
            }
            else
            {
                //update an existing record           
                // get this section ID from the database
                var sectionQuery = from x in _context.Sections where x.SectionId == id select x;
                if (!sectionQuery.Any())
                {
                    return NotFound( new
                    {
                        error = 404,
                        message =
                        $"No section with ID '{id}'."
                    });
                }
                var thisSection = sectionQuery.First();

                string beforeContent = JsonConvert.SerializeObject(thisSection);
                thisSection.SectionTitle = s.SectionTitle;

                m.LogAuditEvent("section/edit", tc.StarId, "edited section " + id.ToString(), beforeContent, JsonConvert.SerializeObject(thisSection), false);
                _context.SaveChanges();
            }

            var result = new
            {
                error = 0,
                data = new
                {
                    new_sectionId = (id == -1 ? s.SectionId : id),
                    new_sectionName = s.SectionTitle,
                }
            };

            return Ok( result);
        }

        [HttpDelete]
        [Route("deleteSection/{id:int}")]

      
        public IActionResult DeleteSection(int id)
        {
            BearerTokenContents tc = Services.GetTokenDataFromUserPrincipal(User);
            var m = new OODBModel(_context);

            var thisSectionQuery = from x in _context.Sections where x.SectionId == id select x;
            if (!thisSectionQuery.Any())
            {
                return NotFound( new
                {
                    error = 404,
                    message =
                    $"No section with ID '{id}'."
                });
            }

            // perform checks
            var checkContentQuery = from x in _context.Contents where x.SectionId == id select x;
            if (checkContentQuery.Count() != 0)
            {
                return BadRequest( new { error = 400, message = "Section cannot be deleted because content items are still associated with it. Please reassign or delete all content items assigned to this section before deleting it." });
            }
            var checkQuizQuery = from x in _context.Questions where x.SectionId == id select x;
            if (checkQuizQuery.Count() != 0)
            {
                return BadRequest(new { error = 400, message = "Section cannot be deleted because a quiz is still associated with it. Please delete the quiz for this section before deleting it." });
            }

            // we are good to remove
            // get the section
            var thisSection = thisSectionQuery.First();

            m.LogAuditEvent("section/delete", tc.StarId, "remove section " + thisSection.SectionId, JsonConvert.SerializeObject(thisSection), null, false);
            _context.Sections.Remove(thisSection);

           _context.SaveChanges();

            return Ok( new { error = 0 });
        }
    }
}
