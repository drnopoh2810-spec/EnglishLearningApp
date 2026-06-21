using CsvHelper;
using CsvHelper.Configuration;
using EnglishLearningApp.Data;
using EnglishLearningApp.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Globalization;
using System.IO;

namespace EnglishLearningApp.Services
{
    public class ImportExportService
    {
        private readonly AppDbContext _context;

        public ImportExportService(AppDbContext context)
        {
            _context = context;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        #region CSV Export/Import

        public async Task ExportToCsvAsync(string filePath, IEnumerable<Sentence> sentences)
        {
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

            csv.WriteHeader<SentenceExportDto>();
            await csv.NextRecordAsync();

            foreach (var sentence in sentences)
            {
                var groupNames = sentence.GroupLinks != null
                    ? string.Join("; ", sentence.GroupLinks.Select(gl => gl.Group?.GroupName ?? ""))
                    : "";

                var dto = new SentenceExportDto
                {
                    EnglishSentence = sentence.EnglishSentence,
                    ArabicTranslation = sentence.ArabicTranslation,
                    DifficultyLevel = sentence.DifficultyLevel.ToString(),
                    MasteryScore = sentence.MasteryScore,
                    ReviewCount = sentence.ReviewCount,
                    Notes = sentence.Notes,
                    Groups = groupNames,
                    CreatedDate = sentence.CreatedDate
                };

                csv.WriteRecord(dto);
                await csv.NextRecordAsync();
            }

            await writer.FlushAsync();
        }

        public async Task<List<SentenceImportDto>> ImportFromCsvAsync(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));

            var records = new List<SentenceImportDto>();
            await foreach (var record in csv.GetRecordsAsync<SentenceImportDto>())
            {
                records.Add(record);
            }

            return records;
        }

        #endregion

        #region Excel Export/Import

        public async Task ExportToExcelAsync(string filePath, IEnumerable<Sentence> sentences)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Sentences");

            // Headers
            worksheet.Cells[1, 1].Value = "English Sentence";
            worksheet.Cells[1, 2].Value = "Arabic Translation";
            worksheet.Cells[1, 3].Value = "Difficulty";
            worksheet.Cells[1, 4].Value = "Mastery Score";
            worksheet.Cells[1, 5].Value = "Review Count";
            worksheet.Cells[1, 6].Value = "Notes";
            worksheet.Cells[1, 7].Value = "Groups";
            worksheet.Cells[1, 8].Value = "Created Date";

