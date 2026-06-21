using EnglishLearningApp.Helpers;
using EnglishLearningApp.Services;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace EnglishLearningApp.ViewModels
{
    public class SettingsViewModel : ViewModelBase, IRefreshable
    {
        private readonly DatabaseService _databaseService;

        private bool _isDarkTheme = true;
        private int _dailyReviewTarget = 20;
        private bool _autoTranslate = true;
        private bool _autoPlayAudio;
        private string _databasePath = "";
        private string _statusMessage = "";

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (SetProperty(ref _isDarkTheme, value))
                {
                    ApplyTheme();
                }
            }
        }

        public int DailyReviewTarget
        {
            get => _dailyReviewTarget;
            set => SetProperty(ref _dailyReviewTarget, value);
        }

        public bool AutoTranslate
        {
            get => _autoTranslate;
            set => SetProperty(ref _autoTranslate, value);
        }

        public bool AutoPlayAudio
        {
            get => _autoPlayAudio;
            set => SetProperty(ref _autoPlayAudio, value);
        }

        public string DatabasePath
        {
            get => _databasePath;
            set => SetProperty(ref _databasePath, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand BackupDatabaseCommand { get; }
        public ICommand CompactDatabaseCommand { get; }
        public ICommand ResetProgressCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand RefreshCommand { get; }

        public SettingsViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            BackupDatabaseCommand = new RelayCommand(async () => await BackupDatabaseAsync());
            CompactDatabaseCommand = new RelayCommand(async () => await CompactDatabaseAsync());
            ResetProgressCommand = new RelayCommand(async () => await ResetProgressAsync());
            SaveSettingsCommand = new RelayCommand(() => SaveSettings());
            RefreshCommand = new RelayCommand(() => Refresh());

            LoadSettings();
            DatabasePath = _databaseService.GetDatabasePath();
        }

        public void Refresh()
        {
            DatabasePath = _databaseService.GetDatabasePath();
        }

        private void ApplyTheme()
        {
            var paletteHelper = new MaterialDesignThemes.Wpf.PaletteHelper();
            var theme = paletteHelper.GetTheme();

            if (IsDarkTheme)
            {
                theme.SetBaseTheme(MaterialDesignThemes.Wpf.Theme.Dark);
            }
            else
            {
                theme.SetBaseTheme(MaterialDesignThemes.Wpf.Theme.Light);
            }

            paletteHelper.SetTheme(theme);
        }

        private async Task BackupDatabaseAsync()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "SQLite Database (*.db)|*.db|All files (*.*)|*.*",
                    FileName = $"english_learning_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db"
                };

                if (dialog.ShowDialog() == true)
                {
                    await _databaseService.BackupDatabaseAsync(dialog.FileName);
                    StatusMessage = $"Database backed up to: {dialog.FileName}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Backup failed: {ex.Message}";
            }
        }

        private async Task CompactDatabaseAsync()
        {
            try
            {
                var result = await _databaseService.CompactDatabaseAsync();
                StatusMessage = result ? "Database compacted successfully." : "Failed to compact database.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Compact failed: {ex.Message}";
            }
        }

        private async Task ResetProgressAsync()
        {
            var result = MessageBox.Show(
                "This will reset all review progress and mastery scores. This action cannot be undone.\n\nAre you sure?",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _databaseService.ResetAllProgressAsync();
                    StatusMessage = "All progress has been reset successfully.";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Reset failed: {ex.Message}";
                }
            }
        }

        private void SaveSettings()
        {
            try
            {
                var settings = new AppSettings
                {
                    IsDarkTheme = IsDarkTheme,
                    DailyReviewTarget = DailyReviewTarget,
                    AutoTranslate = AutoTranslate,
                    AutoPlayAudio = AutoPlayAudio
                };

                var settingsPath = GetSettingsPath();
                var json = System.Text.Json.JsonSerializer.Serialize(settings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(settingsPath, json);

                StatusMessage = "Settings saved successfully.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to save settings: {ex.Message}";
            }
        }

        private void LoadSettings()
        {
            try
            {
                var settingsPath = GetSettingsPath();
                if (File.Exists(settingsPath))
                {
                    var json = File.ReadAllText(settingsPath);
                    var settings = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json);

                    if (settings != null)
                    {
                        IsDarkTheme = settings.IsDarkTheme;
                        DailyReviewTarget = settings.DailyReviewTarget;
                        AutoTranslate = settings.AutoTranslate;
                        AutoPlayAudio = settings.AutoPlayAudio;
                    }
                }
            }
            catch
            {
                // Use defaults
            }
        }

        private string GetSettingsPath()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(folder, "EnglishLearningApp");
            Directory.CreateDirectory(appFolder);
            return Path.Combine(appFolder, "settings.json");
        }
    }

    public class AppSettings
    {
        public bool IsDarkTheme { get; set; } = true;
        public int DailyReviewTarget { get; set; } = 20;
        public bool AutoTranslate { get; set; } = true;
        public bool AutoPlayAudio { get; set; } = false;
    }
}
