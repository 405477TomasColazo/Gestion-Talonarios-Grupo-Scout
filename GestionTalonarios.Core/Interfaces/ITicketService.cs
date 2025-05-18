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
        Task<Ticket> GetByIdAsync(int id);
        Task<IEnumerable<Ticket>> GetBySearchAsync(string search,SearchType searchType);
        Task<int> AddTicketAsync(Ticket ticketDTO);
        Task UpdateTicketAsync(Ticket ticketDTO);
        Task DeleteTicketAsync(int id);
        Task PayTicketAsync(int id);
        Task DeliverTicketAsync(int id);
        Task<int> ObtenerPorcionesRestantesAsync();
    }
}
