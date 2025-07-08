using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using GestionTalonarios.Core.Interfaces;
using GestionTalonarios.Core.Services;
using GestionTalonarios.Data.Context;
using GestionTalonarios.Data.Repositories;
using GestionTalonarios.UI.Interfaces;
using GestionTalonarios.UI.Services;
using GestionTalonarios.UI.ViewModels;
using GestionTalonarios.UI.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using OfficeOpenXml;

namespace GestionTalonarios.UI
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;
        private IConfiguration _configuration;

        /// <summary>
        /// Método que se ejecuta al iniciar la aplicación
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Configurar licencia de EPPlus 8.0 para importación de Excel
            ExcelPackage.License.SetNonCommercialPersonal("GestionTalonarios");

            // Configurar servicios y DI antes de iniciar la aplicación
            ConfigureServices();

            // Iniciar la ventana principal
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        /// <summary>
        /// Método para configurar los servicios y la inyección de dependencias
        /// </summary>
        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Configurar el archivo de configuración
            ConfigureAppSettings(services);

            // Configurar logging
            ConfigureLogging(services);

            // Configurar acceso a datos
            ConfigureDataAccess(services);

            // Configurar servicios de la aplicación
            ConfigureApplicationServices(services);

            // Configurar ViewModels
            ConfigureViewModels(services);

            // Configurar vistas
            ConfigureViews(services);

            // Construir el proveedor de servicios
            _serviceProvider = services.BuildServiceProvider();
        }


        /// <summary>
        /// Configurar el archivo de configuración (appsettings.json)
        /// </summary>
        private void ConfigureAppSettings(IServiceCollection services)
        {
            // Cargar configuración desde appsettings.json
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.AddSingleton<IConfiguration>(_configuration);
        }

        /// <summary>
        /// Configurar el sistema de logging
        /// </summary>
        private void ConfigureLogging(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddNLog(_configuration);
            });
        }

        /// <summary>
        /// Configurar acceso a datos (conexión a BD y repositorios)
        /// </summary>
        private void ConfigureDataAccess(IServiceCollection services)
        {
            // Registrar fábrica de conexiones
            services.AddSingleton<ConnectionFactory>();

            // Registrar repositorios
            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<ICommissionRepository, CommissionRepository>();
            // Agregar otros repositorios si es necesario
        }

        /// <summary>
        /// Configurar servicios de la aplicación
        /// </summary>
        private void ConfigureApplicationServices(IServiceCollection services)
        {
            // Registrar servicios de negocio
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IImportService, ImportService>();
            services.AddScoped<ICommissionService, CommissionService>();
            services.AddSingleton<INavigationService, NavigationService>();
            // Agregar otros servicios si es necesario
        }

        /// <summary>
        /// Configurar ViewModels
        /// </summary>
        private void ConfigureViewModels(IServiceCollection services)
        {
            // Registrar ViewModels - cambiar a Scoped para que funcione con servicios Scoped
            services.AddScoped<MainViewModel>();
            services.AddScoped<FinancialReportsViewModel>();
            services.AddScoped<CommissionConfigViewModel>();
            // Agregar otros ViewModels si es necesario
        }

        /// <summary>
        /// Configurar vistas
        /// </summary>
        private void ConfigureViews(IServiceCollection services)
        {
            // Registrar ventanas
            services.AddTransient<MainWindow>();
            services.AddTransient<NuevoTicketWindow>();
            services.AddTransient<ImportProgressDialog>();
            services.AddTransient<ImportResultDialog>();
            services.AddTransient<FinancialReportsWindow>();
            services.AddTransient<CommissionConfigWindow>();
            // Agregar otras ventanas si es necesario
        }

        /// <summary>
        /// Método que se ejecuta al cerrar la aplicación
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            // Disponer del ServiceProvider
            _serviceProvider?.Dispose();

            // Llamar al método de la clase base
            base.OnExit(e);
        }

        /// <summary>
        /// Método para manejar excepciones no controladas
        /// </summary>
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // Loguear la excepción
            var logger = _serviceProvider?.GetService<ILogger<App>>();
            logger?.LogError(e.Exception, "Se produjo una excepción no controlada");

            // Mostrar mensaje al usuario
            MessageBox.Show($"Se ha producido un error inesperado:\n{e.Exception.Message}\n\nLa aplicación intentará continuar.",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            // Marcar la excepción como manejada para evitar que la aplicación se cierre
            e.Handled = true;
        }
    }

}
