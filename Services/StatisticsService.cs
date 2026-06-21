using EnglishLearningApp.Data;
using EnglishLearningApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningApp.Services
{
    public class StatisticsService
    {
        private readonly AppDbContext _context;

        public StatisticsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatistics> GetDashboardStatisticsAsync()
        {
            var today = DateTime.UtcNow.Date;

            var totalSentences = await _context.Sentences.CountAsync();
            var masteredSentences = await _context.Sentences.CountAsync(s => s.MasteryScore >= 80);
            var sentencesDueToday = await _context.Sentences.CountAsync(s => s.NextReviewDate == null || s.NextReviewDate.Value.Date <= today);
            var sentencesLearned = await _context.Sentences.CountAsync(s => s.ReviewCount > 0);
            var averageMastery = totalSentences > 0 ? await _context.Sentences.AverageAsync(s => s.MasteryScore) : 0;
            var currentStreak = await CalculateStreakAsync();
            var reviewsToday = await _context.Reviews.CountAsync(r => r.ReviewDate.Date == today);
            var totalReviews = await _context.Reviews.CountAsync();

            // Difficulty distribution
            var difficultyDistribution = await _context.Sentences
                .GroupBy(s => s.DifficultyLevel)
                .Select(g => new DifficultyStat
                {
                    Level = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            // Weekly activity
            var weeklyActivity = await GetWeeklyActivityAsync();

            // Mastery distribution
            var masteryDistribution = new List<MasteryRangeStat>
            {
                new MasteryRangeStat { Range = "0-25%", Count = await _context.Sentences.CountAsync(s => s.MasteryScore >= 0 && s.MasteryScore < 25) },
                new MasteryRangeStat { Range = "25-50%", Count = await _context.Sentences.CountAsync(s => s.MasteryScore >= 25 && s.MasteryScore < 50) },
                new MasteryRangeStat { Range = "50-75%", Count = await _context.Sentences.CountAsync(s => s.MasteryScore >= 50 && s.MasteryScore < 75) },
                new MasteryRangeStat { Range = "75-100%", Count = await _context.Sentences.CountAsync(s => s.MasteryScore >= 75 && s.MasteryScore <= 100) }
            };

            // Group statistics
            var groupStats = await _context.SentenceGroups
                .Select(g => new GroupStat
                {
                    GroupName = g.GroupName,
                    SentenceCount = g.SentenceLinks.Count,
                    AverageMastery = g.SentenceLinks.Any() ? g.SentenceLinks.Average(sl => sl.Sentence.MasteryScore) : 0
                })
                .ToListAsync();

            return new DashboardStatistics
            {
                TotalSentences = totalSentences,
                MasteredSentences = masteredSentences,
                SentencesDueToday = sentencesDueToday,
                SentencesLearned = sentencesLearned,
                AverageMastery = Math.Round(averageMastery, 1),
                CurrentStreak = currentStreak,
                ReviewsToday = reviewsToday,
                TotalReviews = totalReviews,
                DifficultyDistribution = difficultyDistribution,
                WeeklyActivity = weeklyActivity,
                MasteryDistribution = masteryDistribution,
                GroupStatistics = groupStats
            };
        }

        public async Task<List<ActivityDay>> GetWeeklyActivityAsync(int days = 14)
        {
            var result = new List<ActivityDay>();
            var today = DateTime.UtcNow.Date;

            for (int i = days - 1; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var count = await _context.Reviews.CountAsync(r => r.ReviewDate.Date == date);
                result.Add(new ActivityDay
                {
                    Date = date,
                    Count = count,
                    DayName = date.ToString("ddd")
                });
            }

            return result;
        }

        public async Task<List<RecentReview>> GetRecentReviewsAsync(int count = 10)
        {
            return await _context.Reviews
                .Include(r => r.Sentence)
                .OrderByDescending(r => r.ReviewDate)
                .Take(count)
                .Select(r => new RecentReview
                {
                    SentenceText = r.Sentence.EnglishSentence,
                    Rating = r.Rating.ToString(),
                    ReviewDate = r.ReviewDate,
                    MasteryAfter = r.Sentence.MasteryScore
                })
                .ToListAsync();
        }

        private async Task<int> CalculateStreakAsync()
        {
            var today = DateTime.UtcNow.Date;
            int streak = 0;

            // Check if reviewed today
            var reviewedToday = await _context.Reviews.AnyAsync(r => r.ReviewDate.Date == today);
            if (!reviewedToday)
            {
                // Check if reviewed yesterday to maintain streak
                var reviewedYesterday = await _context.Reviews.AnyAsync(r => r.ReviewDate.Date == today.AddDays(-1));
                if (!reviewedYesterday)
                    return 0;
            }
            else
            {
                streak = 1;
            }

            // Count backward
            int daysBack = reviewedToday ? 1 : 2;
            while (true)
            {
                var dateToCheck = today.AddDays(-daysBack);
                var hasReview = await _context.Reviews.AnyAsync(r => r.ReviewDate.Date == dateToCheck);

                if (hasReview)
                {
                    streak++;
                    daysBack++;
                }
                else
                {
                    break;
                }
            }

            return streak;
        }
    }

    public class DashboardStatistics
    {
        public int TotalSentences { get; set; }
        public int MasteredSentences { get; set; }
        public int SentencesDueToday { get; set; }
        public int SentencesLearned { get; set; }
        public double AverageMastery { get; set; }
        public int CurrentStreak { get; set; }
        public int ReviewsToday { get; set; }
        public int TotalReviews { get; set; }
        public List<DifficultyStat> DifficultyDistribution { get; set; } = new();
        public List<ActivityDay> WeeklyActivity { get; set; } = new();
        public List<MasteryRangeStat> MasteryDistribution { get; set; } = new();
        public List<GroupStat> GroupStatistics { get; set; } = new();
    }

    public class DifficultyStat
    {
        public string Level { get; set; } = "";
        public int Count { get; set; }
    }

    public class ActivityDay
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public string DayName { get; set; } = "";
    }

    public class MasteryRangeStat
    {
        public string Range { get; set; } = "";
        public int Count { get; set; }
    }

    public class GroupStat
    {
        public string GroupName { get; set; } = "";
        public int SentenceCount { get; set; }
        public double AverageMastery { get; set; }
    }

    public class RecentReview
    {
        public string SentenceText { get; set; } = "";
        public string Rating { get; set; } = "";
        public DateTime ReviewDate { get; set; }
        public double MasteryAfter { get; set; }
    }
}
