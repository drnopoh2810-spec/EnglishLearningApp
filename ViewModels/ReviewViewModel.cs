using EnglishLearningApp.Helpers;
using EnglishLearningApp.Models;
using EnglishLearningApp.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace EnglishLearningApp.ViewModels
{
    public class ReviewViewModel : ViewModelBase, IRefreshable
    {
        private readonly ReviewService _reviewService;
        private readonly YouGlishService _youGlishService;
        private readonly StatisticsService _statisticsService;

        private ObservableCollection<Sentence> _reviewQueue = new();
        private Sentence? _currentSentence;
        private bool _isShowingFront = true;
        private bool _isReviewActive;
        private int _currentIndex;
        private int _totalCount;
        private string _reviewStatus = "";
        private double _sessionMasteryChange;
        private int _reviewsCompleted;

        public ObservableCollection<Sentence> ReviewQueue
        {
            get => _reviewQueue;
            set => SetProperty(ref _reviewQueue, value);
        }

        public Sentence? CurrentSentence
        {
            get => _currentSentence;
            set
            {
                if (SetProperty(ref _currentSentence, value))
                {
                    IsShowingFront = true;
                    OnPropertyChanged(nameof(CurrentSentenceText));
                    OnPropertyChanged(nameof(CurrentTranslation));
                    OnPropertyChanged(nameof(CurrentNotes));
                    OnPropertyChanged(nameof(CurrentMastery));
                    OnPropertyChanged(nameof(CurrentReviewCount));
                    OnPropertyChanged(nameof(CurrentGroupNames));
                    OnPropertyChanged(nameof(CurrentDifficulty));
                }
            }
        }

        public string CurrentSentenceText => CurrentSentence?.EnglishSentence ?? "No sentence available";
        public string CurrentTranslation => CurrentSentence?.ArabicTranslation ?? "";
        public string CurrentNotes => CurrentSentence?.Notes ?? "";
        public double CurrentMastery => CurrentSentence?.MasteryScore ?? 0;
        public int CurrentReviewCount => CurrentSentence?.ReviewCount ?? 0;
        public string CurrentGroupNames => CurrentSentence?.GroupNames ?? "";
        public string CurrentDifficulty => CurrentSentence?.DifficultyLevel.ToString() ?? "";

        public bool IsShowingFront
        {
            get => _isShowingFront;
            set
            {
                if (SetProperty(ref _isShowingFront, value))
                {
                    OnPropertyChanged(nameof(IsShowingBack));
                }
            }
        }

        public bool IsShowingBack => !IsShowingFront;

        public bool IsReviewActive
        {
            get => _isReviewActive;
            set => SetProperty(ref _isReviewActive, value);
        }

        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                SetProperty(ref _currentIndex, value);
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        public int TotalCount
        {
            get => _totalCount;
            set
            {
                SetProperty(ref _totalCount, value);
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        public string ProgressText => $"{CurrentIndex + 1} / {TotalCount}";

        public string ReviewStatus
        {
            get => _reviewStatus;
            set => SetProperty(ref _reviewStatus, value);
        }

        public int ReviewsCompleted
        {
            get => _reviewsCompleted;
            set => SetProperty(ref _reviewsCompleted, value);
        }

        public double SessionMasteryChange
        {
            get => _sessionMasteryChange;
            set => SetProperty(ref _sessionMasteryChange, value);
        }

        public ICommand StartReviewCommand { get; }
        public ICommand FlipCardCommand { get; }
        public ICommand RateAgainCommand { get; }
        public ICommand RateHardCommand { get; }
        public ICommand RateGoodCommand { get; }
        public ICommand RateEasyCommand { get; }
        public ICommand SkipCommand { get; }
        public ICommand OpenYouGlishCommand { get; }
        public ICommand PlayAudioCommand { get; }
        public ICommand EndSessionCommand { get; }

        public ReviewViewModel(
            ReviewService reviewService,
            YouGlishService youGlishService,
            StatisticsService statisticsService)
        {
            _reviewService = reviewService;
            _youGlishService = youGlishService;
            _statisticsService = statisticsService;

            StartReviewCommand = new RelayCommand(async () => await StartReviewAsync());
            FlipCardCommand = new RelayCommand(() => IsShowingFront = !IsShowingFront);
            RateAgainCommand = new RelayCommand(async () => await RateSentenceAsync(ReviewRating.Again));
            RateHardCommand = new RelayCommand(async () => await RateSentenceAsync(ReviewRating.Hard));
            RateGoodCommand = new RelayCommand(async () => await RateSentenceAsync(ReviewRating.Good));
            RateEasyCommand = new RelayCommand(async () => await RateSentenceAsync(ReviewRating.Easy));
            SkipCommand = new RelayCommand(() => NextSentence());
            OpenYouGlishCommand = new RelayCommand(() => OpenYouGlish());
            PlayAudioCommand = new RelayCommand(() => PlayAudio());
            EndSessionCommand = new RelayCommand(() => EndSession());
        }

        public void Refresh()
        {
            if (IsReviewActive)
            {
                EndSession();
            }
            _ = CheckDueCountAsync();
        }

        private async Task CheckDueCountAsync()
        {
            try
            {
                var stats = await _statisticsService.GetDashboardStatisticsAsync();
                ReviewStatus = $"{stats.SentencesDueToday} sentences due for review";
            }
            catch { }
        }

        private async Task StartReviewAsync()
        {
            var sentences = (await _reviewService.GetPrioritizedSentencesAsync(20)).ToList();

            if (sentences.Count == 0)
            {
                MessageBox.Show("No sentences are due for review! Add some sentences or come back later.", "Review Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ReviewQueue = new ObservableCollection<Sentence>(sentences);
            CurrentIndex = 0;
            TotalCount = sentences.Count;
            ReviewsCompleted = 0;
            SessionMasteryChange = 0;
            IsReviewActive = true;

            // Enrich first sentence
            await EnrichSentenceAsync(sentences[0]);
            CurrentSentence = sentences[0];
        }

        private async Task RateSentenceAsync(ReviewRating rating)
        {
            if (CurrentSentence == null) return;

            try
            {
                var result = await _reviewService.ProcessReviewAsync(CurrentSentence.Id, rating);
                SessionMasteryChange += (result.NewMastery - result.PreviousMastery);
                ReviewsCompleted++;

                NextSentence();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing review: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NextSentence()
        {
            if (CurrentIndex < TotalCount - 1)
            {
                CurrentIndex++;
                var sentence = ReviewQueue[CurrentIndex];
                _ = EnrichSentenceAsync(sentence);
                CurrentSentence = sentence;
                IsShowingFront = true;
            }
            else
            {
                EndSession();
            }
        }

        private async Task EnrichSentenceAsync(Sentence sentence)
        {
            // Load group names
            sentence.GroupNames = sentence.GroupLinks != null
                ? string.Join(", ", sentence.GroupLinks.Select(gl => gl.Group?.GroupName ?? ""))
                : "";

            // Generate YouGlish URL if missing
            if (string.IsNullOrEmpty(sentence.YouGlishUrl))
            {
                sentence.YouGlishUrl = _youGlishService.GenerateUrl(sentence.EnglishSentence);
            }

            await Task.CompletedTask;
        }

        private void OpenYouGlish()
        {
            if (CurrentSentence != null)
            {
                _youGlishService.OpenInBrowser(CurrentSentence.EnglishSentence);
            }
        }

        private void PlayAudio()
        {
            if (CurrentSentence == null) return;

            try
            {
                var synth = new System.Speech.Synthesis.SpeechSynthesizer();
                synth.SpeakAsync(CurrentSentence.EnglishSentence);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not play audio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EndSession()
        {
            IsReviewActive = false;
            CurrentSentence = null;
            ReviewQueue.Clear();
            _ = CheckDueCountAsync();

            if (ReviewsCompleted > 0)
            {
                MessageBox.Show(
                    $"Review session complete!\n\nSentences reviewed: {ReviewsCompleted}\nMastery change: {SessionMasteryChange:F1}",
                    "Session Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
    }
}
