using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

namespace NewDotnet.Models
{
    [Table("playlists")]
    public class Playlists
    {
        public Playlists()
        {
            Configuration = new HashSet<Configuration>();
            Guest = new HashSet<Guest>();
            Playlist = new HashSet<Playlist>();
            ;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PlaylistId { get; set; }

        [Required]
        [StringLength(64)]
        public string PlaylistTitle { get; set; }

        [JsonIgnore] // Using System.Text.Json
        public virtual ICollection<Configuration> Configuration { get; set; }

        [JsonIgnore] // Using System.Text.Json
        public virtual ICollection<Guest> Guest { get; set; }

        [JsonIgnore] // Using System.Text.Json
        public virtual ICollection<Playlist> Playlist { get; set; }

        [JsonIgnore] // Using System.Text.Json
        public virtual ICollection<CohortPrompt> CohortPrompt { get; set; }
    }

}
