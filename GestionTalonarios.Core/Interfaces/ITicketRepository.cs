using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GestionTalonarios.Core.Enums;
using GestionTalonarios.Core.Models;

namespace GestionTalonarios.Core.Interfaces
{
    public interface ITicketRepository:IRepository<Ticket>
    {
        Task <IEnumerable<Ticket>> SearchTicketsAsync(string searchText,SearchType searchType);
        Task<Ticket> GetTicketByCode(int code);
        Task PayTicketAsync(int id);
        Task DeliverTicketAsync(int id);
        Task<int> GetRemainingPortionsCountAsync();
    }
}