            // Style header
            using (var range = worksheet.Cells[1, 1, 1, 8])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            }

            // Data
            int row = 2;
            foreach (var sentence in sentences)
            {
                var groupNames = sentence.GroupLinks != null
                    ? string.Join("; ", sentence.GroupLinks.Select(gl => gl.Group?.GroupName ?? ""))
                    : "";

                worksheet.Cells[row, 1].Value = sentence.EnglishSentence;
                worksheet.Cells[row, 2].Value = sentence.ArabicTranslation;
                worksheet.Cells[row, 3].Value = sentence.DifficultyLevel.ToString();
                worksheet.Cells[row, 4].Value = sentence.MasteryScore;
                worksheet.Cells[row, 5].Value = sentence.ReviewCount;
                worksheet.Cells[row, 6].Value = sentence.Notes;
                worksheet.Cells[row, 7].Value = groupNames;
                worksheet.Cells[row, 8].Value = sentence.CreatedDate.ToString("yyyy-MM-dd");
                row++;
            }

            worksheet.Cells.AutoFitColumns();
            await package.SaveAsAsync(new FileInfo(filePath));
        }

        public async Task<List<SentenceImportDto>> ImportFromExcelAsync(string filePath)
        {
            var records = new List<SentenceImportDto>();

            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension?.Rows ?? 0;

            for (int row = 2; row <= rowCount; row++)
            {
                var dto = new SentenceImportDto
                {
                    EnglishSentence = worksheet.Cells[row, 1].Text ?? "",
                    ArabicTranslation = worksheet.Cells[row, 2].Text ?? "",
                    DifficultyLevel = worksheet.Cells[row, 3].Text ?? "Beginner",
                    Notes = worksheet.Cells[row, 6].Text ?? "",
                    Groups = worksheet.Cells[row, 7].Text ?? ""
                };

                if (!string.IsNullOrWhiteSpace(dto.EnglishSentence))
                {
                    records.Add(dto);
                }
            }

            return records;
        }

        #endregion

        #region JSON Export/Import

        public async Task ExportToJsonAsync(string filePath, IEnumerable<Sentence> sentences)
        {
            var exportData = new SentenceExportCollection
            {
                ExportDate = DateTime.UtcNow,
                Version = "1.0",
                Sentences = sentences.Select(s => new SentenceExportJsonDto
                {
                    EnglishSentence = s.EnglishSentence,
                    ArabicTranslation = s.ArabicTranslation,
                    DifficultyLevel = s.DifficultyLevel.ToString(),
                    MasteryScore = s.MasteryScore,
                    ReviewCount = s.ReviewCount,
                    Notes = s.Notes,
                    Groups = s.GroupLinks != null
                        ? s.GroupLinks.Select(gl => gl.Group?.GroupName ?? "").ToList()
                        : new List<string>(),
                    CreatedDate = s.CreatedDate
                }).ToList()
            };

            var json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task<List<SentenceImportDto>> ImportFromJsonAsync(string filePath)
        {
            var json = await File.ReadAllTextAsync(filePath);
            var collection = JsonConvert.DeserializeObject<SentenceExportCollection>(json);

            return collection?.Sentences?.Select(s => new SentenceImportDto
            {
                EnglishSentence = s.EnglishSentence,
                ArabicTranslation = s.ArabicTranslation,
                DifficultyLevel = s.DifficultyLevel,
                Notes = s.Notes,
                Groups = string.Join("; ", s.Groups ?? new List<string>())
            }).ToList() ?? new List<SentenceImportDto>();
        }

        #endregion

        #region Process Import

        public async Task<ImportResult> ProcessImportAsync(List<SentenceImportDto> dtos, int defaultGroupId, TranslationService translationService)
        {
            var result = new ImportResult();

            // Load all existing sentences into a HashSet upfront to avoid O(n²) DB queries
            var existingSentences = new HashSet<string>(
                await _context.Sentences.Select(s => s.EnglishSentence.ToLower()).ToListAsync(),
                StringComparer.OrdinalIgnoreCase);

            foreach (var dto in dtos)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dto.EnglishSentence))
                    {
                        result.Skipped++;
                        continue;
                    }

                    // Check for duplicates using in-memory HashSet (fast)
                    if (existingSentences.Contains(dto.EnglishSentence.ToLower()))
                    {
                        result.Skipped++;
                        continue;
                    }

                    // Get translation if not provided
                    var translation = string.IsNullOrWhiteSpace(dto.ArabicTranslation)
                        ? await translationService.TranslateToArabicAsync(dto.EnglishSentence)
                        : dto.ArabicTranslation;

                    // Parse difficulty
                    if (!Enum.TryParse<DifficultyLevel>(dto.DifficultyLevel, true, out var difficulty))
                    {
                        difficulty = DifficultyLevel.Beginner;
                    }

                    var sentence = new Sentence
                    {
                        EnglishSentence = dto.EnglishSentence,
                        ArabicTranslation = translation,
                        DifficultyLevel = difficulty,
                        Notes = dto.Notes,
                        YouGlishUrl = $"https://youglish.com/pronounce/{Uri.EscapeDataString(dto.EnglishSentence)}/english"
                    };

                    _context.Sentences.Add(sentence);
                    await _context.SaveChangesAsync();

                    // Link to group
                    var link = new SentenceGroupLink
                    {
                        SentenceId = sentence.Id,
                        GroupId = defaultGroupId
                    };
                    _context.SentenceGroupLinks.Add(link);

                    // Parse additional groups
                    if (!string.IsNullOrWhiteSpace(dto.Groups))
                    {
                        var groupNames = dto.Groups.Split(';', StringSplitOptions.RemoveEmptyEntries)
                            .Select(g => g.Trim())
                            .Where(g => !string.IsNullOrWhiteSpace(g))
                            .Distinct();

                        foreach (var groupName in groupNames)
                        {
                            var group = await _context.SentenceGroups
                                .FirstOrDefaultAsync(g => g.GroupName.ToLower() == groupName.ToLower());

                            if (group == null)
                            {
                                group = new SentenceGroup { GroupName = groupName };
                                _context.SentenceGroups.Add(group);
                                await _context.SaveChangesAsync();
                            }

                            if (group.Id != defaultGroupId)
                            {
                                var additionalLink = new SentenceGroupLink
                                {
                                    SentenceId = sentence.Id,
                                    GroupId = group.Id
                                };
                                _context.SentenceGroupLinks.Add(additionalLink);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    // Track newly imported sentence to prevent duplicates within the same batch
                    existingSentences.Add(dto.EnglishSentence.ToLower());
                    result.Imported++;
                }
                catch (Exception ex)
                {
                    result.Errors++;
                    result.ErrorMessages.Add($"Error importing '{dto.EnglishSentence}': {ex.Message}");
                }
            }

            return result;
        }

        #endregion
    }

    #region DTOs

    public class SentenceExportDto
    {
        public string EnglishSentence { get; set; } = "";
        public string ArabicTranslation { get; set; } = "";
        public string DifficultyLevel { get; set; } = "Beginner";
        public double MasteryScore { get; set; }
        public int ReviewCount { get; set; }
        public string Notes { get; set; } = "";
        public string Groups { get; set; } = "";
        public DateTime CreatedDate { get; set; }
    }

    public class SentenceImportDto
    {
        public string EnglishSentence { get; set; } = "";
        public string ArabicTranslation { get; set; } = "";
        public string DifficultyLevel { get; set; } = "Beginner";
        public string Notes { get; set; } = "";
        public string Groups { get; set; } = "";
    }

    public class SentenceExportJsonDto
    {
        public string EnglishSentence { get; set; } = "";
        public string ArabicTranslation { get; set; } = "";
        public string DifficultyLevel { get; set; } = "";
        public double MasteryScore { get; set; }
        public int ReviewCount { get; set; }
        public string Notes { get; set; } = "";
        public List<string> Groups { get; set; } = new();
        public DateTime CreatedDate { get; set; }
    }

    public class SentenceExportCollection
    {
        public DateTime ExportDate { get; set; }
        public string Version { get; set; } = "1.0";
        public List<SentenceExportJsonDto> Sentences { get; set; } = new();
    }

    public class ImportResult
    {
        public int Imported { get; set; }
        public int Skipped { get; set; }
        public int Errors { get; set; }
        public List<string> ErrorMessages { get; set; } = new();
    }

    #endregion
}
