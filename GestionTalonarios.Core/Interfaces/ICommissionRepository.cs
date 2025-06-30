using GestionTalonarios.Core.Models;

namespace GestionTalonarios.Core.Interfaces
{
    public interface ICommissionRepository
    {
        Task<CommissionConfig?> GetActiveConfigAsync();
        Task<CommissionConfig> SaveConfigAsync(CommissionConfig config);
        Task<List<CommissionConfig>> GetAllConfigsAsync();
    }
}