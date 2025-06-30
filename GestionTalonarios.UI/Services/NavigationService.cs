using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GestionTalonarios.UI.Interfaces;
using GestionTalonarios.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Microsoft.Win32;

namespace GestionTalonarios.UI.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public bool? ShowDialog<T>() where T : Window
        {
            var window = _serviceProvider.GetService<T>();
            return window.ShowDialog();
        }

        public string? ShowOpenFileDialog(string filter = "Archivos Excel|*.xlsx;*.xls", string title = "Seleccionar archivo")
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = filter,
                Title = title,
                Multiselect = false
            };

            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }

        public void ShowMessageBox(string message, string title = "Información", MessageBoxImage icon = MessageBoxImage.Information)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        }

        public void ShowFinancialReportsWindow()
        {
            var window = _serviceProvider.GetService<FinancialReportsWindow>();
            window?.Show();
        }

        public void ShowCommissionConfigWindow()
        {
            var window = _serviceProvider.GetService<CommissionConfigWindow>();
            window?.ShowDialog();
        }
    }
}
