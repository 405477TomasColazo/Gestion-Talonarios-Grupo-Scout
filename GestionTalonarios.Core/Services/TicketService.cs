using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using GestionTalonarios.Core.DTOs;
using GestionTalonarios.Core.Enums;
using GestionTalonarios.Core.Interfaces;
using GestionTalonarios.Core.Models;
using Microsoft.Extensions.Logging;

namespace GestionTalonarios.Core.Services
{
    public class TicketService : ITicketService
    {

        private readonly ITicketRepository _ticketRepository;
        private readonly ILogger<TicketService> _logger;

        public TicketService(ITicketRepository ticketRepository, ILogger<TicketService> logger)
        {
            _ticketRepository = ticketRepository;
            _logger = logger;
        }
        public Task<int> AddTicketAsync(TicketDTO ticketDTO)
        {
            throw new NotImplementedException();
        }

        public Task<int> AddTicketAsync(Ticket ticketDTO)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteTicketAsync(int id)
        {
            await _ticketRepository.DeleteAsync(id);
        }

        public async Task DeliverTicketAsync(int id)
        {
            try
            {
                await _ticketRepository.DeliverTicketAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al entregar el ticket de id: {id}");
                throw;
            }
        }

        public async Task<IEnumerable<Ticket>> GetAllAsync()
        {
            try
            {
                return await _ticketRepository.GetAllAsync();
           
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error al obtener todos los talonarios");
                throw;
            }
        }

        public async Task<Ticket> GetByIdAsync(int id)
        {
            try
            {
                return await _ticketRepository.GetByIdAsync(id);    }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener el ticket de id {id}");
                throw;
            }
        }

        public async Task<IEnumerable<Ticket>> GetBySearchAsync(string search, SearchType searchType)
        {
            try
            {
                return await _ticketRepository.SearchTicketsAsync(search, searchType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al entregar los tickets siguiendo este parametro de busqueda: {search}");
                throw;
            }
        }

        public async Task<PorcionesResumenDto> ObtenerDetallesPorcionesRestantesAsync()
        {
            try
            {
                return await _ticketRepository.GetRemainingPortionsDetailAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles de porciones restantes");
                throw;
            }
        }

        public async Task<int> ObtenerPorcionesRestantesAsync()
        {
            try
            {
                return await _ticketRepository.GetRemainingPortionsCountAsync();
            } catch (Exception ex) {
                _logger.LogError(ex, "Error al calcular las porciones restantes");
                throw;
            }
        }

        public async Task PayTicketAsync(int id)
        {
            try
            {
                await _ticketRepository.PayTicketAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al pagar el ticket de id: {id}");
                throw;
            }
        }

        public Task UpdateTicketAsync(TicketDTO ticketDTO)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTicketAsync(Ticket ticketDTO)
        {
            throw new NotImplementedException();
        }

        public async Task<decimal> ObtenerPrecioUnitarioDefaultAsync()
        {
            try
            {
                // Puedes implementar esto de varias formas:

                // 1. Consultar una tabla de configuración
                // return await _configRepository.GetDecimalValueAsync("PrecioUnitarioLocro");

                // 2. Consultar directamente un valor predeterminado
                return await _ticketRepository.GetDefaultUnitPriceAsync();

                // 3. Como alternativa rápida, puedes codificar el valor
                // return 350.0m;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener precio unitario predeterminado");
                throw;
            }
        }
        public async Task<int> CrearTicketAsync(Ticket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException(nameof(ticket));

            if (ticket.SellerId <= 0)
                throw new ArgumentException("El vendedor es requerido");

            if (ticket.ClientId <= 0)
                throw new ArgumentException("El cliente es requerido");

            if (ticket.TraditionalQty + ticket.VeganQty <= 0)
                throw new ArgumentException("Debe tener al menos una porción");

            if (ticket.UnitCost <= 0)
                throw new ArgumentException("El precio unitario debe ser mayor a cero");

            return await _ticketRepository.AddAync(ticket);
        }

        public async Task<bool> ExistTicketAsync(int code)
        {
            if (code <= 0)
                return false;

            return await _ticketRepository.CodeExistsAsync(code);
        }

        public async Task<PorcionesVentaDto> ObtenerPorcionesEnVentaAsync()
        {
            return await _ticketRepository.GetRemainingPortionsToSell();
        }
    }
}
