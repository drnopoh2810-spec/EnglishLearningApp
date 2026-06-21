using EnglishLearningApp.Helpers;
using EnglishLearningApp.Models;
using EnglishLearningApp.Repositories;
using EnglishLearningApp.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace EnglishLearningApp.ViewModels
{
    public class ImportExportViewModel : ViewModelBase, IRefreshable
    {
        private readonly ImportExportService _importExportService;
        private readonly TranslationService _translationService;
        private readonly SentenceRepository _sentenceRepository;
        private readonly GroupRepository _groupRepository;

        private ObservableCollection<SentenceGroup> _groups = new();
        private int _selectedGroupId = 1;
        private bool _isImporting;
        private bool _isExporting;
        private string _statusMessage = "";
        private bool _isStatusError;

        public ObservableCollection<SentenceGroup> Groups
        {
            get => _groups;
            set => SetProperty(ref _groups, value);
        }

        public int SelectedGroupId
        {
            get => _selectedGroupId;
            set => SetProperty(ref _selectedGroupId, value);
        }

        public bool IsImporting
        {
            get => _isImporting;
            set => SetProperty(ref _isImporting, value);
        }

        public bool IsExporting
        {
            get => _isExporting;
            set => SetProperty(ref _isExporting, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsStatusError
        {
            get => _isStatusError;
            set => SetProperty(ref _isStatusError, value);
        }

        public ICommand ExportCsvCommand { get; }
        public ICommand ExportExcelCommand { get; }
        public ICommand ExportJsonCommand { get; }
        public ICommand ImportCsvCommand { get; }
        public ICommand ImportExcelCommand { get; }
        public ICommand ImportJsonCommand { get; }
        public ICommand RefreshCommand { get; }

        public ImportExportViewModel(
            ImportExportService importExportService,
            TranslationService translationService,
            SentenceRepository sentenceRepository,
            GroupRepository groupRepository)
        {
            _importExportService = importExportService;
            _translationService = translationService;
            _sentenceRepository = sentenceRepository;
            _groupRepository = groupRepository;

            ExportCsvCommand = new RelayCommand(async () => await ExportAsync("csv"));
            ExportExcelCommand = new RelayCommand(async () => await ExportAsync("excel"));
            ExportJsonCommand = new RelayCommand(async () => await ExportAsync("json"));
            ImportCsvCommand = new RelayCommand(async () => await ImportAsync("csv"));
            ImportExcelCommand = new RelayCommand(async () => await ImportAsync("excel"));
            ImportJsonCommand = new RelayCommand(async () => await ImportAsync("json"));
            RefreshCommand = new RelayCommand(() => Refresh());

            _ = LoadGroupsAsync();
        }

        public void Refresh()
        {
            _ = LoadGroupsAsync();
        }

        private async Task LoadGroupsAsync()
        {
            var groups = await _groupRepository.GetAllAsync();
            Groups = new ObservableCollection<SentenceGroup>(groups);
        }

        private async Task ExportAsync(string format)
        {
            IsExporting = true;
            StatusMessage = "";

            try
            {
                var sentences = await _sentenceRepository.GetAllWithDetailsAsync();

                if (!sentences.Any())
                {
                    StatusMessage = "No sentences to export.";
                    IsStatusError = true;
                    return;
                }

                var dialog = new SaveFileDialog();
                dialog.Filter = format.ToLower() switch
                {
                    "csv" => "CSV files (*.csv)|*.csv",
                    "excel" => "Excel files (*.xlsx)|*.xlsx",
                    "json" => "JSON files (*.json)|*.json",
                    _ => "All files (*.*)|*.*"
                };

                if (dialog.ShowDialog() == true)
                {
                    switch (format.ToLower())
                    {
                        case "csv":
                            await _importExportService.ExportToCsvAsync(dialog.FileName, sentences);
                            break;
                        case "excel":
                            await _importExportService.ExportToExcelAsync(dialog.FileName, sentences);
                            break;
                        case "json":
                            await _importExportService.ExportToJsonAsync(dialog.FileName, sentences);
                            break;
                    }

                    StatusMessage = $"Exported {sentences.Count()} sentences to {Path.GetFileName(dialog.FileName)}";
                    IsStatusError = false;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Export failed: {ex.Message}";
                IsStatusError = true;
            }
            finally
            {
                IsExporting = false;
            }
        }

        private async Task ImportAsync(string format)
        {
            IsImporting = true;
            StatusMessage = "";

            try
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = format.ToLower() switch
                {
                    "csv" => "CSV files (*.csv)|*.csv",
                    "excel" => "Excel files (*.xlsx)|*.xlsx",
                    "json" => "JSON files (*.json)|*.json",
                    _ => "All files (*.*)|*.*"
                };

                if (dialog.ShowDialog() != true) return;

                List<SentenceImportDto> dtos;

                switch (format.ToLower())
                {
                    case "csv":
                        dtos = await _importExportService.ImportFromCsvAsync(dialog.FileName);
                        break;
                    case "excel":
                        dtos = await _importExportService.ImportFromExcelAsync(dialog.FileName);
                        break;
                    case "json":
                        dtos = await _importExportService.ImportFromJsonAsync(dialog.FileName);
                        break;
                    default:
                        dtos = new List<SentenceImportDto>();
                        break;
                }

                if (!dtos.Any())
                {
                    StatusMessage = "No valid sentences found in file.";
                    IsStatusError = true;
                    return;
                }

                var result = await _importExportService.ProcessImportAsync(dtos, SelectedGroupId, _translationService);

                StatusMessage = $"Import complete! Imported: {result.Imported}, Skipped: {result.Skipped}, Errors: {result.Errors}";
                IsStatusError = result.Errors > 0;

                if (result.ErrorMessages.Any())
                {
                    var errorFile = Path.Combine(
                        Path.GetDirectoryName(dialog.FileName) ?? "",
                        $"import_errors_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                    await File.WriteAllLinesAsync(errorFile, result.ErrorMessages);
                    StatusMessage += $"\nError details saved to: {errorFile}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Import failed: {ex.Message}";
                IsStatusError = true;
            }
            finally
            {
                IsImporting = false;
            }
        }
    }
}
