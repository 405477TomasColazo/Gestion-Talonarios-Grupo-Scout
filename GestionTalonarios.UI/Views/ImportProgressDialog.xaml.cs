using System.Windows;

namespace GestionTalonarios.UI.Views
{
    public partial class ImportProgressDialog : Window
    {
        public ImportProgressDialog()
        {
            InitializeComponent();
        }

        public void UpdateStatus(string message)
        {
            txtStatusMessage.Text = message;
        }
    }
}