using Microsoft.EntityFrameworkCore;
using EnglishLearningApp.Data;
using EnglishLearningApp.Models;

namespace EnglishLearningApp.Repositories
{
    public class ReviewRepository : GenericRepository<Review>
    {
        public ReviewRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Review>> GetReviewsBySentenceAsync(int sentenceId)
        {
            return await _context.Reviews
                .Where(r => r.SentenceId == sentenceId)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();
        }

        public async Task<Review?> GetLatestReviewAsync(int sentenceId)
        {
            return await _context.Reviews
                .Where(r => r.SentenceId == sentenceId)
                .OrderByDescending(r => r.ReviewDate)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetReviewCountForDateAsync(DateTime date)
        {
            return await _context.Reviews
                .CountAsync(r => r.ReviewDate.Date == date.Date);
        }

        public async Task<IEnumerable<Review>> GetRecentReviewsAsync(int count = 50)
        {
            return await _context.Reviews
                .Include(r => r.Sentence)
                .OrderByDescending(r => r.ReviewDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetCurrentStreakAsync()
        {
            var today = DateTime.UtcNow.Date;
            int streak = 0;

            while (true)
            {
                var dateToCheck = today.AddDays(-streak);
                var hasReview = await _context.Reviews
                    .AnyAsync(r => r.ReviewDate.Date == dateToCheck);

                if (hasReview)
                    streak++;
                else
                    break;
            }

            return streak;
        }

        public async Task<int> GetReviewsTodayAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Reviews
                .CountAsync(r => r.ReviewDate.Date == today);
        }

        public async Task<int> GetTotalReviewCountAsync()
        {
            return await _context.Reviews.CountAsync();
        }

        public async Task DeleteAllReviewsForSentenceAsync(int sentenceId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.SentenceId == sentenceId)
                .ToListAsync();

            if (reviews.Any())
            {
                _context.Reviews.RemoveRange(reviews);
                await _context.SaveChangesAsync();
            }
        }
    }
}
