using Microsoft.EntityFrameworkCore;
using EnglishLearningApp.Models;
using System.IO;

namespace EnglishLearningApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Sentence> Sentences { get; set; } = null!;
        public DbSet<SentenceGroup> SentenceGroups { get; set; } = null!;
        public DbSet<SentenceGroupLink> SentenceGroupLinks { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;

        private readonly string _dbPath;

        public AppDbContext()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(folder, "EnglishLearningApp");
            Directory.CreateDirectory(appFolder);
            _dbPath = Path.Combine(appFolder, "english_learning.db");
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            _dbPath = "english_learning.db";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Data Source={_dbPath}");
                optionsBuilder.EnableSensitiveDataLogging(false);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure indexes for performance
            modelBuilder.Entity<Sentence>()
                .HasIndex(s => s.EnglishSentence);

            modelBuilder.Entity<Sentence>()
                .HasIndex(s => s.NextReviewDate);

            modelBuilder.Entity<Sentence>()
                .HasIndex(s => s.MasteryScore);

            modelBuilder.Entity<Sentence>()
                .HasIndex(s => s.DifficultyLevel);

            modelBuilder.Entity<Sentence>()
                .HasIndex(s => s.ArabicTranslation);

            modelBuilder.Entity<SentenceGroup>()
                .HasIndex(g => g.GroupName);

            modelBuilder.Entity<Review>()
                .HasIndex(r => r.SentenceId);

            modelBuilder.Entity<Review>()
                .HasIndex(r => r.ReviewDate);

            modelBuilder.Entity<SentenceGroupLink>()
                .HasIndex(l => l.SentenceId);

            modelBuilder.Entity<SentenceGroupLink>()
                .HasIndex(l => l.GroupId);

            modelBuilder.Entity<SentenceGroupLink>()
                .HasIndex(l => new { l.SentenceId, l.GroupId })
                .IsUnique();

            // Seed default groups
            modelBuilder.Entity<SentenceGroup>().HasData(
                new SentenceGroup { Id = 1, GroupName = "Movies", Description = "Sentences from movies and TV shows", CreatedDate = DateTime.UtcNow },
                new SentenceGroup { Id = 2, GroupName = "Daily English", Description = "Everyday conversational English", CreatedDate = DateTime.UtcNow },
                new SentenceGroup { Id = 3, GroupName = "Special Education", Description = "Special education terminology", CreatedDate = DateTime.UtcNow },
                new SentenceGroup { Id = 4, GroupName = "ABA", Description = "Applied Behavior Analysis terms", CreatedDate = DateTime.UtcNow },
                new SentenceGroup { Id = 5, GroupName = "Interviews", Description = "Job interview preparation", CreatedDate = DateTime.UtcNow },
                new SentenceGroup { Id = 6, GroupName = "Travel", Description = "Travel and tourism phrases", CreatedDate = DateTime.UtcNow }
            );

            // Seed default user
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Name = "Learner", CreatedDate = DateTime.UtcNow }
            );
        }
    }
}
