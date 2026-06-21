using System.Windows.Controls;
using System.Windows.Input;
using EnglishLearningApp.ViewModels;

namespace EnglishLearningApp.Views
{
    public partial class ReviewView : UserControl
    {
        public ReviewView()
        {
            InitializeComponent();
            this.KeyDown += ReviewView_KeyDown;
            this.Focusable = true;
            this.Loaded += (s, e) => this.Focus();
        }

        private void ReviewView_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is ReviewViewModel vm)
            {
                if (e.Key == Key.Space)
                {
                    vm.FlipCardCommand.Execute(null);
                    e.Handled = true;
                }
                else if (e.Key == Key.D1 || e.Key == Key.NumPad1)
                {
                    vm.RateAgainCommand.Execute(null);
                    e.Handled = true;
                }
                else if (e.Key == Key.D2 || e.Key == Key.NumPad2)
                {
                    vm.RateHardCommand.Execute(null);
                    e.Handled = true;
                }
                else if (e.Key == Key.D3 || e.Key == Key.NumPad3)
                {
                    vm.RateGoodCommand.Execute(null);
                    e.Handled = true;
                }
                else if (e.Key == Key.D4 || e.Key == Key.NumPad4)
                {
                    vm.RateEasyCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        private void Card_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ReviewViewModel vm)
            {
                vm.FlipCardCommand.Execute(null);
            }
        }
    }
}
