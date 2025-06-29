using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using GestionTalonarios.Core.DTOs;
using GestionTalonarios.Core.Enums;
using GestionTalonarios.Core.Interfaces;
using GestionTalonarios.Core.Models;
using GestionTalonarios.Data.Context;
using Microsoft.Extensions.Logging;

namespace GestionTalonarios.Data.Repositories
{
    public class TicketRepository:RepositoryBase<Ticket>,ITicketRepository
    {
        public TicketRepository(ConnectionFactory connectionFactory, ILogger<TicketRepository> logger)
    : base(connectionFactory, logger, "Tickets", "code")
        {
        }

        public async Task DeliverTicketAsync(int code)
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    await connection.OpenAsync(); // IMPORTANTE: Asegúrate de abrir la conexión

                    string sql = @"
                UPDATE Tickets
                SET is_delivered = 1,
                    withdrawal_time = @FechaEntrega
                WHERE code = @Code";

                    await connection.ExecuteAsync(sql, new { Code = code, FechaEntrega = DateTime.Now });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al marcar como entregado el ticket con código {code}");
                throw;
            }
        }

        public Task<Ticket> GetTicketByCode(int code)
        {
            throw new NotImplementedException();
        }

        public async Task PayTicketAsync(int code)
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    await connection.OpenAsync(); // IMPORTANTE: Asegúrate de abrir la conexión

                    string sql = @"
                UPDATE Tickets
                SET is_paid = 1,
                    payment_date = @FechaPago
                WHERE code = @Code";

                    await connection.ExecuteAsync(sql, new { Code = code, FechaPago = DateTime.Now });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al marcar como pagado el ticket con código {code}");
                throw;
            }
        }

        public override async Task<IEnumerable<Ticket>> GetAllAsync()
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    await connection.OpenAsync(); // Asegúrate de abrir la conexión

                    const string sql = @"
                SELECT t.*, s.name as SellerName, s.section as SellerSection, 
                       c.name as ClientName, c.phone as ClientPhone
                FROM Tickets t
                LEFT JOIN Sellers s ON t.seller_id = s.id
                LEFT JOIN Clients c ON t.client_id = c.id
                WHERE t.sold = 1 
                order by code";

                    var tickets = new List<Ticket>();

                    await connection.QueryAsync<dynamic>(sql, param: null).ContinueWith(t =>
                    {
                        var result = t.Result;
                        foreach (var row in result)
                        {
                            var ticket = new Ticket
                            {
                                Code = row.code,
                                SellerId = row.seller_id,
                                ClientId = row.client_id,
                                UnitCost = row.unit_cost,
                                IsPaid = row.is_paid,
                                Sold = row.sold,
                                TraditionalQty = row.traditional_qty,
                                VeganQty = row.vegan_qty,
                                IsDelivered = row.is_delivered,
                                Observations = row.observations,
                                WithdrawalTime = row.withdrawal_time,
                                SaleDate = row.sale_date,
                                PaymentDate = row.payment_date,
                                Seller = new Seller
                                {
                                    Id = row.seller_id,
                                    Name = row.SellerName,
                                    Section = row.SellerSection
                                },
                                Client = new Client
                                {
                                    Id = row.client_id,
                                    Name = row.ClientName,
                                    Phone = row.ClientPhone
                                }
                            };

                            tickets.Add(ticket);
                        }
                    });

                    return tickets;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los tickets");
                throw;
            }
        }

        public async Task<IEnumerable<Ticket>> SearchTicketsAsync(string searchText, SearchType searchType)
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    string sql = @"
                SELECT t.*, s.name as SellerName, s.section as SellerSection, 
                       c.name as ClientName, c.phone as ClientPhone
                FROM Tickets t
                LEFT JOIN Sellers s ON t.seller_id = s.id
                LEFT JOIN Clients c ON t.client_id = c.id
                WHERE t.sold = 1 and ";

                    switch (searchType)
                    {
                        case SearchType.Code:
                            sql += "CAST(code AS NVARCHAR) LIKE @Busqueda order by code";
                            break;
                        case SearchType.Seller:
                            sql += "s.name LIKE @Busqueda order by code";
                            break;
                        case SearchType.Client:
                            sql += "c.name LIKE @Busqueda order by code";
                            break;
                        default: // Todos
                            sql += "(CAST(code AS NVARCHAR) LIKE @Busqueda OR " +
                                   "s.name LIKE @Busqueda OR " +
                                   "c.name LIKE @Busqueda) order by code";
                            break;
                    }

                    var tickets = new List<Ticket>();
                    var result = await connection.QueryAsync<dynamic>(sql, new { Busqueda = $"%{searchText}%" });

                    foreach (var row in result)
                    {
                        var ticket = new Ticket
                        {
                            Code = row.code,
                            SellerId = row.seller_id,
                            ClientId = row.client_id,
                            UnitCost = row.unit_cost,
                            IsPaid = row.is_paid,
                            Sold = row.sold,
                            TraditionalQty = row.traditional_qty,
                            VeganQty = row.vegan_qty,
                            IsDelivered = row.is_delivered,
                            Observations = row.observations,
                            WithdrawalTime = row.withdrawal_time,
                            SaleDate = row.sale_date,
                            PaymentDate = row.payment_date,
                            Seller = new Seller
                            {
                                Id = row.seller_id,
                                Name = row.SellerName,
                                Section = row.SellerSection
                            },
                            Client = new Client
                            {
                                Id = row.client_id,
                                Name = row.ClientName,
                                Phone = row.ClientPhone
                            }
                        };

                        tickets.Add(ticket);
                    }

                    return tickets;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al buscar tickets con texto '{searchText}' y tipo {searchType}");
                throw;
            }
        }

        // Método auxiliar para ejecutar consultas con relaciones
        private async Task<IEnumerable<Ticket>> QueryTicketsWithRelationsAsync(IDbConnection connection, string sql, object parameters = null)
        {
            var ticketDictionary = new Dictionary<int, Ticket>();

            var tickets = await connection.QueryAsync<Ticket, Seller, Client, Ticket>(
                sql,
                (ticket, seller, client) =>
                {
                    if (!ticketDictionary.TryGetValue(ticket.Code, out var existingTicket))
                    {
                        existingTicket = ticket;
                        existingTicket.Seller = seller;
                        existingTicket.Client = client;
                        ticketDictionary.Add(existingTicket.Code, existingTicket);
                    }
                    return existingTicket;
                },
                parameters,
                splitOn: "code,code"
            );

            return ticketDictionary.Values;
        }

        // Método para contar porciones restantes
        public async Task<int> GetRemainingPortionsCountAsync()
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    string sql = @"
                    SELECT SUM(quantity) 
                    FROM Tickets 
                    WHERE sold = 1 AND is_delivered = 0";

                    var result = await connection.ExecuteScalarAsync<int?>(sql);
                    return result ?? 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el conteo de porciones restantes");
                throw;
            }
        }

        public async Task<decimal> GetDefaultUnitPriceAsync()
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    // Opción 1: Consultar un valor de una tabla de configuración
                    /*
                    const string sql = "SELECT value FROM Configuration WHERE key = 'PrecioUnitarioLocro'";
                    var result = await connection.ExecuteScalarAsync<decimal?>(sql);
                    return result ?? 350.0m; // Valor predeterminado si no se encuentra
                    */

                    // Opción 2: Usar el valor más común en tickets existentes
                    const string sql = "SELECT TOP 1 unit_cost FROM Tickets ORDER BY sale_date DESC";
                    var result = await connection.ExecuteScalarAsync<decimal?>(sql);
                    return result ?? 350.0m; // Valor predeterminado si no hay tickets
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener precio unitario predeterminado");
                return 350.0m; // Valor predeterminado en caso de error
            }
        }

        public async Task<PorcionesResumenDto> GetRemainingPortionsDetailAsync()
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    string sql = @"
                SELECT 
                    SUM(traditional_qty) AS PorcionesTradicionalesRestantes,
                    SUM(vegan_qty) AS PorcionesVeganasRestantes
                FROM Tickets 
                WHERE sold = 1 AND is_delivered = 0";

                    var result = await connection.QuerySingleAsync<PorcionesResumenDto>(sql);

                    // Manejar posibles nulos (cuando no hay registros)
                    result.PorcionesTradicionalesRestantes = result.PorcionesTradicionalesRestantes;
                    result.PorcionesVeganasRestantes = result.PorcionesVeganasRestantes;

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles de porciones restantes");
                throw;
            }
        }
        public async Task<PorcionesVentaDto> GetRemainingPortionsToSell()
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    await connection.OpenAsync();
                    string sql = "select 900 - SUM(traditional_qty)PorcionesRestantesTradicionales, 50 - SUM(vegan_qty)PorcionesRestantesVeganas from Tickets";
                    var result = await connection.QuerySingleAsync<PorcionesVentaDto>(sql);
          
                    return result;

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles de porciones restantes");
                throw;
            }
        }

        public async Task<bool> CodeExistsAsync(int code)
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    await connection.OpenAsync();

                    string sql = "SELECT COUNT(1) FROM Tickets WHERE code = @Code";

                    int count = await connection.ExecuteScalarAsync<int>(sql, new { Code = code });

                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al verificar si existe el código {code}");
                throw;
            }
        }

        public override async Task<int> AddAync(Ticket ticket)
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    await connection.OpenAsync();

                    // SQL con nombres exactos de las columnas en la base de datos
                    string sql = @"
                INSERT INTO Tickets (
                    code, seller_id, client_id, unit_cost, 
                    traditional_qty, vegan_qty, is_paid, is_delivered, 
                    sold, observations, sale_date)
                VALUES (
                    @Code, @SellerId, @ClientId, @UnitCost, 
                    @TraditionalQty, @VeganQty, @IsPaid, @IsDelivered, 
                    @Sold, @Observations, @SaleDate);
                SELECT CAST(SCOPE_IDENTITY() as int);";

                    var parameters = new
                    {
                        ticket.Code,
                        ticket.SellerId,
                        ticket.ClientId,
                        ticket.UnitCost,
                        ticket.TraditionalQty,
                        ticket.VeganQty,
                        ticket.IsPaid,
                        ticket.IsDelivered,
                        ticket.Sold,
                        ticket.Observations,
                        SaleDate = ticket.SaleDate != default ? ticket.SaleDate : DateTime.Now
                    };

                    return await connection.ExecuteScalarAsync<int>(sql, parameters);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al agregar ticket en la tabla {_tableName}");
                throw;
            }
        }
        public async Task UpdateObservationsAsync(int code, string observations)
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    await connection.OpenAsync();

                    string sql = @"
                UPDATE Tickets
                SET observations = @Observations
                WHERE code = @Code";

                    var rowsAffected = await connection.ExecuteAsync(sql, new
                    {
                        Code = code,
                        Observations = observations?.Trim()
                    });

                    if (rowsAffected == 0)
                    {
                        throw new InvalidOperationException($"No se encontró el ticket con código {code}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar observaciones del ticket con código {code}");
                throw;
            }
        }

    }

}
