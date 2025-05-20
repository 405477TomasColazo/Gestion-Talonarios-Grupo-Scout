using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GestionTalonarios.Core.Models;

namespace GestionTalonarios.Core.Interfaces
{
    public interface IClientRepository:IRepository<Client>
    {
        Task<Client> GetByNameAsync(string name);
    }
}
