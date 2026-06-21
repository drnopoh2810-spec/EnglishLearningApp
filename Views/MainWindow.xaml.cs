using EnglishLearningApp.ViewModels;
using System.Windows;

namespace EnglishLearningApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
