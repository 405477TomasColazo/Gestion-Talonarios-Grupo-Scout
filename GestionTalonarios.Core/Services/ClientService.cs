using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GestionTalonarios.Core.Interfaces;
using GestionTalonarios.Core.Models;
using Microsoft.Extensions.Logging;

namespace GestionTalonarios.Core.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;

        public ClientService(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public async Task<Client?> BuscarClientePorNombreAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return null;

            return await _clientRepository.GetByNameAsync(nombre);
        }

        public async Task<int> CrearClienteAsync(Client cliente)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));

            if (string.IsNullOrWhiteSpace(cliente.Name))
                throw new ArgumentException("El nombre del cliente es requerido");

            return await _clientRepository.AddAync(cliente);
        }

        public async Task<Client> FindOrCreateClientAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del cliente es requerido", nameof(nombre));

            // Buscar cliente existente
            var clienteExistente = await BuscarClientePorNombreAsync(nombre);
            if (clienteExistente != null)
                return clienteExistente;

            // Crear nuevo cliente
            var nuevoCliente = new Client
            {
                Name = nombre.Trim(),
                Phone = string.Empty // Teléfono vacío por defecto
            };

            var clienteId = await CrearClienteAsync(nuevoCliente);
            nuevoCliente.Id = clienteId;

            return nuevoCliente;
        }
    }
}
