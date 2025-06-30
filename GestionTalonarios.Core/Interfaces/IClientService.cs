using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GestionTalonarios.Core.Models;

namespace GestionTalonarios.Core.Interfaces
{
    public interface IClientService
    {
        Task<Client?> BuscarClientePorNombreAsync(string nombre);
        Task<int> CrearClienteAsync(Client cliente);
        Task<Client> FindOrCreateClientAsync(string nombre);
    }
}
