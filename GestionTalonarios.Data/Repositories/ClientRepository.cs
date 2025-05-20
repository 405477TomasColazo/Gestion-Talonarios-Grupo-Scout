using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using GestionTalonarios.Core.Interfaces;
using GestionTalonarios.Core.Models;
using GestionTalonarios.Data.Context;
using Microsoft.Extensions.Logging;

namespace GestionTalonarios.Data.Repositories
{
    public class ClientRepository:RepositoryBase<Client> , IClientRepository
    {
        public ClientRepository(ConnectionFactory connectionFactory, ILogger<ClientRepository> logger)
    : base(connectionFactory, logger, "Clientes")
        {
        }

        public async Task<Client> GetByNameAsync(string name)
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    string sql = "SELECT * FROM Clients WHERE name LIKE @Name";
                    return await connection.QueryFirstOrDefaultAsync<Client>(sql, new { Name = $"%{name}%" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al buscar cliente por nombre '{name}'");
                throw;
            }
        }


    }
}
