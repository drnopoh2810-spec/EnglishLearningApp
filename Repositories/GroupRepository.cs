using Microsoft.EntityFrameworkCore;
using EnglishLearningApp.Data;
using EnglishLearningApp.Models;

namespace EnglishLearningApp.Repositories
{
    public class GroupRepository : GenericRepository<SentenceGroup>
    {
        public GroupRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<SentenceGroup>> GetAllWithCountsAsync()
        {
            var groups = await _context.SentenceGroups
                .Include(g => g.SentenceLinks)
                .OrderBy(g => g.GroupName)
                .ToListAsync();

            foreach (var group in groups)
            {
                group.SentenceCount = group.SentenceLinks?.Count ?? 0;
            }

            return groups;
        }

        public async Task<SentenceGroup?> GetByNameAsync(string name)
        {
            return await _context.SentenceGroups
                .FirstOrDefaultAsync(g => g.GroupName == name);
        }

        public async Task MoveSentenceToGroupAsync(int sentenceId, int newGroupId)
        {
            var existingLinks = await _context.SentenceGroupLinks
                .Where(l => l.SentenceId == sentenceId)
                .ToListAsync();

            _context.SentenceGroupLinks.RemoveRange(existingLinks);

            var newLink = new SentenceGroupLink
            {
                SentenceId = sentenceId,
                GroupId = newGroupId
            };

            await _context.SentenceGroupLinks.AddAsync(newLink);
            await _context.SaveChangesAsync();
        }

        public async Task CopySentenceToGroupAsync(int sentenceId, int groupId)
        {
            var exists = await _context.SentenceGroupLinks
                .AnyAsync(l => l.SentenceId == sentenceId && l.GroupId == groupId);

            if (!exists)
            {
                var newLink = new SentenceGroupLink
                {
                    SentenceId = sentenceId,
                    GroupId = groupId
                };

                await _context.SentenceGroupLinks.AddAsync(newLink);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveSentenceFromGroupAsync(int sentenceId, int groupId)
        {
            var link = await _context.SentenceGroupLinks
                .FirstOrDefaultAsync(l => l.SentenceId == sentenceId && l.GroupId == groupId);

            if (link != null)
            {
                _context.SentenceGroupLinks.Remove(link);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteGroupWithSentencesAsync(int groupId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Remove all links
                var links = await _context.SentenceGroupLinks
                    .Where(l => l.GroupId == groupId)
                    .ToListAsync();
                _context.SentenceGroupLinks.RemoveRange(links);

                // Delete the group
                var group = await _context.SentenceGroups.FindAsync(groupId);
                if (group != null)
                {
                    _context.SentenceGroups.Remove(group);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
