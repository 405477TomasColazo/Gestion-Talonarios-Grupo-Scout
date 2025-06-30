using System.Windows;
using GestionTalonarios.UI.ViewModels;

namespace GestionTalonarios.UI.Views
{
    public partial class CommissionConfigWindow : Window
    {
        private readonly CommissionConfigViewModel _viewModel;

        public CommissionConfigWindow(CommissionConfigViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadConfigAsync();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}