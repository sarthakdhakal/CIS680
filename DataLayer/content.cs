using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Markdig;

namespace NewDotnet.DataLayer
{
    public partial class Database
    {
        // Unconditionally return a piece of content
        public ContentResponse GetContent(int id)
        {
            var content = _context.Contents.FirstOrDefault(x => x.ContentId == id);
            if (content == null) return null;

            string htmlContent;

            // Special case code handlers
            if (content.ContentData.StartsWith("@video:"))
            {
                // This slide contains a video. Return appropriate video tag.
                string videoUrl = content.ContentData.Substring(7);
                htmlContent = $"<p><a href=\"{videoUrl}\">Click to play Video</a></p>";
            }
            else if (content.ContentData.StartsWith("@youtube:"))
            {
                // This slide contains a video. Return appropriate video tag.
                string videoUrl = content.ContentData.Substring(7);
                htmlContent = $"<p><a href=\"https://www.youtube.com/watch?v={videoUrl}\">Click to play YouTube Video</a></p>";
            }
            else if (content.ContentData.StartsWith("@raw:"))
            {
                // This is raw content. Send it as is.
                htmlContent = content.ContentData.Substring(5);
            }
            else
            {
                htmlContent = Markdown.ToHtml(content.ContentData);
                // Add the img-responsive class
                // TODO: look into a better way to do this (maybe a framework?)
                htmlContent = htmlContent.Replace("<img ", "<img class=\"img-responsive\" ");
            }

            return new ContentResponse
            {
                id = id,
                section = content.Section.SectionTitle,
                section_id = content.Section.SectionId,
                title = content.ContentTitle,
                image = content.HeaderImage,
                md_content = content.ContentData,
                html_content = htmlContent,
                cohort_id_list = content.Flags.Where(x => x.FlagType == "cohort").Select(x => x.FlagId).ToArray()
            };
        }
    }

    public class ContentResponse
    {
        public int id { get; set; }
        public string section { get; set; }
        public int section_id { get; set; }
        public string title { get; set; }
        public string image { get; set; }
        public string md_content { get; set; }
        public string html_content { get; set; }
        public bool isFirst { get; set; }
        public bool isLast { get; set; }
        public int[] cohort_id_list { get; set; }
    }
}
