namespace NewDotnet.Code
{
       public class FullPlaylistItem
        {
        
            public int PlaylistId { get; set; }
            public int PlaylistOrder { get; set; }
            public string ItemType { get; set; }
            public int ItemId { get; set; }
            public int? SectionId { get; set; }
            public string SectionTitle { get; set; }
            public int IsFirst { get; set; }
            public int IsLast { get; set; }
        }

        public class QuizResponse
        {
            public string Seed { get; set; }
            public int[] Answers { get; set; }
        }

        public class QuestionUpdate
        {
            public string QuestionText { get; set; }
            public string[] QuestionAnswers { get; set; }
        }

        public class BearerTokenContents
        {
            public string StarId { get; set; }
            public string GuestId { get; set; }
            public DateTime Expiry { get; set; }
            public int Playlist { get; set; }
            public string KeyType { get; set; }
        }
 }


