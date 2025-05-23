using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GestionTalonarios.UI.Views
{
    public partial class EditarObservacionesDialog : Window
    {
        public string Observaciones
        {
            get => txtObservaciones.Text;
            set => txtObservaciones.Text = value ?? string.Empty;
        }

        public EditarObservacionesDialog(string observacionesActuales, string ticketInfo = null)
        {
            InitializeComponent();

            // Configurar observaciones iniciales
            Observaciones = observacionesActuales;

            // Configurar información del ticket si se proporciona
            if (!string.IsNullOrEmpty(ticketInfo))
            {
                txtTicketInfo.Text = ticketInfo;
            }
            else
            {
                txtTicketInfo.Visibility = Visibility.Collapsed;
            }

            // Enfocar el TextBox y seleccionar todo el texto
            Loaded += (s, e) =>
            {
                txtObservaciones.Focus();
                txtObservaciones.SelectAll();
            };
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
