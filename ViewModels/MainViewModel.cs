using EnglishLearningApp.Helpers;
using System.Windows.Input;

namespace EnglishLearningApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private object _currentView = null!;
        private string _currentViewName = "Dashboard";
        private bool _isDarkTheme = true;

        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public string CurrentViewName
        {
            get => _currentViewName;
            set => SetProperty(ref _currentViewName, value);
        }

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set => SetProperty(ref _isDarkTheme, value);
        }

        // Navigation Commands
        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToSentencesCommand { get; }
        public ICommand NavigateToGroupsCommand { get; }
        public ICommand NavigateToReviewCommand { get; }
        public ICommand NavigateToImportExportCommand { get; }
        public ICommand NavigateToStatisticsCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }
        public ICommand ToggleThemeCommand { get; }

        // ViewModels
        public DashboardViewModel DashboardViewModel { get; }
        public SentencesViewModel SentencesViewModel { get; }
        public GroupsViewModel GroupsViewModel { get; }
        public ReviewViewModel ReviewViewModel { get; }
        public ImportExportViewModel ImportExportViewModel { get; }
        public StatisticsViewModel StatisticsViewModel { get; }
        public SettingsViewModel SettingsViewModel { get; }

        public MainViewModel(
            DashboardViewModel dashboardViewModel,
            SentencesViewModel sentencesViewModel,
            GroupsViewModel groupsViewModel,
            ReviewViewModel reviewViewModel,
            ImportExportViewModel importExportViewModel,
            StatisticsViewModel statisticsViewModel,
            SettingsViewModel settingsViewModel)
        {
            DashboardViewModel = dashboardViewModel;
            SentencesViewModel = sentencesViewModel;
            GroupsViewModel = groupsViewModel;
            ReviewViewModel = reviewViewModel;
            ImportExportViewModel = importExportViewModel;
            StatisticsViewModel = statisticsViewModel;
            SettingsViewModel = settingsViewModel;

            NavigateToDashboardCommand = new RelayCommand(() => NavigateTo("Dashboard"));
            NavigateToSentencesCommand = new RelayCommand(() => NavigateTo("Sentences"));
            NavigateToGroupsCommand = new RelayCommand(() => NavigateTo("Groups"));
            NavigateToReviewCommand = new RelayCommand(() => NavigateTo("Review"));
            NavigateToImportExportCommand = new RelayCommand(() => NavigateTo("ImportExport"));
            NavigateToStatisticsCommand = new RelayCommand(() => NavigateTo("Statistics"));
            NavigateToSettingsCommand = new RelayCommand(() => NavigateTo("Settings"));
            ToggleThemeCommand = new RelayCommand(ToggleTheme);

            // Set initial view
            CurrentView = DashboardViewModel;
        }

        private void NavigateTo(string viewName)
        {
            CurrentViewName = viewName;
            CurrentView = viewName switch
            {
                "Dashboard" => DashboardViewModel,
                "Sentences" => SentencesViewModel,
                "Groups" => GroupsViewModel,
                "Review" => ReviewViewModel,
                "ImportExport" => ImportExportViewModel,
                "Statistics" => StatisticsViewModel,
                "Settings" => SettingsViewModel,
                _ => DashboardViewModel
            };

            // Refresh data when navigating
            if (CurrentView is IRefreshable refreshable)
            {
                refreshable.Refresh();
            }
        }

        private void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
            var theme = IsDarkTheme ? "Dark" : "Light";
            
            // Apply theme using Material Design palette helper
            var paletteHelper = new MaterialDesignThemes.Wpf.PaletteHelper();
            var themeObj = paletteHelper.GetTheme();
            
            if (IsDarkTheme)
            {
                themeObj.SetBaseTheme(MaterialDesignThemes.Wpf.BaseTheme.Dark);
            }
            else
            {
                themeObj.SetBaseTheme(MaterialDesignThemes.Wpf.BaseTheme.Light);
            }
            
            paletteHelper.SetTheme(themeObj);
        }
    }

    public interface IRefreshable
    {
        void Refresh();
    }
}
