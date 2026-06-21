using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningApp.Models
{
    public class SentenceGroupLink
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int SentenceId { get; set; }

        [Required]
        public int GroupId { get; set; }

        [ForeignKey("SentenceId")]
        public virtual Sentence Sentence { get; set; } = null!;

        [ForeignKey("GroupId")]
        public virtual SentenceGroup Group { get; set; } = null!;
    }
}
