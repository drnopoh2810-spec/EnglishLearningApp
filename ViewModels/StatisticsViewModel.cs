using EnglishLearningApp.Helpers;
using EnglishLearningApp.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EnglishLearningApp.ViewModels
{
    public class StatisticsViewModel : ViewModelBase, IRefreshable
    {
        private readonly StatisticsService _statisticsService;

        private bool _isLoading;
        private int _totalSentences;
        private int _masteredSentences;
        private int _sentencesDueToday;
        private int _sentencesLearned;
        private double _averageMastery;
        private int _currentStreak;
        private int _reviewsToday;
        private int _totalReviews;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public int TotalSentences
        {
            get => _totalSentences;
            set => SetProperty(ref _totalSentences, value);
        }

        public int MasteredSentences
        {
            get => _masteredSentences;
            set => SetProperty(ref _masteredSentences, value);
        }

        public int SentencesDueToday
        {
            get => _sentencesDueToday;
            set => SetProperty(ref _sentencesDueToday, value);
        }

        public int SentencesLearned
        {
            get => _sentencesLearned;
            set => SetProperty(ref _sentencesLearned, value);
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

        public ObservableCollection<ActivityDayViewModel> WeeklyActivity { get; set; } = new();
        public ObservableCollection<GroupStatViewModel> GroupStats { get; set; } = new();
        public ObservableCollection<RecentReviewViewModel> RecentReviews { get; set; } = new();

        public ICommand RefreshCommand { get; }

        public StatisticsViewModel(StatisticsService statisticsService)
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
                MasteredSentences = stats.MasteredSentences;
                SentencesDueToday = stats.SentencesDueToday;
                SentencesLearned = stats.SentencesLearned;
                AverageMastery = stats.AverageMastery;
                CurrentStreak = stats.CurrentStreak;
                ReviewsToday = stats.ReviewsToday;
                TotalReviews = stats.TotalReviews;

                WeeklyActivity = new ObservableCollection<ActivityDayViewModel>(
                    stats.WeeklyActivity.Select(a => new ActivityDayViewModel
                    {
                        DayName = a.DayName,
                        Count = a.Count,
                        BarHeight = Math.Min(100, a.Count > 0 ? Math.Max(10, a.Count * 5) : 0)
                    }));

                GroupStats = new ObservableCollection<GroupStatViewModel>(
                    stats.GroupStatistics.Select(g => new GroupStatViewModel
                    {
                        GroupName = g.GroupName,
                        SentenceCount = g.SentenceCount,
                        AverageMastery = Math.Round(g.AverageMastery, 1)
                    }));

                var recentReviews = await _statisticsService.GetRecentReviewsAsync(20);
                RecentReviews = new ObservableCollection<RecentReviewViewModel>(
                    recentReviews.Select(r => new RecentReviewViewModel
                    {
                        SentenceText = r.SentenceText.Length > 50 ? r.SentenceText[..50] + "..." : r.SentenceText,
                        Rating = r.Rating,
                        ReviewDate = r.ReviewDate.ToString("g"),
                        MasteryAfter = r.MasteryAfter
                    }));

                OnPropertyChanged(nameof(WeeklyActivity));
                OnPropertyChanged(nameof(GroupStats));
                OnPropertyChanged(nameof(RecentReviews));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading statistics: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    public class GroupStatViewModel
    {
        public string GroupName { get; set; } = "";
        public int SentenceCount { get; set; }
        public double AverageMastery { get; set; }
    }

    public class RecentReviewViewModel
    {
        public string SentenceText { get; set; } = "";
        public string Rating { get; set; } = "";
        public string ReviewDate { get; set; } = "";
        public double MasteryAfter { get; set; }
    }
}
