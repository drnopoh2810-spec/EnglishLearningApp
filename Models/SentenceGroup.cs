using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningApp.Models
{
    public class SentenceGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string GroupName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual ICollection<SentenceGroupLink> SentenceLinks { get; set; } = new List<SentenceGroupLink>();

        [NotMapped]
        public int SentenceCount { get; set; }
    }
}
