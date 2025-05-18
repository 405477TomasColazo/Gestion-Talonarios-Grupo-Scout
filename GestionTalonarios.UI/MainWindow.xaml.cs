using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GestionTalonarios.Core.Models;
using GestionTalonarios.UI.ViewModels;

namespace GestionTalonarios.UI
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        /// <summary>
        /// Constructor para inyección de dependencias
        /// </summary>
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = _viewModel;

            lvResultados.SelectionChanged += LvResultados_SelectionChanged;
            _viewModel.TicketUpdated += ViewModel_TicketUpdated;

            // Registrar evento para limpiar recursos cuando se cierre la ventana
            Closed += MainWindow_Closed;
        }

        /// <summary>
        /// Constructor para diseñador (no usar en producción)
        /// </summary>
        public MainWindow()
        {
            // Este constructor solo es para el diseñador y no se debería usar en producción
            InitializeComponent();

            // Si se usa este constructor, no tenemos ViewModel inyectado
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                // Crear datos de diseño para vista previa
                DataContext = new MainViewModel(null); // No usar en producción
            }
        }

        /// <summary>
        /// Evento que se dispara cuando se cierra la ventana
        /// </summary>
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // Limpiar recursos y detener temporizadores
            _viewModel?.Cleanup();
        }

        private void ViewModel_TicketUpdated(object sender, TicketViewModel ticket)
        {
            // Actualizar la interfaz cuando un ticket cambia
            UpdateTicketDetails(ticket);
        }

        /// <summary>
        /// Evento para manejar la tecla Enter en el cuadro de búsqueda
        /// </summary>
        private void txtBusqueda_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && _viewModel?.SearchCommand?.CanExecute(null) == true)
            {
                _viewModel.SearchCommand.Execute(null);
                e.Handled = true;
            }
        }

        // En MainWindow.xaml.cs
        private void LvResultados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var ticket = lvResultados.SelectedItem as TicketViewModel;
                UpdateTicketDetails(ticket);
                if (ticket != null)
                {
                    // Mostrar panel de detalles y ocultar mensaje
                    panelDetails.Visibility = Visibility.Visible;
                    txtNoSelection.Visibility = Visibility.Collapsed;

                    // Actualizar campos de información
                    txtTicketNumber.Text = ticket.TicketNumber;
                    txtSellerName.Text = ticket.SellerName;
                    txtClientName.Text = ticket.ClientName;
                    txtQuantity.Text = ticket.QuantityDisplay;
                    txtPaymentStatus.Text = ticket.PaymentStatus;
                    txtDeliveryStatus.Text = ticket.DeliveryStatus;
                    txtUnitPrice.Text = $"$ {ticket.UnitCost:N2}";
                    txtTotalAmount.Text = $"$ {ticket.TotalAmount:N2}";
                    txtObservations.Text = ticket.Observations;

                    // Establecer colores según estados
                    txtPaymentStatus.Foreground = ticket.IsPaid
                        ? new SolidColorBrush(Color.FromRgb(67, 160, 71))  // Verde
                        : new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Rojo
                    txtPaymentStatus.FontWeight = FontWeights.Bold;

                    txtDeliveryStatus.Foreground = ticket.IsDelivered
                        ? new SolidColorBrush(Color.FromRgb(67, 160, 71))  // Verde
                        : new SolidColorBrush(Color.FromRgb(158, 158, 158)); // Gris
                    txtDeliveryStatus.FontWeight = FontWeights.Bold;

                    // Mostrar/ocultar botones según estado
                    btnPay.Visibility = ticket.IsPaid ? Visibility.Collapsed : Visibility.Visible;
                    btnDeliver.Visibility = (ticket.IsPaid && !ticket.IsDelivered) ? Visibility.Visible : Visibility.Collapsed;
                    txtDelivered.Visibility = ticket.IsDelivered ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    // Ocultar panel cuando no hay selección
                    panelDetails.Visibility = Visibility.Collapsed;
                    txtNoSelection.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en selección: {ex.Message}");
                MessageBox.Show($"Error al mostrar detalles: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTicketDetails(TicketViewModel ticket)
        {
            try
            {
                if (ticket != null)
                {
                    // Mostrar panel de detalles y ocultar mensaje
                    panelDetails.Visibility = Visibility.Visible;
                    txtNoSelection.Visibility = Visibility.Collapsed;

                    // Actualizar campos de información
                    txtTicketNumber.Text = ticket.TicketNumber;
                    txtSellerName.Text = ticket.SellerName;
                    txtClientName.Text = ticket.ClientName;
                    txtQuantity.Text = ticket.QuantityDisplay;
                    txtPaymentStatus.Text = ticket.PaymentStatus;
                    txtDeliveryStatus.Text = ticket.DeliveryStatus;
                    txtUnitPrice.Text = $"$ {ticket.UnitCost:N2}";
                    txtTotalAmount.Text = $"$ {ticket.TotalAmount:N2}";
                    txtObservations.Text = ticket.Observations;

                    // Establecer colores según estados
                    txtPaymentStatus.Foreground = ticket.IsPaid
                        ? new SolidColorBrush(Color.FromRgb(67, 160, 71))  // Verde
                        : new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Rojo
                    txtPaymentStatus.FontWeight = FontWeights.Bold;

                    txtDeliveryStatus.Foreground = ticket.IsDelivered
                        ? new SolidColorBrush(Color.FromRgb(67, 160, 71))  // Verde
                        : new SolidColorBrush(Color.FromRgb(158, 158, 158)); // Gris
                    txtDeliveryStatus.FontWeight = FontWeights.Bold;

                    // Mostrar/ocultar botones según estado
                    btnPay.Visibility = ticket.IsPaid ? Visibility.Collapsed : Visibility.Visible;
                    btnDeliver.Visibility = (ticket.IsPaid && !ticket.IsDelivered) ? Visibility.Visible : Visibility.Collapsed;
                    txtDelivered.Visibility = ticket.IsDelivered ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    // Ocultar panel cuando no hay selección
                    panelDetails.Visibility = Visibility.Collapsed;
                    txtNoSelection.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en actualización de UI: {ex.Message}");
                MessageBox.Show($"Error al mostrar detalles: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}