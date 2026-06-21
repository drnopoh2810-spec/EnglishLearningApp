using EnglishLearningApp.Helpers;
using EnglishLearningApp.Models;
using EnglishLearningApp.Repositories;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace EnglishLearningApp.ViewModels
{
    public class GroupsViewModel : ViewModelBase, IRefreshable
    {
        private readonly GroupRepository _groupRepository;
        private readonly SentenceRepository _sentenceRepository;

        private ObservableCollection<SentenceGroup> _groups = new();
        private SentenceGroup? _selectedGroup;
        private ObservableCollection<Sentence> _groupSentences = new();
        private bool _isAddingNew;
        private string _newGroupName = "";
        private string _newGroupDescription = "";
        private bool _isEditing;

        public ObservableCollection<SentenceGroup> Groups
        {
            get => _groups;
            set => SetProperty(ref _groups, value);
        }

        public SentenceGroup? SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (SetProperty(ref _selectedGroup, value))
                {
                    _ = LoadGroupSentencesAsync();
                }
            }
        }

        public ObservableCollection<Sentence> GroupSentences
        {
            get => _groupSentences;
            set => SetProperty(ref _groupSentences, value);
        }

        public bool IsAddingNew
        {
            get => _isAddingNew;
            set => SetProperty(ref _isAddingNew, value);
        }

        public string NewGroupName
        {
            get => _newGroupName;
            set => SetProperty(ref _newGroupName, value);
        }

        public string NewGroupDescription
        {
            get => _newGroupDescription;
            set => SetProperty(ref _newGroupDescription, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public ICommand AddNewCommand { get; }
        public ICommand SaveNewCommand { get; }
        public ICommand CancelAddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SaveEditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }

        public GroupsViewModel(GroupRepository groupRepository, SentenceRepository sentenceRepository)
        {
            _groupRepository = groupRepository;
            _sentenceRepository = sentenceRepository;

            AddNewCommand = new RelayCommand(() => { IsAddingNew = true; IsEditing = false; });
            SaveNewCommand = new RelayCommand(async () => await SaveNewGroupAsync());
            CancelAddCommand = new RelayCommand(() => { IsAddingNew = false; IsEditing = false; ClearForm(); });
            EditCommand = new RelayCommand<SentenceGroup>((g) => StartEdit(g));
            SaveEditCommand = new RelayCommand(async () => await SaveEditAsync());
            DeleteCommand = new RelayCommand<SentenceGroup>(async (g) => await DeleteGroupAsync(g));
            RefreshCommand = new RelayCommand(() => Refresh());

            _ = LoadGroupsAsync();
        }

        public void Refresh()
        {
            _ = LoadGroupsAsync();
        }

        private async Task LoadGroupsAsync()
        {
            var groups = await _groupRepository.GetAllWithCountsAsync();
            Groups = new ObservableCollection<SentenceGroup>(groups);
        }

        private async Task LoadGroupSentencesAsync()
        {
            if (SelectedGroup == null)
            {
                GroupSentences.Clear();
                return;
            }

            var sentences = await _sentenceRepository.GetSentencesByGroupAsync(SelectedGroup.Id);
            GroupSentences = new ObservableCollection<Sentence>(sentences);
        }

        private async Task SaveNewGroupAsync()
        {
            if (string.IsNullOrWhiteSpace(NewGroupName))
            {
                MessageBox.Show("Please enter a group name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var group = new SentenceGroup
            {
                GroupName = NewGroupName.Trim(),
                Description = NewGroupDescription?.Trim() ?? ""
            };

            await _groupRepository.AddAsync(group);
            ClearForm();
            IsAddingNew = false;
            await LoadGroupsAsync();
        }

        private void StartEdit(SentenceGroup? group)
        {
            if (group == null) return;
            SelectedGroup = group;
            NewGroupName = group.GroupName;
            NewGroupDescription = group.Description;
            IsEditing = true;
            IsAddingNew = false;
        }

        private async Task SaveEditAsync()
        {
            if (SelectedGroup == null) return;
            if (string.IsNullOrWhiteSpace(NewGroupName))
            {
                MessageBox.Show("Please enter a group name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedGroup.GroupName = NewGroupName.Trim();
            SelectedGroup.Description = NewGroupDescription?.Trim() ?? "";

            await _groupRepository.UpdateAsync(SelectedGroup);
            ClearForm();
            IsEditing = false;
            await LoadGroupsAsync();
        }

        private async Task DeleteGroupAsync(SentenceGroup? group)
        {
            if (group == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{group.GroupName}'?\n\nThis will not delete the sentences in this group, only the group itself.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _groupRepository.DeleteGroupWithSentencesAsync(group.Id);
                await LoadGroupsAsync();
                GroupSentences.Clear();
            }
        }

        private void ClearForm()
        {
            NewGroupName = "";
            NewGroupDescription = "";
        }
    }
}
