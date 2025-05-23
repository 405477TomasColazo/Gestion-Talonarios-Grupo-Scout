using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GestionTalonarios.Core.Interfaces;
using GestionTalonarios.UI.Helpers;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows;
using GestionTalonarios.Core.Enums;
using GestionTalonarios.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using GestionTalonarios.UI.Interfaces;

namespace GestionTalonarios.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ITicketService _ticketService;
        private readonly DispatcherTimer _updateTimer;
        private readonly INavigationService _navigationService;

        // Propiedades observables
        private ObservableCollection<TicketViewModel> _tickets;
        private TicketViewModel _selectedTicket;
        private string _searchText;
        private SearchType _tipoBusqueda;
        private bool _isBusy;
        private int _porcionesRestantes;
        private int _porcionesEnVentaTradicionales;
        private int _porcionesEnVentaVeganas;
        private int _porcionesTradicionalesRestantes;
        private int _porcionesVeganasRestantes;
        private string _statusMessage;

        public ObservableCollection<TicketViewModel> Tickets
        {
            get => _tickets;
            set => SetProperty(ref _tickets, value);
        }

        public TicketViewModel SelectedTicket
        {
            get => _selectedTicket;
            set
            {
                if (SetProperty(ref _selectedTicket, value))
                {
                    // Refrescar estado de los comandos cuando cambia la selección
                    (MarcarPagadoCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (MarcarEntregadoCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (EditarObservacionesCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public SearchType TipoBusqueda
        {
            get => _tipoBusqueda;
            set => SetProperty(ref _tipoBusqueda, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public int PorcionesRestantes
        {
            get => _porcionesRestantes;
            set => SetProperty(ref _porcionesRestantes, value);
        }
        public int PorcionesEnVentaTradicionales
        {
            get => _porcionesEnVentaTradicionales;
            set => SetProperty(ref _porcionesEnVentaTradicionales, value);
        }
        public int PorcionesEnVentaVeganas
        {
            get => _porcionesEnVentaVeganas;
            set => SetProperty(ref _porcionesEnVentaVeganas, value);
        }

        public int PorcionesTradicionalesRestantes
        {
            get => _porcionesTradicionalesRestantes;
            set => SetProperty(ref _porcionesTradicionalesRestantes, value);
        }

        public int PorcionesVeganasRestantes
        {
            get => _porcionesVeganasRestantes;
            set => SetProperty(ref _porcionesVeganasRestantes, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        // Comandos
        public ICommand SearchCommand { get; }
        public ICommand MarcarPagadoCommand { get; }
        public ICommand MarcarEntregadoCommand { get; }
        public ICommand NuevoTicketCommand { get; }
        public ICommand VerDetallePorcionesCommand { get; }
        public ICommand LimpiarFiltrosCommand { get; }
        public ICommand EditarObservacionesCommand { get; }

        // Constructor
        public MainViewModel(ITicketService ticketService,INavigationService navigationService)
        {
            _ticketService = ticketService;
            _navigationService = navigationService;

            // Inicializar colecciones
            Tickets = new ObservableCollection<TicketViewModel>();

            // Inicializar valores predeterminados
            TipoBusqueda = SearchType.All;

            // Inicializar comandos
            SearchCommand = new RelayCommand(async param => await ExecuteSearchAsync());
            MarcarPagadoCommand = new RelayCommand(
                async param => await ExecuteMarcarPagadoAsync(),
                param => CanExecuteMarcarPagado());
            MarcarEntregadoCommand = new RelayCommand(
                async param => await ExecuteMarcarEntregadoAsync(),
                param => CanExecuteMarcarEntregado());
            NuevoTicketCommand = new RelayCommand(ExecuteNuevoTicket);
            VerDetallePorcionesCommand = new RelayCommand(async param => await ExecuteVerDetallePorcionesAsync());
            LimpiarFiltrosCommand = new RelayCommand(async param => await ExecuteLimpiarFiltrosAsync());
            EditarObservacionesCommand = new RelayCommand(
                param => ExecuteEditarObservaciones(),
                param => CanExecuteEditarObservaciones());

            // Inicializar temporizador para actualizar cada 30 segundos
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _updateTimer.Tick += async (s, e) => await ActualizarPorcionesRestantesAsync();
            _updateTimer.Start();

            // Cargar datos iniciales
            LoadTicketsAsync();
        }

        private bool CanExecuteEditarObservaciones()
        {
            return SelectedTicket != null;
        }

        private void ExecuteEditarObservaciones()
        {
            if (SelectedTicket == null) return;

            try
            {
                // Crear una ventana de diálogo para editar observaciones
                var observacionesActuales = SelectedTicket.Observations ?? string.Empty;

                var dialog = new Views.EditarObservacionesDialog(observacionesActuales)
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                if (dialog.ShowDialog() == true)
                {
                    var nuevasObservaciones = dialog.Observaciones;

                    // Actualizar las observaciones del ticket
                    Task.Run(async () =>
                    {
                        try
                        {
                            await _ticketService.UpdateObservationsAsync(SelectedTicket.Id, nuevasObservaciones);

                            // Actualizar en la UI thread
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                SelectedTicket.Observations = nuevasObservaciones;
                                OnTicketUpdated(SelectedTicket);
                                StatusMessage = $"Observaciones actualizadas para ticket #{SelectedTicket.Code}";
                            });
                        }
                        catch (Exception ex)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                StatusMessage = "Error al actualizar observaciones.";
                                MessageBox.Show($"Error al actualizar observaciones: {ex.Message}",
                                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Error al abrir editor de observaciones.";
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteNuevoTicket(object parameter)
        {
            try
            {
                if (_navigationService.ShowDialog<NuevoTicketWindow>() == true)
                {
                    // Recargar tickets y contador
                    LoadTicketsAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Error al abrir ventana de nuevo ticket";
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Método para liberar recursos
        public void Cleanup()
        {
            _updateTimer.Stop();
        }

        private async Task LoadTicketsAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Cargando tickets...";

                var tickets = await _ticketService.GetAllAsync();

                Tickets.Clear();
                foreach (var ticket in tickets)
                {
                    Tickets.Add(new TicketViewModel(ticket));
                }

                StatusMessage = $"Se cargaron {Tickets.Count} tickets.";

                // Actualizar contador de porciones
                await ActualizarPorcionesRestantesAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = "Error al cargar tickets.";
                MessageBox.Show($"Error al cargar tickets: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExecuteSearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadTicketsAsync();
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "Buscando...";

                var tickets = await _ticketService.GetBySearchAsync(SearchText, TipoBusqueda);

                Tickets.Clear();
                foreach (var ticket in tickets)
                {
                    Tickets.Add(new TicketViewModel(ticket));
                }

                StatusMessage = $"Se encontraron {Tickets.Count} resultados.";

                await ActualizarPorcionesRestantesAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = "Error en la búsqueda.";
                MessageBox.Show($"Error al buscar tickets: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanExecuteMarcarPagado()
        {
            return SelectedTicket != null && !SelectedTicket.IsPaid;
        }

        private async Task ExecuteMarcarPagadoAsync()
        {
            if (SelectedTicket == null) return;

            try
            {
                IsBusy = true;

                // Confirmar acción
                var result = MessageBox.Show(
                    $"¿Está seguro de marcar como PAGADO el ticket #{SelectedTicket.Id}?\n\n" +
                    $"Cliente: {SelectedTicket.ClientName}\n" +
                    $"Cantidad: {SelectedTicket.Quantity} porciones\n" +
                    $"Monto: ${SelectedTicket.TotalAmount:N2}",
                    "Confirmar pago",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                await _ticketService.PayTicketAsync(SelectedTicket.Id);

                // Actualizar modelo local
                SelectedTicket.IsPaid = true;
                SelectedTicket.PaymentDate = DateTime.Now;

                // Refrescar comandos
                (MarcarPagadoCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (MarcarEntregadoCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (EditarObservacionesCommand as RelayCommand)?.RaiseCanExecuteChanged();

                // Actualizar contador de porciones
                await ActualizarPorcionesRestantesAsync();
                // Disparar evento de notificación
                OnTicketUpdated(SelectedTicket);


                StatusMessage = $"Ticket #{SelectedTicket.Id} marcado como PAGADO.";

                MessageBox.Show(
                    $"El ticket #{SelectedTicket.Id} ha sido marcado como PAGADO.",
                    "Pago registrado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = "Error al marcar como pagado.";
                MessageBox.Show($"Error al marcar como pagado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event EventHandler<TicketViewModel> TicketUpdated;

        protected virtual void OnTicketUpdated(TicketViewModel ticket)
        {
            TicketUpdated?.Invoke(this, ticket);
        }

        private bool CanExecuteMarcarEntregado()
        {
            return SelectedTicket != null && SelectedTicket.IsPaid && !SelectedTicket.IsDelivered;
        }

        private async Task ExecuteMarcarEntregadoAsync()
        {
            if (SelectedTicket == null) return;

            try
            {
                IsBusy = true;

                // Confirmar acción
                var result = MessageBox.Show(
                    $"¿Está seguro de marcar como ENTREGADO el ticket #{SelectedTicket.Id}?\n\n" +
                    $"Cliente: {SelectedTicket.ClientName}\n" +
                    $"Cantidad: {SelectedTicket.Quantity} porciones",
                    "Confirmar entrega",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                await _ticketService.DeliverTicketAsync(SelectedTicket.Id);

                // Actualizar modelo local
                SelectedTicket.IsDelivered = true;
                SelectedTicket.WithdrawalTime = DateTime.Now;

                // Refrescar comandos
                (MarcarEntregadoCommand as RelayCommand)?.RaiseCanExecuteChanged();

                // Actualizar contador de porciones
                await ActualizarPorcionesRestantesAsync();
                OnTicketUpdated(SelectedTicket);

                StatusMessage = $"Ticket #{SelectedTicket.Id} marcado como ENTREGADO.";

                MessageBox.Show(
                    $"El ticket #{SelectedTicket.Id} ha sido marcado como ENTREGADO.",
                    "Entrega registrada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = "Error al marcar como entregado.";
                MessageBox.Show($"Error al marcar como entregado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExecuteNuevoTicketAsync()
        {
            // Esta implementación dependerá de cómo quieras manejar la creación de nuevos tickets
            // Por ejemplo, podrías abrir una nueva ventana/diálogo
            MessageBox.Show("La funcionalidad para crear nuevos tickets se implementará próximamente.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task ExecuteVerDetallePorcionesAsync()
        {
            try
            {
                IsBusy = true;

                // Obtenemos los tickets pagados pero no entregados
                var ticketsPendientes = Tickets.Where(t => t.IsPaid && !t.IsDelivered).ToList();

                if (ticketsPendientes.Count == 0)
                {
                    MessageBox.Show(
                        "No hay porciones pendientes de entrega.",
                        "Detalle de Porciones",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                // Construir mensaje detallado
                var mensaje = "Detalle de porciones pendientes de entrega:\n\n";

                foreach (var ticket in ticketsPendientes)
                {
                    mensaje += $"Ticket #{ticket.Id} - {ticket.ClientName}: {ticket.Quantity} porciones\n";
                }

                mensaje += $"\nTotal: {PorcionesRestantes} porciones pendientes de entrega.";

                MessageBox.Show(
                    mensaje,
                    "Detalle de Porciones",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener detalle de porciones: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExecuteLimpiarFiltrosAsync()
        {
            SearchText = string.Empty;
            TipoBusqueda = SearchType.All;
            await LoadTicketsAsync();
        }

        public async Task ActualizarPorcionesRestantesAsync()
        {
            try
            {
                var porcionesData = await _ticketService.ObtenerDetallesPorcionesRestantesAsync();
                var porcionesVenta = await _ticketService.ObtenerPorcionesEnVentaAsync();
                PorcionesEnVentaTradicionales = porcionesVenta.PorcionesRestantesTradicionales;
                PorcionesEnVentaVeganas = porcionesVenta.PorcionesRestantesVeganas;
                PorcionesTradicionalesRestantes = porcionesData.PorcionesTradicionalesRestantes;
                PorcionesVeganasRestantes = porcionesData.PorcionesVeganasRestantes;
                PorcionesRestantes = porcionesData.TotalPorcionesRestantes;
            }
            catch (Exception ex)
            {
                // Simplemente registrar el error, no mostrar mensaje al usuario por cada actualización
                System.Diagnostics.Debug.WriteLine($"Error al actualizar porciones restantes: {ex.Message}");
            }
        }
        // En MainViewModel.cs
        public string DebugSelectedTicketInfo
        {
            get
            {
                if (SelectedTicket == null)
                    return "No hay ticket seleccionado";

                return $"Ticket #{SelectedTicket.Id}\n" +
                       $"Vendedor: {SelectedTicket.SellerName}\n" +
                       $"Cliente: {SelectedTicket.ClientName}\n" +
                       $"Cantidad: {SelectedTicket.QuantityDisplay}\n" +
                       $"Pagado: {(SelectedTicket.IsPaid ? "SÍ" : "NO")}\n" +
                       $"Entregado: {(SelectedTicket.IsDelivered ? "SÍ" : "NO")}";
            }
        }


    }
}
