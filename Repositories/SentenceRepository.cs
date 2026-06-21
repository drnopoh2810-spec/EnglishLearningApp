using Microsoft.EntityFrameworkCore;
using EnglishLearningApp.Data;
using EnglishLearningApp.Models;

namespace EnglishLearningApp.Repositories
{
    public class SentenceRepository : GenericRepository<Sentence>
    {
        public SentenceRepository(AppDbContext context) : base(context) { }

        public async Task<Sentence?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Sentences
                .Include(s => s.GroupLinks)
                .ThenInclude(gl => gl.Group)
                .Include(s => s.Reviews)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Sentence>> GetAllWithDetailsAsync()
        {
            return await _context.Sentences
                .Include(s => s.GroupLinks)
                .ThenInclude(gl => gl.Group)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Sentence>> GetDueSentencesAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Sentences
                .Include(s => s.GroupLinks)
                .ThenInclude(gl => gl.Group)
                .Where(s => s.NextReviewDate == null || s.NextReviewDate.Value.Date <= today)
                .OrderBy(s => s.NextReviewDate)
                .ThenBy(s => s.MasteryScore)
                .ToListAsync();
        }

        public async Task<IEnumerable<Sentence>> GetSentencesByGroupAsync(int groupId)
        {
            return await _context.Sentences
                .Include(s => s.GroupLinks)
                .ThenInclude(gl => gl.Group)
                .Where(s => s.GroupLinks.Any(gl => gl.GroupId == groupId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Sentence>> SearchSentencesAsync(string? searchText = null, int? groupId = null, DifficultyLevel? difficulty = null)
        {
            var query = _context.Sentences
                .Include(s => s.GroupLinks)
                .ThenInclude(gl => gl.Group)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.ToLower();
                query = query.Where(s =>
                    s.EnglishSentence.ToLower().Contains(searchText) ||
                    s.ArabicTranslation.ToLower().Contains(searchText));
            }

            if (groupId.HasValue)
            {
                query = query.Where(s => s.GroupLinks.Any(gl => gl.GroupId == groupId.Value));
            }

            if (difficulty.HasValue)
            {
                query = query.Where(s => s.DifficultyLevel == difficulty.Value);
            }

            return await query.OrderByDescending(s => s.CreatedDate).ToListAsync();
        }

        public async Task<IEnumerable<Sentence>> GetOverdueSentencesAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Sentences
                .Where(s => s.NextReviewDate.HasValue && s.NextReviewDate.Value.Date < today)
                .ToListAsync();
        }

        public async Task<int> GetDueCountAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Sentences
                .CountAsync(s => s.NextReviewDate == null || s.NextReviewDate.Value.Date <= today);
        }

        public async Task<int> GetMasteredCountAsync()
        {
            return await _context.Sentences
                .CountAsync(s => s.MasteryScore >= 80);
        }

        public async Task<double> GetAverageMasteryAsync()
        {
            var count = await _context.Sentences.CountAsync();
            if (count == 0) return 0;
            return await _context.Sentences.AverageAsync(s => s.MasteryScore);
        }

        public async Task<IEnumerable<Sentence>> GetNewSentencesAsync(int count)
        {
            return await _context.Sentences
                .Where(s => s.ReviewCount == 0)
                .OrderByDescending(s => s.CreatedDate)
                .Take(count)
                .ToListAsync();
        }
    }
}
