using EnglishLearningApp.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace EnglishLearningApp.Services
{
    public class DatabaseService
    {
        private readonly AppDbContext _context;

        public DatabaseService(AppDbContext context)
        {
            _context = context;
        }

        public async Task InitializeDatabaseAsync()
        {
            await _context.Database.MigrateAsync();
        }

        public string GetDatabasePath()
        {
            var connectionString = _context.Database.GetDbConnection().ConnectionString;
            return connectionString.Replace("Data Source=", "");
        }

        public async Task BackupDatabaseAsync(string backupPath)
        {
            await Task.Run(() =>
            {
                var backupDir = Path.GetDirectoryName(backupPath);
                if (!string.IsNullOrEmpty(backupDir))
                    Directory.CreateDirectory(backupDir);

                // Use SQLite Online Backup API via a second connection to avoid corruption
                var dbPath = GetDatabasePath();
                using var sourceConn = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbPath}");
                using var destConn = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={backupPath}");
                sourceConn.Open();
                destConn.Open();
                sourceConn.BackupDatabase(destConn);
            });
        }

        public async Task ResetAllProgressAsync()
        {
            // Delete all review history
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Reviews;");

            // Reset all sentence statistics
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Sentences SET MasteryScore = 0, ReviewCount = 0, LastReviewDate = NULL, NextReviewDate = NULL;");
        }

        public async Task<bool> CompactDatabaseAsync()
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("VACUUM;");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
