using OfficeOpenXml;
using GestionTalonarios.Core.DTOs;
using GestionTalonarios.Core.Interfaces;
using GestionTalonarios.Core.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace GestionTalonarios.Core.Services
{
    public class ImportService : IImportService
    {
        private readonly ITicketService _ticketService;
        private readonly IClientService _clientService;
        private readonly ILogger<ImportService> _logger;

        // Configurar licencia en cada uso del ExcelPackage

        public ImportService(ITicketService ticketService, IClientService clientService, ILogger<ImportService> logger)
        {
            _ticketService = ticketService;
            _clientService = clientService;
            _logger = logger;
        }

        public async Task<ImportResult> ImportTicketsFromExcelAsync(string filePath)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new ImportResult();

            try
            {
                _logger.LogInformation($"Iniciando importación desde archivo: {filePath}");

                var ticketsToImport = await ValidateExcelDataAsync(filePath);
                result.TotalRows = ticketsToImport.Count;

                foreach (var ticketDto in ticketsToImport)
                {
                    try
                    {
                        if (!ticketDto.IsValid)
                        {
                            result.FailedImports++;
                            result.Errors.AddRange(ticketDto.ValidationErrors.Select(error => new ImportError
                            {
                                RowNumber = ticketDto.RowNumber,
                                Code = ticketDto.Nro_Ticket,
                                ErrorMessage = error
                            }));
                            continue;
                        }

                        var ticket = new Ticket
                        {
                            Code = ticketDto.Nro_Ticket,
                            SellerId = ticketDto.SellerId,
                            ClientId = ticketDto.ClientId,
                            UnitCost = ticketDto.UnitCost,
                            TraditionalQty = ticketDto.Tradicionales,
                            VeganQty = ticketDto.Veganas,
                            IsPaid = ticketDto.IsPaid,
                            IsDelivered = ticketDto.IsDelivered,
                            Sold = ticketDto.Sold,
                            Observations = ticketDto.Observations,
                            SaleDate = ticketDto.SaleDate
                        };

                        await _ticketService.CrearTicketAsync(ticket);
                        result.SuccessfulImports++;
                        
                        _logger.LogDebug($"Ticket importado exitosamente: {ticket.Code}");
                    }
                    catch (Exception ex)
                    {
                        result.FailedImports++;
                        result.Errors.Add(new ImportError
                        {
                            RowNumber = ticketDto.RowNumber,
                            Code = ticketDto.Nro_Ticket,
                            ErrorMessage = $"Error al crear ticket: {ex.Message}"
                        });
                        
                        _logger.LogError(ex, $"Error al importar ticket en fila {ticketDto.RowNumber}, código {ticketDto.Nro_Ticket}");
                    }
                }

                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;

                _logger.LogInformation($"Importación completada. {result.Summary}. Tiempo: {result.ProcessingTime.TotalSeconds:F2}s");
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;
                
                _logger.LogError(ex, $"Error crítico durante la importación desde {filePath}");
                
                result.Errors.Add(new ImportError
                {
                    RowNumber = 0,
                    ErrorMessage = $"Error crítico: {ex.Message}"
                });
                
                return result;
            }
        }

        public async Task<List<TicketImportDto>> ValidateExcelDataAsync(string filePath)
        {
            var tickets = new List<TicketImportDto>();

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"El archivo {filePath} no existe");
            }

            using var package = CreateExcelPackage(filePath);
            
            // Buscar hoja "Tickets"
            var ticketsWorksheet = package.Workbook.Worksheets["Tickets"] ?? package.Workbook.Worksheets.FirstOrDefault();
            if (ticketsWorksheet == null)
            {
                throw new InvalidOperationException("No se encontró la hoja 'Tickets' en el archivo");
            }

            // Buscar hoja "Vendedores"
            var vendedoresWorksheet = package.Workbook.Worksheets["Vendedores"];
            if (vendedoresWorksheet == null)
            {
                throw new InvalidOperationException("No se encontró la hoja 'Vendedores' en el archivo");
            }

            // Cargar vendedores desde la hoja correspondiente
            var vendedores = LoadSellersFromWorksheet(vendedoresWorksheet);

            if (ticketsWorksheet.Dimension == null)
            {
                throw new InvalidOperationException("La hoja 'Tickets' está vacía");
            }

            var rowCount = ticketsWorksheet.Dimension.End.Row;

            // Validar headers de la hoja Tickets (fila 1)
            var expectedHeaders = new[] { "Nro_Ticket", "Seccion", "Vendedor", "Cliente", "Tradicionales", "Veganas", "Esta_Pago" };
            ValidateHeaders(ticketsWorksheet, expectedHeaders);

            // Obtener precio unitario por defecto del sistema
            var defaultUnitCost = await _ticketService.ObtenerPrecioUnitarioDefaultAsync();

            // Procesar datos (desde fila 2)
            for (int row = 2; row <= rowCount; row++)
            {
                var ticket = new TicketImportDto { RowNumber = row };

                try
                {
                    // Leer valores de las celdas
                    ticket.Nro_Ticket = GetIntValue(ticketsWorksheet, row, 1);
                    ticket.Seccion = GetStringValue(ticketsWorksheet, row, 2) ?? string.Empty;
                    ticket.Vendedor = GetStringValue(ticketsWorksheet, row, 3) ?? string.Empty;
                    ticket.Cliente = GetStringValue(ticketsWorksheet, row, 4) ?? string.Empty;
                    ticket.Tradicionales = GetIntValue(ticketsWorksheet, row, 5);
                    ticket.Veganas = GetIntValue(ticketsWorksheet, row, 6);
                    ticket.Esta_Pago = GetStringValue(ticketsWorksheet, row, 7) ?? "No";
                    ticket.UnitCost = defaultUnitCost;

                    // Validar datos
                    await ValidateTicketData(ticket, vendedores);
                }
                catch (Exception ex)
                {
                    ticket.ValidationErrors.Add($"Error al procesar fila: {ex.Message}");
                }

                tickets.Add(ticket);
            }

            return tickets;
        }

        private List<SellerImportDto> LoadSellersFromWorksheet(ExcelWorksheet worksheet)
        {
            var sellers = new List<SellerImportDto>();

            if (worksheet.Dimension == null)
            {
                throw new InvalidOperationException("La hoja 'Vendedores' está vacía");
            }

            var rowCount = worksheet.Dimension.End.Row;

            // Validar headers de vendedores
            var expectedHeaders = new[] { "id", "name", "section" };
            ValidateHeaders(worksheet, expectedHeaders);

            // Cargar vendedores (desde fila 2)
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var seller = new SellerImportDto
                    {
                        RowNumber = row,
                        Id = GetIntValue(worksheet, row, 1),
                        Name = GetStringValue(worksheet, row, 2) ?? string.Empty,
                        Section = GetStringValue(worksheet, row, 3) ?? string.Empty
                    };

                    if (!string.IsNullOrEmpty(seller.Name))
                    {
                        sellers.Add(seller);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error al cargar vendedor en fila {row}: {ex.Message}");
                }
            }

            return sellers;
        }

        private void ValidateHeaders(ExcelWorksheet worksheet, string[] expectedHeaders)
        {
            for (int col = 1; col <= expectedHeaders.Length; col++)
            {
                var headerValue = worksheet.Cells[1, col].Text?.Trim();
                if (string.IsNullOrEmpty(headerValue) || !headerValue.Equals(expectedHeaders[col - 1], StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Header incorrecto en columna {col}. Se esperaba '{expectedHeaders[col - 1]}', se encontró '{headerValue}'");
                }
            }
        }

        private async Task ValidateTicketData(TicketImportDto ticket, List<SellerImportDto> vendedores)
        {
            // Validar código único
            if (ticket.Nro_Ticket <= 0)
            {
                ticket.ValidationErrors.Add("El número de ticket debe ser mayor a cero");
            }
            else if (await _ticketService.ExistTicketAsync(ticket.Nro_Ticket))
            {
                ticket.ValidationErrors.Add($"Ya existe un ticket con número {ticket.Nro_Ticket}");
            }

            // Validar vendedor
            if (string.IsNullOrWhiteSpace(ticket.Vendedor))
            {
                ticket.ValidationErrors.Add("El vendedor es requerido");
            }
            else
            {
                var vendedor = vendedores.FirstOrDefault(v => v.Name.Equals(ticket.Vendedor.Trim(), StringComparison.OrdinalIgnoreCase));
                if (vendedor == null)
                {
                    ticket.ValidationErrors.Add($"El vendedor '{ticket.Vendedor}' no existe en la lista de vendedores");
                }
                else
                {
                    ticket.SellerId = vendedor.Id;
                    
                    // Validar que la sección coincida
                    if (!string.IsNullOrWhiteSpace(ticket.Seccion) && 
                        !vendedor.Section.Equals(ticket.Seccion.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        ticket.ValidationErrors.Add($"La sección '{ticket.Seccion}' no coincide con la sección del vendedor '{vendedor.Section}'");
                    }
                }
            }

            // Validar cliente
            if (string.IsNullOrWhiteSpace(ticket.Cliente))
            {
                ticket.ValidationErrors.Add("El cliente es requerido");
            }
            else
            {
                try
                {
                    // Buscar o crear cliente
                    var cliente = await _clientService.FindOrCreateClientAsync(ticket.Cliente.Trim());
                    ticket.ClientId = cliente.Id;
                }
                catch (Exception ex)
                {
                    ticket.ValidationErrors.Add($"Error al procesar cliente '{ticket.Cliente}': {ex.Message}");
                }
            }

            // Validar cantidades
            if (ticket.Tradicionales < 0)
            {
                ticket.ValidationErrors.Add("La cantidad de porciones tradicionales no puede ser negativa");
            }

            if (ticket.Veganas < 0)
            {
                ticket.ValidationErrors.Add("La cantidad de porciones veganas no puede ser negativa");
            }

            if (ticket.Quantity <= 0)
            {
                ticket.ValidationErrors.Add("Debe tener al menos una porción (tradicional o vegana)");
            }

            // Validar precio
            if (ticket.UnitCost <= 0)
            {
                ticket.ValidationErrors.Add("El precio unitario debe ser mayor a cero");
            }

            // Validar estado de pago
            var estadoPagoValido = new[] { "si", "sí", "no" };
            if (!string.IsNullOrWhiteSpace(ticket.Esta_Pago) && 
                !estadoPagoValido.Contains(ticket.Esta_Pago.ToLower()))
            {
                ticket.ValidationErrors.Add("El estado de pago debe ser 'Si' o 'No'");
            }

            // Validar observaciones
            if (!string.IsNullOrEmpty(ticket.Observations) && ticket.Observations.Length > 1000)
            {
                ticket.ValidationErrors.Add("Las observaciones no pueden exceder los 1000 caracteres");
            }
        }

        private int GetIntValue(ExcelWorksheet worksheet, int row, int col)
        {
            var cellValue = worksheet.Cells[row, col].Text?.Trim();
            
            if (string.IsNullOrEmpty(cellValue))
                return 0;
            
            if (int.TryParse(cellValue, out int result))
                return result;
                
            throw new FormatException($"Valor inválido en fila {row}, columna {col}: '{cellValue}' no es un número entero válido");
        }

        private decimal GetDecimalValue(ExcelWorksheet worksheet, int row, int col)
        {
            var cellValue = worksheet.Cells[row, col].Text?.Trim();
            
            if (string.IsNullOrEmpty(cellValue))
                return 0m;
            
            if (decimal.TryParse(cellValue, out decimal result))
                return result;
                
            throw new FormatException($"Valor inválido en fila {row}, columna {col}: '{cellValue}' no es un número decimal válido");
        }

        private bool GetBooleanValue(ExcelWorksheet worksheet, int row, int col)
        {
            var cellValue = worksheet.Cells[row, col].Text?.Trim().ToLower();
            
            if (string.IsNullOrEmpty(cellValue))
                return false;
            
            return cellValue == "true" || cellValue == "1" || cellValue == "sí" || cellValue == "si" || cellValue == "yes";
        }

        private string? GetStringValue(ExcelWorksheet worksheet, int row, int col)
        {
            return worksheet.Cells[row, col].Text?.Trim();
        }

        private ExcelPackage CreateExcelPackage(string filePath)
        {
            // La licencia de EPPlus 8.0 ya está configurada al inicio de la aplicación
            return new ExcelPackage(new FileInfo(filePath));
        }
    }
}