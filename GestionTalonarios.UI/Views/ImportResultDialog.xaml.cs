using System.Windows;
using GestionTalonarios.Core.DTOs;

namespace GestionTalonarios.UI.Views
{
    public partial class ImportResultDialog : Window
    {
        public ImportResultDialog(ImportResult result)
        {
            InitializeComponent();
            LoadResult(result);
        }

        private void LoadResult(ImportResult result)
        {
            // Cargar resumen
            txtTotalRows.Text = result.TotalRows.ToString();
            txtSuccessful.Text = result.SuccessfulImports.ToString();
            txtFailed.Text = result.FailedImports.ToString();
            txtProcessingTime.Text = result.ProcessingTime.TotalSeconds.ToString("F2");

            // Mensaje de resumen
            if (result.IsSuccess)
            {
                txtSummaryMessage.Text = $"✅ ¡Importación completada exitosamente! Se procesaron {result.TotalRows} filas y se importaron {result.SuccessfulImports} tickets correctamente.";
                txtRecommendations.Text = "Todos los tickets se han agregado a la base de datos y están listos para su uso.";
            }
            else
            {
                txtSummaryMessage.Text = $"⚠️ Importación completada con errores. Se procesaron {result.TotalRows} filas: {result.SuccessfulImports} exitosas y {result.FailedImports} fallidas.";
                txtRecommendations.Text = "Revise los errores detallados en la pestaña correspondiente. Corrija el archivo Excel y vuelva a intentar la importación de las filas fallidas.";
            }

            // Cargar errores
            if (result.Errors.Any())
            {
                dgErrors.ItemsSource = result.Errors;
                tabErrorDetails.Visibility = Visibility.Visible;
            }
            else
            {
                tabErrorDetails.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}