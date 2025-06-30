using System.Windows;
using System.Windows.Input;
using GestionTalonarios.Core.Interfaces;
using GestionTalonarios.Core.Models;
using GestionTalonarios.UI.Helpers;
using Microsoft.Extensions.Logging;
using PropertyChanged;

namespace GestionTalonarios.UI.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class CommissionConfigViewModel : ViewModelBase
    {
        private readonly ICommissionService _commissionService;
        private readonly ILogger<CommissionConfigViewModel> _logger;
        private CommissionConfig _originalConfig;

        public CommissionConfigViewModel(
            ICommissionService commissionService,
            ILogger<CommissionConfigViewModel> logger)
        {
            _commissionService = commissionService;
            _logger = logger;

            // Initialize with default values
            ProductionCost = 200m;
            SalePrice = 350m;
            Tier1Limit = 5;
            Tier2Limit = 10;
            
            StatusMessage = "Listo para configurar";

            // Commands
            SaveCommand = new RelayCommand(async _ => await SaveConfigAsync(), _ => CanSave());

            // Update calculated properties when costs change
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ProductionCost) || e.PropertyName == nameof(SalePrice))
                {
                    OnPropertyChanged(nameof(ProfitPerPortion));
                    UpdateExampleCalculations();
                }
            };
        }

        #region Properties

        public decimal ProductionCost { get; set; }
        public decimal SalePrice { get; set; }
        public int Tier1Limit { get; set; }
        public int Tier2Limit { get; set; }
        public string StatusMessage { get; set; }
        public bool IsLoading { get; set; }

        // Calculated Properties
        public decimal ProfitPerPortion => SalePrice - ProductionCost;

        // Example Calculations (for a seller with 12 portions)
        public decimal ExampleTier1Group => 5 * ProfitPerPortion * 1.0m; // 5 portions * 100% group
        public decimal ExampleTier2Group => 5 * ProfitPerPortion * 0.5m; // 5 portions * 50% group
        public decimal ExampleTier2Seller => 5 * ProfitPerPortion * 0.5m; // 5 portions * 50% seller
        public decimal ExampleTier3Seller => 2 * ProfitPerPortion * 1.0m; // 2 portions * 100% seller
        public decimal ExampleTotalSeller => ExampleTier2Seller + ExampleTier3Seller;

        #endregion

        #region Commands

        public ICommand SaveCommand { get; }

        #endregion

        #region Methods

        public async Task LoadConfigAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Cargando configuración...";

                var config = await _commissionService.GetCommissionConfigAsync();
                _originalConfig = config;

                ProductionCost = config.ProductionCost;
                SalePrice = config.SalePrice;
                Tier1Limit = config.Tier1Limit;
                Tier2Limit = config.Tier2Limit;

                StatusMessage = "Configuración cargada";
                _logger.LogInformation("Configuración de comisiones cargada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar configuración de comisiones");
                StatusMessage = "Error al cargar configuración";
                
                MessageBox.Show(
                    "No se pudo cargar la configuración de comisiones. Se usarán valores por defecto.",
                    "Advertencia",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveConfigAsync()
        {
            try
            {
                if (!ValidateInput())
                    return;

                IsLoading = true;
                StatusMessage = "Guardando configuración...";

                var config = new CommissionConfig
                {
                    ProductionCost = ProductionCost,
                    SalePrice = SalePrice,
                    Tier1Limit = Tier1Limit,
                    Tier2Limit = Tier2Limit,
                    Tier1GroupPercentage = 1.0m,
                    Tier2GroupPercentage = 0.5m,
                    Tier3GroupPercentage = 0.0m,
                    IsActive = true
                };

                await _commissionService.SaveCommissionConfigAsync(config);

                StatusMessage = "Configuración guardada exitosamente";
                _logger.LogInformation($"Configuración guardada: Costo={ProductionCost}, Precio={SalePrice}");

                MessageBox.Show(
                    "La configuración se ha guardado correctamente.",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Close the window
                if (Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w.DataContext == this) is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar configuración de comisiones");
                StatusMessage = "Error al guardar configuración";
                
                MessageBox.Show(
                    "No se pudo guardar la configuración. Intente nuevamente.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool ValidateInput()
        {
            if (ProductionCost <= 0)
            {
                MessageBox.Show("El costo de producción debe ser mayor a cero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (SalePrice <= 0)
            {
                MessageBox.Show("El precio de venta debe ser mayor a cero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (SalePrice <= ProductionCost)
            {
                MessageBox.Show("El precio de venta debe ser mayor al costo de producción.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool CanSave()
        {
            return !IsLoading && ProductionCost > 0 && SalePrice > ProductionCost;
        }

        private void UpdateExampleCalculations()
        {
            OnPropertyChanged(nameof(ExampleTier1Group));
            OnPropertyChanged(nameof(ExampleTier2Group));
            OnPropertyChanged(nameof(ExampleTier2Seller));
            OnPropertyChanged(nameof(ExampleTier3Seller));
            OnPropertyChanged(nameof(ExampleTotalSeller));
        }

        #endregion
    }
}