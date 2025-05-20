using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GestionTalonarios.Core.Interfaces;
using GestionTalonarios.Core.Models;

namespace GestionTalonarios.UI.Views
{
    /// <summary>
    /// Lógica de interacción para NuevoTicketWindow.xaml
    /// </summary>
    public partial class NuevoTicketWindow : Window
    {

        private readonly ITicketService _ticketService;
        private readonly IClientService _clientService;
        private readonly int _defaultSellerId = 114; // ID del Grupo Scout
        private decimal _precioUnitario = 0.0m;
        public NuevoTicketWindow(ITicketService ticketService, IClientService clientService)
        {
            InitializeComponent();

            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
            _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));

            // Configurar eventos
            txtPorcionesTrad.TextChanged += ActualizarTotal;
            txtPorcionesVeg.TextChanged += ActualizarTotal;

            // Validación numérica
            txtPorcionesTrad.PreviewTextInput += SoloNumeros;
            txtPorcionesVeg.PreviewTextInput += SoloNumeros;
            txtCode.PreviewTextInput += SoloNumeros;

            // Cargar precio por defecto
            CargarPrecioUnitario();

            // Establecer el foco inicial
            Loaded += (s, e) => txtNombreCliente.Focus();
        }
        private async void CargarPrecioUnitario()
        {
            try
            {
                // Obtener el precio unitario de la configuración o base de datos
                _precioUnitario = await _ticketService.ObtenerPrecioUnitarioDefaultAsync();
                txtPrecioUnitario.Text = $"$ {_precioUnitario:N2}";

                // Actualizar total inicial
                ActualizarTotal(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar precio unitario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ActualizarTotal(object sender, TextChangedEventArgs e)
        {
            try
            {
                // Obtener cantidades
                if (!int.TryParse(txtPorcionesTrad.Text, out int cantTrad))
                    cantTrad = 0;

                if (!int.TryParse(txtPorcionesVeg.Text, out int cantVeg))
                    cantVeg = 0;

                // Calcular total
                decimal total = _precioUnitario * (cantTrad + cantVeg);
                txtTotal.Text = $"$ {total:N2}";
            }
            catch (Exception ex)
            {
                txtTotal.Text = "Error";
            }
        }

        private void SoloNumeros(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(txtNombreCliente.Text))
                {
                    MessageBox.Show("Debe ingresar el nombre del cliente", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNombreCliente.Focus();
                    return;
                }


                if (!int.TryParse(txtPorcionesTrad.Text, out int cantTrad) || cantTrad < 0)
                    cantTrad = 0;

                if (!int.TryParse(txtPorcionesVeg.Text, out int cantVeg) || cantVeg < 0)
                    cantVeg = 0;

                if (!int.TryParse(txtCode.Text, out int code) || code <= 0)
                {
                    MessageBox.Show("Debe ingresar un número de talonario válido", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtCode.Focus();
                    return;
                }

                if (await _ticketService.ExistTicketAsync(code))
                {
                    MessageBox.Show($"El número de talonario {code} ya existe en el sistema. Por favor, ingrese otro número.",
                        "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtCode.Focus();
                    return;
                }

                if (cantTrad + cantVeg <= 0)
                {
                    MessageBox.Show("Debe ingresar al menos una porción", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPorcionesTrad.Focus();
                    return;
                }

                // Guardar cliente o buscar si ya existe
                int clientId = await GuardarCliente();
                if (clientId <= 0)
                {
                    MessageBox.Show("Error al guardar los datos del cliente", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Crear ticket
                var ticket = new Ticket
                {
                    SellerId = _defaultSellerId, // Grupo Scout como vendedor
                    ClientId = clientId,
                    TraditionalQty = cantTrad,
                    VeganQty = cantVeg,
                    UnitCost = _precioUnitario,
                    Code = code,
                    IsPaid = chkPagado.IsChecked ?? false,
                    IsDelivered = chkEntregado.IsChecked ?? false,
                    Sold = true,
                    Observations = txtObservaciones.Text,
                    SaleDate = DateTime.Now,
                    PaymentDate = (chkPagado.IsChecked ?? false) ? DateTime.Now : (DateTime?)null,
                    WithdrawalTime = (chkEntregado.IsChecked ?? false) ? DateTime.Now : (DateTime?)null
                };

                // Guardar ticket
                int ticketId = await _ticketService.CrearTicketAsync(ticket);

                if (ticketId > 0)
                {
                    MessageBox.Show($"Ticket #{code} guardado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Error al guardar el ticket", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<int> GuardarCliente()
        {
            try
            {
                // Comprobar si el cliente ya existe
                var clienteExistente = await _clientService.BuscarClientePorNombreAsync(txtNombreCliente.Text);

                if (clienteExistente != null)
                    return clienteExistente.Id;

                // Crear nuevo cliente (sin teléfono)
                var nuevoCliente = new Client
                {
                    Name = txtNombreCliente.Text,
                    Phone = "" // Campo vacío, no es necesario para tickets del día
                };

                return await _clientService.CrearClienteAsync(nuevoCliente);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al procesar cliente: {ex.Message}", ex);
            }
        }
    }
}
