using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningApp.Models
{
    public enum DifficultyLevel
    {
        Beginner = 1,
        Intermediate = 2,
        Advanced = 3,
        Expert = 4
    }

    public class Sentence
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string EnglishSentence { get; set; } = string.Empty;

        public string ArabicTranslation { get; set; } = string.Empty;

        public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Beginner;

        public double MasteryScore { get; set; } = 0;

        public int ReviewCount { get; set; } = 0;

        public DateTime? LastReviewDate { get; set; }

        public DateTime? NextReviewDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty;

        [MaxLength(500)]
        public string YouGlishUrl { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<SentenceGroupLink> GroupLinks { get; set; } = new List<SentenceGroupLink>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        [NotMapped]
        public string GroupNames { get; set; } = string.Empty;

        [NotMapped]
        public bool IsDue => NextReviewDate.HasValue && NextReviewDate.Value.Date <= DateTime.UtcNow.Date;
    }
}
