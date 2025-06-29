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
    public interface ITicketRepository:IRepository<Ticket>
    {
        Task <IEnumerable<Ticket>> SearchTicketsAsync(string searchText,SearchType searchType);
        Task<Ticket> GetTicketByCode(int code);
        Task PayTicketAsync(int code);
        Task DeliverTicketAsync(int code);
        Task<int> GetRemainingPortionsCountAsync();
        Task<PorcionesResumenDto> GetRemainingPortionsDetailAsync();
        Task<decimal> GetDefaultUnitPriceAsync();
        Task<bool> CodeExistsAsync(int code);
        Task<PorcionesVentaDto> GetRemainingPortionsToSell();
        Task UpdateObservationsAsync(int code, string observations);
    }
}
