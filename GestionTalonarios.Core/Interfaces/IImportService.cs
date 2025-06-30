using GestionTalonarios.Core.DTOs;

namespace GestionTalonarios.Core.Interfaces
{
    public interface IImportService
    {
        Task<ImportResult> ImportTicketsFromExcelAsync(string filePath);
        Task<List<TicketImportDto>> ValidateExcelDataAsync(string filePath);
    }
}