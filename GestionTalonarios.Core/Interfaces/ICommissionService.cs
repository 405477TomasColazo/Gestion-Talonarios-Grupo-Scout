using GestionTalonarios.Core.DTOs;
using GestionTalonarios.Core.Models;

namespace GestionTalonarios.Core.Interfaces
{
    public interface ICommissionService
    {
        Task<FinancialSummaryDto> GetFinancialSummaryAsync();
        Task<List<SellerCommissionDto>> GetSellerCommissionsAsync();
        Task<CommissionConfig> GetCommissionConfigAsync();
        Task SaveCommissionConfigAsync(CommissionConfig config);
        SellerCommissionDto CalculateSellerCommission(int sellerId, string sellerName, string sellerSection, int portionsSold, CommissionConfig config);
    }
}