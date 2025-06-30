using Dapper;
using GestionTalonarios.Core.Interfaces;
using GestionTalonarios.Core.Models;
using GestionTalonarios.Data.Context;
using Microsoft.Extensions.Logging;

namespace GestionTalonarios.Data.Repositories
{
    public class CommissionRepository : ICommissionRepository
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly ILogger<CommissionRepository> _logger;

        public CommissionRepository(ConnectionFactory connectionFactory, ILogger<CommissionRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<CommissionConfig?> GetActiveConfigAsync()
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                await connection.OpenAsync();

                // Crear tabla si no existe
                await CreateTableIfNotExistsAsync(connection);

                // Primero intentamos obtener la configuración de la tabla con mapeo explícito
                const string sql = @"
                    SELECT TOP 1 
                        id as Id,
                        production_cost as ProductionCost,
                        sale_price as SalePrice,
                        tier1_limit as Tier1Limit,
                        tier2_limit as Tier2Limit,
                        tier1_group_percentage as Tier1GroupPercentage,
                        tier2_group_percentage as Tier2GroupPercentage,
                        tier3_group_percentage as Tier3GroupPercentage,
                        is_active as IsActive,
                        created_date as CreatedDate
                    FROM CommissionConfig 
                    WHERE is_active = 1 
                    ORDER BY created_date DESC";

                var config = await connection.QueryFirstOrDefaultAsync<CommissionConfig>(sql);
                
                _logger.LogInformation($"Configuración obtenida de BD: ProductionCost={config?.ProductionCost}, SalePrice={config?.SalePrice}");
                
                return config;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener configuración activa de comisiones, usando valores por defecto");
                return null;
            }
        }

        public async Task<CommissionConfig> SaveConfigAsync(CommissionConfig config)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                await connection.OpenAsync();

                // Crear tabla si no existe
                await CreateTableIfNotExistsAsync(connection);

                // Desactivar configuraciones anteriores
                await connection.ExecuteAsync(
                    "UPDATE CommissionConfig SET is_active = 0 WHERE is_active = 1");

                // Insertar nueva configuración
                const string sql = @"
                    INSERT INTO CommissionConfig (
                        production_cost, sale_price, tier1_limit, tier2_limit,
                        tier1_group_percentage, tier2_group_percentage, tier3_group_percentage,
                        is_active, created_date
                    ) VALUES (
                        @ProductionCost, @SalePrice, @Tier1Limit, @Tier2Limit,
                        @Tier1GroupPercentage, @Tier2GroupPercentage, @Tier3GroupPercentage,
                        @IsActive, @CreatedDate
                    );
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                config.Id = await connection.QuerySingleAsync<int>(sql, config);
                
                _logger.LogInformation($"Configuración de comisiones guardada con ID: {config.Id}");
                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar configuración de comisiones");
                throw;
            }
        }

        public async Task<List<CommissionConfig>> GetAllConfigsAsync()
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                await connection.OpenAsync();

                const string sql = @"
                    SELECT 
                        id as Id,
                        production_cost as ProductionCost,
                        sale_price as SalePrice,
                        tier1_limit as Tier1Limit,
                        tier2_limit as Tier2Limit,
                        tier1_group_percentage as Tier1GroupPercentage,
                        tier2_group_percentage as Tier2GroupPercentage,
                        tier3_group_percentage as Tier3GroupPercentage,
                        is_active as IsActive,
                        created_date as CreatedDate
                    FROM CommissionConfig 
                    ORDER BY created_date DESC";
                var configs = await connection.QueryAsync<CommissionConfig>(sql);
                
                return configs.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las configuraciones de comisiones");
                throw;
            }
        }

        private async Task CreateTableIfNotExistsAsync(System.Data.IDbConnection connection)
        {
            const string createTableSql = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CommissionConfig' AND xtype='U')
                BEGIN
                    CREATE TABLE CommissionConfig (
                        id INT IDENTITY(1,1) PRIMARY KEY,
                        production_cost DECIMAL(10,2) NOT NULL,
                        sale_price DECIMAL(10,2) NOT NULL,
                        tier1_limit INT NOT NULL DEFAULT 5,
                        tier2_limit INT NOT NULL DEFAULT 10,
                        tier1_group_percentage DECIMAL(3,2) NOT NULL DEFAULT 1.0,
                        tier2_group_percentage DECIMAL(3,2) NOT NULL DEFAULT 0.5,
                        tier3_group_percentage DECIMAL(3,2) NOT NULL DEFAULT 0.0,
                        is_active BIT NOT NULL DEFAULT 1,
                        created_date DATETIME NOT NULL DEFAULT GETDATE()
                    )
                END";

            await connection.ExecuteAsync(createTableSql);
        }
    }
}