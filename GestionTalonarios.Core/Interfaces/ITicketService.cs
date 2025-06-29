using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GestionTalonarios.Core.DTOs;
using GestionTalonarios.Core.Enums;
using GestionTalonarios.Core.Models;

namespace GestionTalonarios.Core.Interfaces
{
    public interface ITicketService
    {
        Task<IEnumerable<Ticket>> GetAllAsync();
        Task<Ticket> GetByCodeAsync(int code);
        Task<IEnumerable<Ticket>> GetBySearchAsync(string search,SearchType searchType);
        Task<int> AddTicketAsync(Ticket ticketDTO);
        Task UpdateTicketAsync(Ticket ticketDTO);
        Task DeleteTicketAsync(int code);
        Task PayTicketAsync(int code);
        Task DeliverTicketAsync(int code);
        Task<int> ObtenerPorcionesRestantesAsync();
        Task<PorcionesResumenDto> ObtenerDetallesPorcionesRestantesAsync();
        Task<decimal> ObtenerPrecioUnitarioDefaultAsync();
        Task<int> CrearTicketAsync(Ticket ticket);
        Task<bool> ExistTicketAsync(int code);
        Task<PorcionesVentaDto> ObtenerPorcionesEnVentaAsync();
        Task UpdateObservationsAsync(int ticketCode, string observations);
    }
}
