using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using GestionTalonarios.Core.Interfaces;
using GestionTalonarios.Data.Context;
using Microsoft.Extensions.Logging;


namespace GestionTalonarios.Data.Repositories
{
    public abstract class RepositoryBase<T> : IRepository<T> where T : class
    {
        protected readonly ConnectionFactory _connectionFactory;
        protected readonly ILogger _logger;
        protected readonly string _tableName;
        protected readonly string _idColumn;

        public RepositoryBase(ConnectionFactory connectionFactory, ILogger logger, string tableName, string idColumn = "id")
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
            _tableName = tableName;
            _idColumn = idColumn;
        }

        public virtual async Task<int> AddAync(T entity)
        {
            try
            {
                using(var connection = _connectionFactory.CreateConnection())
                {
                    return await connection.InsertAsync(entity);                }
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, $"Error al agregar entidad en la tabla {_tableName}");
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    string sql = $"DELETE FROM {_tableName} WHERE {_idColumn} = @Id";
                    await connection.ExecuteAsync(sql, new { Id = id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar entidad con ID {id} de la tabla {_tableName}");
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    return await connection.QueryAsync<T>($"SELECT * FROM {_tableName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener todos los registros de {_tableName}");
                throw;
            }
        }

        public async Task<T> GetByIdAsync(int id)
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    string sql = $"SELECT * FROM {_tableName} WHERE {_idColumn} = @Id";
                    return await connection.QuerySingleOrDefaultAsync<T>(sql, new { Id = id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener entidad con ID {id} de la tabla {_tableName}");
                throw;
            }
        }

        public async Task UpdateAsync(T entity)
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    await connection.UpdateAsync(entity);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al agregar entidad en la tabla {_tableName}");
                throw;
            }
        }
    }
}
