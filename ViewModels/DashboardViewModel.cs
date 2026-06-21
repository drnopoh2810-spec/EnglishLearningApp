using EnglishLearningApp.Helpers;
using EnglishLearningApp.Services;
using System.Windows.Input;

namespace EnglishLearningApp.ViewModels
{
    public class DashboardViewModel : ViewModelBase, IRefreshable
    {
        private readonly StatisticsService _statisticsService;

        private int _totalSentences;
        private int _sentencesLearned;
        private int _sentencesDueToday;
        private int _masteredSentences;
        private double _averageMastery;
        private int _currentStreak;
        private int _reviewsToday;
        private int _totalReviews;
        private bool _isLoading;

        public int TotalSentences
        {
            get => _totalSentences;
            set => SetProperty(ref _totalSentences, value);
        }

        public int SentencesLearned
        {
            get => _sentencesLearned;
            set => SetProperty(ref _sentencesLearned, value);
        }

        public int SentencesDueToday
        {
            get => _sentencesDueToday;
            set => SetProperty(ref _sentencesDueToday, value);
        }

        public int MasteredSentences
        {
            get => _masteredSentences;
            set => SetProperty(ref _masteredSentences, value);
        }

        public double AverageMastery
        {
            get => _averageMastery;
            set => SetProperty(ref _averageMastery, value);
        }

        public int CurrentStreak
        {
            get => _currentStreak;
            set => SetProperty(ref _currentStreak, value);
        }

        public int ReviewsToday
        {
            get => _reviewsToday;
            set => SetProperty(ref _reviewsToday, value);
        }

        public int TotalReviews
        {
            get => _totalReviews;
            set => SetProperty(ref _totalReviews, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public List<ActivityDayViewModel> WeeklyActivity { get; set; } = new();
        public List<DifficultyStatViewModel> DifficultyDistribution { get; set; } = new();
        public List<MasteryRangeViewModel> MasteryDistribution { get; set; } = new();

        public ICommand RefreshCommand { get; }

        public DashboardViewModel(StatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
            RefreshCommand = new RelayCommand(() => Refresh());
            _ = LoadDataAsync();
        }

        public void Refresh()
        {
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;

            try
            {
                var stats = await _statisticsService.GetDashboardStatisticsAsync();

                TotalSentences = stats.TotalSentences;
                SentencesLearned = stats.SentencesLearned;
                SentencesDueToday = stats.SentencesDueToday;
                MasteredSentences = stats.MasteredSentences;
                AverageMastery = stats.AverageMastery;
                CurrentStreak = stats.CurrentStreak;
                ReviewsToday = stats.ReviewsToday;
                TotalReviews = stats.TotalReviews;

                WeeklyActivity = stats.WeeklyActivity.Select(a => new ActivityDayViewModel
                {
                    DayName = a.DayName,
                    Count = a.Count,
                    BarHeight = Math.Min(100, a.Count > 0 ? Math.Max(10, a.Count * 5) : 0)
                }).ToList();

                DifficultyDistribution = stats.DifficultyDistribution.Select(d => new DifficultyStatViewModel
                {
                    Level = d.Level,
                    Count = d.Count
                }).ToList();

                MasteryDistribution = stats.MasteryDistribution.Select(m => new MasteryRangeViewModel
                {
                    Range = m.Range,
                    Count = m.Count
                }).ToList();

                OnPropertyChanged(nameof(WeeklyActivity));
                OnPropertyChanged(nameof(DifficultyDistribution));
                OnPropertyChanged(nameof(MasteryDistribution));
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    public class ActivityDayViewModel
    {
        public string DayName { get; set; } = "";
        public int Count { get; set; }
        public double BarHeight { get; set; }
    }

    public class DifficultyStatViewModel
    {
        public string Level { get; set; } = "";
        public int Count { get; set; }
    }

    public class MasteryRangeViewModel
    {
        public string Range { get; set; } = "";
        public int Count { get; set; }
    }
}
