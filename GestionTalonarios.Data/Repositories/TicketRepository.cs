using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
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
    : base(connectionFactory, logger, "Tickets")
        {
        }

        public async Task DeliverTicketAsync(int id)
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    await connection.OpenAsync(); // IMPORTANTE: Asegúrate de abrir la conexión

                    string sql = @"
                UPDATE Tickets
                SET is_delivered= 1,
                    payment_date = @FechaPago
                WHERE id = @Id";

                    await connection.ExecuteAsync(sql, new { Id = id, FechaPago = DateTime.Now });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al marcar como pagado el ticket con ID {id}");
                throw;
            }
        }

        public Task<Ticket> GetTicketByCode(int code)
        {
            throw new NotImplementedException();
        }

        public async Task PayTicketAsync(int id)
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
                WHERE id = @Id";

                    await connection.ExecuteAsync(sql, new { Id = id, FechaPago = DateTime.Now });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al marcar como pagado el ticket con ID {id}");
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
                LEFT JOIN Selleres s ON t.seller_id = s.id
                LEFT JOIN Clients c ON t.client_id = c.id
                WHERE t.sold = 1";

                    var tickets = new List<Ticket>();

                    await connection.QueryAsync<dynamic>(sql, param: null).ContinueWith(t =>
                    {
                        var result = t.Result;
                        foreach (var row in result)
                        {
                            var ticket = new Ticket
                            {
                                Id = row.id,
                                SellerId = row.seller_id,
                                ClientId = row.client_id,
                                Code = row.code,
                                UnitCost = row.unit_cost,
                                Quantity = row.quantity,
                                IsPaid = row.is_paid,
                                Sold = row.sold,
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
                LEFT JOIN Selleres s ON t.seller_id = s.id
                LEFT JOIN Clients c ON t.client_id = c.id
                WHERE t.sold = 1 and ";

                    switch (searchType)
                    {
                        case SearchType.Code:
                            sql += "CAST(code AS NVARCHAR) LIKE @Busqueda";
                            break;
                        case SearchType.Seller:
                            sql += "s.name LIKE @Busqueda";
                            break;
                        case SearchType.Client:
                            sql += "c.name LIKE @Busqueda";
                            break;
                        default: // Todos
                            sql += "CAST(code AS NVARCHAR) LIKE @Busqueda OR " +
                                   "s.name LIKE @Busqueda OR " +
                                   "c.name LIKE @Busqueda";
                            break;
                    }

                    var tickets = new List<Ticket>();
                    var result = await connection.QueryAsync<dynamic>(sql, new { Busqueda = $"%{searchText}%" });

                    foreach (var row in result)
                    {
                        var ticket = new Ticket
                        {
                            Id = row.id,
                            SellerId = row.seller_id,
                            ClientId = row.client_id,
                            UnitCost = row.unit_cost,
                            Quantity = row.quantity,
                            IsPaid = row.is_paid,
                            Code = row.code,
                            Sold = row.sold,
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
                    if (!ticketDictionary.TryGetValue(ticket.Id, out var existingTicket))
                    {
                        existingTicket = ticket;
                        existingTicket.Seller = seller;
                        existingTicket.Client = client;
                        ticketDictionary.Add(existingTicket.Id, existingTicket);
                    }
                    return existingTicket;
                },
                parameters,
                splitOn: "id,id"
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
    }
}
