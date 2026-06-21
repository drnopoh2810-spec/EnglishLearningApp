using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningApp.Models
{
    public enum ReviewRating
    {
        Again = 1,
        Hard = 2,
        Good = 3,
        Easy = 4
    }

    public class Review
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int SentenceId { get; set; }

        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

        public ReviewRating Rating { get; set; }

        public DateTime NextReviewDate { get; set; }

        [ForeignKey("SentenceId")]
        public virtual Sentence Sentence { get; set; } = null!;
    }
}
