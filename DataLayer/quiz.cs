using NewDotnet.Models;

namespace NewDotnet.DataLayer
{
   
        public partial class Database
        {

            public void EnsureQuizIsNotInPlaylist(int playlistId, int sectionId)
            {
                // TODO: When we update to the newer database schema, replace this code.
                // This code is a kludge to ensure a quiz is not in the playlist after it has been deleted, to prevent an error.

                // look for and remove the quiz from anywhere in the playlist ahead of this
                var anyQuiz = _context.Playlists.Where(x => x.ItemType == "q" && x.PlaylistId == playlistId && x.ItemId == sectionId);
                _context.Playlists.RemoveRange(anyQuiz);
               _context.SaveChanges();

                RenumberPlaylist(playlistId);

            }
            public void EnsureQuizIsInPlaylist(int playlistId, int sectionId)
            {
                // TODO: When we update to the newer database schema, replace this code.
                // This code is a kludge to allow new quizzes to be properly registered in the playlist without requiring a schema update.

                // first get a list of all content items in the playlist
                var allContentPlItems = _context.Playlists
                    .Where(x => x.ItemType == "c" && x.PlaylistId == playlistId)
                    .Join(_context.Contents, pl => pl.ItemId, c => c.ContentId, (pl, c) => new { order = pl.PlaylistOrder, id = c.ContentId, section = c.SectionId });

                // determine LAST item in playlist belonging to desired section
                var lastSectionItem = allContentPlItems.Where(x => x.section == sectionId).OrderByDescending(x => x.order).FirstOrDefault();

                if (lastSectionItem == null) throw new InvalidOperationException("Section does not have any items in the playlist. Cannot proceed.");

                // get order of last section item
                int highestOrder = lastSectionItem.order;

                // look for and remove the quiz from anywhere in the playlist ahead of this
                var anyQuiz = _context.Playlists.Where(x => x.ItemType == "q" && x.PlaylistId == playlistId && x.ItemId == sectionId);
                _context.Playlists.RemoveRange(anyQuiz);

                // add one to the highest order value and insert the quiz
                _context.Playlists.Add(new Playlist
                {
                    ItemId = sectionId,
                    PlaylistOrder = highestOrder + 1,
                    ItemType = "q",
                    PlaylistId = playlistId
                });

                // save
                _context.SaveChanges();

                RenumberPlaylist(playlistId);

                // all finished.
            }
        }
    
}
