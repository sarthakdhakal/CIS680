using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NewDotnet.Models
{
    [Table("analytics")]
    public class Analytics
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(8)]
        public string StarId { get; set; }

        [StringLength(8)]
        public string TechId { get; set; }

        [Column(Order = 1)]
        [StringLength(512)]
        public string Name { get; set; }

        public DateTime? EndTime { get; set; }

        public int? IsMilitary { get; set; }

       
        [Column(Order = 2)]
        [StringLength(12)]
        public string Location { get; set; }

    
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int AssignedPlaylist { get; set; }

        [Column(Order = 4)]
        [StringLength(64)]
        public string AssignedPlaylistTitle { get; set; }

        public int? QuizAttemptCount { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? QuizSuccessRate { get; set; }
    }
}
