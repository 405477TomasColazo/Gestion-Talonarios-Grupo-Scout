using System.Windows;
using GestionTalonarios.UI.ViewModels;

namespace GestionTalonarios.UI.Views
{
    public partial class FinancialReportsWindow : Window
    {
        private readonly FinancialReportsViewModel _viewModel;

        public FinancialReportsWindow(FinancialReportsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadDataAsync();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.RefreshDataAsync();
        }

        private void BtnConfig_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OpenConfigurationCommand.Execute(null);
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ExportReportCommand.Execute(null);
        }
    }
}