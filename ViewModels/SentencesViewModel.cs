using EnglishLearningApp.Helpers;
using EnglishLearningApp.Models;
using EnglishLearningApp.Repositories;
using EnglishLearningApp.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace EnglishLearningApp.ViewModels
{
    public class SentencesViewModel : ViewModelBase, IRefreshable
    {
        private readonly SentenceRepository _sentenceRepository;
        private readonly GroupRepository _groupRepository;
        private readonly TranslationService _translationService;
        private readonly YouGlishService _youGlishService;

        private ObservableCollection<Sentence> _sentences = new();
        private ObservableCollection<SentenceGroup> _groups = new();
        private Sentence? _selectedSentence;
        private string _searchText = "";
        private int? _selectedGroupId;
        private DifficultyLevel? _selectedDifficulty;
        private bool _isAddingNew;
        private string _newEnglishSentence = "";
        private string _newArabicTranslation = "";
        private DifficultyLevel _newDifficulty = DifficultyLevel.Beginner;
        private string _newNotes = "";
        private int _newGroupId = 0;
        private bool _isTranslating;

        public ObservableCollection<Sentence> Sentences
        {
            get => _sentences;
            set => SetProperty(ref _sentences, value);
        }

        public ObservableCollection<SentenceGroup> Groups
        {
            get => _groups;
            set => SetProperty(ref _groups, value);
        }

        public Sentence? SelectedSentence
        {
            get => _selectedSentence;
            set => SetProperty(ref _selectedSentence, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    _ = SearchSentencesAsync();
                }
            }
        }

        public int? SelectedGroupId
        {
            get => _selectedGroupId;
            set
            {
                if (SetProperty(ref _selectedGroupId, value))
                {
                    _ = SearchSentencesAsync();
                }
            }
        }

        public DifficultyLevel? SelectedDifficulty
        {
            get => _selectedDifficulty;
            set
            {
                if (SetProperty(ref _selectedDifficulty, value))
                {
                    _ = SearchSentencesAsync();
                }
            }
        }

        public bool IsAddingNew
        {
            get => _isAddingNew;
            set => SetProperty(ref _isAddingNew, value);
        }

        public string NewEnglishSentence
        {
            get => _newEnglishSentence;
            set => SetProperty(ref _newEnglishSentence, value);
        }

        public string NewArabicTranslation
        {
            get => _newArabicTranslation;
            set => SetProperty(ref _newArabicTranslation, value);
        }

        public DifficultyLevel NewDifficulty
        {
            get => _newDifficulty;
            set => SetProperty(ref _newDifficulty, value);
        }

        public string NewNotes
        {
            get => _newNotes;
            set => SetProperty(ref _newNotes, value);
        }

        public int NewGroupId
        {
            get => _newGroupId;
            set => SetProperty(ref _newGroupId, value);
        }

        public bool IsTranslating
        {
            get => _isTranslating;
            set => SetProperty(ref _isTranslating, value);
        }

        public ICommand SearchCommand { get; }
        public ICommand AddNewCommand { get; }
        public ICommand SaveNewCommand { get; }
        public ICommand CancelAddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand OpenYouGlishCommand { get; }
        public ICommand PlayPronunciationCommand { get; }
        public ICommand TranslateCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        public SentencesViewModel(
            SentenceRepository sentenceRepository,
            GroupRepository groupRepository,
            TranslationService translationService,
            YouGlishService youGlishService)
        {
            _sentenceRepository = sentenceRepository;
            _groupRepository = groupRepository;
            _translationService = translationService;
            _youGlishService = youGlishService;

            SearchCommand = new RelayCommand(() => _ = SearchSentencesAsync());
            AddNewCommand = new RelayCommand(() => IsAddingNew = true);
            SaveNewCommand = new RelayCommand(async () => await SaveNewSentenceAsync());
            CancelAddCommand = new RelayCommand(() => IsAddingNew = false);
            DeleteCommand = new RelayCommand<Sentence>(async (s) => await DeleteSentenceAsync(s));
            OpenYouGlishCommand = new RelayCommand<Sentence>((s) => OpenYouGlish(s));
            PlayPronunciationCommand = new RelayCommand<Sentence>((s) => PlayPronunciation(s));
            TranslateCommand = new RelayCommand(async () => await TranslateNewSentenceAsync());
            RefreshCommand = new RelayCommand(() => Refresh());
            ClearFiltersCommand = new RelayCommand(() => ClearFilters());

            _ = LoadDataAsync();
        }

        public void Refresh()
        {
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var groups = await _groupRepository.GetAllAsync();
            Groups = new ObservableCollection<SentenceGroup>(groups);

            // Set default group to the first available group (not hardcoded ID)
            if (NewGroupId == 0 && Groups.Count > 0)
                NewGroupId = Groups[0].Id;

            await SearchSentencesAsync();
        }

        private async Task SearchSentencesAsync()
        {
            string? search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText;
            int? groupId = SelectedGroupId == 0 ? null : SelectedGroupId;
            DifficultyLevel? difficulty = SelectedDifficulty == 0 ? null : SelectedDifficulty;

            var sentences = await _sentenceRepository.SearchSentencesAsync(search, groupId, difficulty);

            // Enrich with group names
            foreach (var sentence in sentences)
            {
                sentence.GroupNames = sentence.GroupLinks != null
                    ? string.Join(", ", sentence.GroupLinks.Select(gl => gl.Group?.GroupName ?? ""))
                    : "";
            }

            Sentences = new ObservableCollection<Sentence>(sentences);
        }

        private async Task SaveNewSentenceAsync()
        {
            if (string.IsNullOrWhiteSpace(NewEnglishSentence))
            {
                MessageBox.Show("Please enter an English sentence.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NewGroupId == 0)
            {
                MessageBox.Show("Please select a group for this sentence.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var sentence = new Sentence
                {
                    EnglishSentence = NewEnglishSentence.Trim(),
                    ArabicTranslation = NewArabicTranslation?.Trim() ?? "",
                    DifficultyLevel = NewDifficulty,
                    Notes = NewNotes?.Trim() ?? "",
                    YouGlishUrl = _youGlishService.GenerateUrl(NewEnglishSentence.Trim()),
                    NextReviewDate = DateTime.UtcNow
                };

                await _sentenceRepository.AddAsync(sentence);
                await _groupRepository.CopySentenceToGroupAsync(sentence.Id, NewGroupId);

                // Reset form
                NewEnglishSentence = "";
                NewArabicTranslation = "";
                NewDifficulty = DifficultyLevel.Beginner;
                NewNotes = "";
                IsAddingNew = false;

                await SearchSentencesAsync();
                MessageBox.Show("Sentence added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save sentence: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteSentenceAsync(Sentence? sentence)
        {
            if (sentence == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete this sentence?\n\n{sentence.EnglishSentence}",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _sentenceRepository.DeleteAsync(sentence);
                Sentences.Remove(sentence);
            }
        }

        private void OpenYouGlish(Sentence? sentence)
        {
            if (sentence == null) return;
            _youGlishService.OpenInBrowser(sentence.EnglishSentence);
        }

        private void PlayPronunciation(Sentence? sentence)
        {
            if (sentence == null) return;
            // Use Windows Speech Synthesis
            try
            {
                var synth = new System.Speech.Synthesis.SpeechSynthesizer();
                synth.SpeakAsync(sentence.EnglishSentence);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not play pronunciation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task TranslateNewSentenceAsync()
        {
            if (string.IsNullOrWhiteSpace(NewEnglishSentence))
            {
                MessageBox.Show("Please enter an English sentence first.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsTranslating = true;
            try
            {
                var translation = await _translationService.TranslateToArabicAsync(NewEnglishSentence);
                if (!string.IsNullOrEmpty(translation))
                {
                    NewArabicTranslation = translation;
                }
                else
                {
                    MessageBox.Show("Translation failed. Please check your internet connection and try again.", "Translation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            finally
            {
                IsTranslating = false;
            }
        }

        private void ClearFilters()
        {
            SearchText = "";
            SelectedGroupId = null;
            SelectedDifficulty = null;
        }
    }
}
