using System.Windows;
using System.Windows.Input;
using GestionTalonarios.Core.DTOs;
using GestionTalonarios.Core.Interfaces;
using GestionTalonarios.UI.Helpers;
using GestionTalonarios.UI.Interfaces;
using Microsoft.Extensions.Logging;
using PropertyChanged;

namespace GestionTalonarios.UI.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class FinancialReportsViewModel : ViewModelBase
    {
        private readonly ICommissionService _commissionService;
        private readonly INavigationService _navigationService;
        private readonly ILogger<FinancialReportsViewModel> _logger;

        public FinancialReportsViewModel(
            ICommissionService commissionService,
            INavigationService navigationService,
            ILogger<FinancialReportsViewModel> logger)
        {
            _commissionService = commissionService;
            _navigationService = navigationService;
            _logger = logger;

            FinancialSummary = new FinancialSummaryDto();
            StatusMessage = "Listo para cargar datos";

            // Commands
            RefreshCommand = new RelayCommand(async _ => await RefreshDataAsync());
            OpenConfigurationCommand = new RelayCommand(_ => OpenConfiguration());
            ExportReportCommand = new RelayCommand(_ => ExportReport());
        }

        #region Properties

        public FinancialSummaryDto FinancialSummary { get; set; }
        public string StatusMessage { get; set; }
        public bool IsLoading { get; set; }

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; }
        public ICommand OpenConfigurationCommand { get; }
        public ICommand ExportReportCommand { get; }

        #endregion

        #region Methods

        public async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Cargando datos financieros...";

                var summary = await _commissionService.GetFinancialSummaryAsync();
                FinancialSummary = summary;

                StatusMessage = $"Datos cargados correctamente. {summary.SellerCommissions.Count} vendedores encontrados.";
                
                _logger.LogInformation("Datos financieros cargados exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar datos financieros");
                StatusMessage = "Error al cargar los datos financieros";
                
                MessageBox.Show(
                    "No se pudieron cargar los datos financieros. Verifique la conexión a la base de datos.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task RefreshDataAsync()
        {
            await LoadDataAsync();
        }

        private void OpenConfiguration()
        {
            try
            {
                _navigationService.ShowCommissionConfigWindow();
                // Después de cerrar la configuración, actualizar los datos
                _ = Task.Run(async () => await RefreshDataAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al abrir ventana de configuración");
                MessageBox.Show(
                    "No se pudo abrir la ventana de configuración.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ExportReport()
        {
            try
            {
                StatusMessage = "Exportando reporte...";
                
                // TODO: Implementar exportación a Excel/PDF
                MessageBox.Show(
                    "Funcionalidad de exportación en desarrollo.",
                    "Información",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                StatusMessage = "Listo";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar reporte");
                StatusMessage = "Error al exportar reporte";
                
                MessageBox.Show(
                    "No se pudo exportar el reporte.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion
    }
}