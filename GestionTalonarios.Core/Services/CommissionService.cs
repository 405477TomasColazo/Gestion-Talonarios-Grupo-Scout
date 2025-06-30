using GestionTalonarios.Core.DTOs;
using GestionTalonarios.Core.Interfaces;
using GestionTalonarios.Core.Models;
using Microsoft.Extensions.Logging;

namespace GestionTalonarios.Core.Services
{
    public class CommissionService : ICommissionService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ICommissionRepository _commissionRepository;
        private readonly ILogger<CommissionService> _logger;

        public CommissionService(
            ITicketRepository ticketRepository,
            ICommissionRepository commissionRepository,
            ILogger<CommissionService> logger)
        {
            _ticketRepository = ticketRepository;
            _commissionRepository = commissionRepository;
            _logger = logger;
        }

        public async Task<FinancialSummaryDto> GetFinancialSummaryAsync()
        {
            try
            {
                var tickets = await _ticketRepository.GetAllAsync();
                var config = await GetCommissionConfigAsync();
                
                _logger.LogInformation($"Config obtenida en FinancialSummary: ProductionCost={config.ProductionCost}, SalePrice={config.SalePrice}");
                
                var sellerCommissions = await GetSellerCommissionsAsync();

                var soldTickets = tickets.Where(t => t.Sold).ToList();
                var paidTickets = soldTickets.Where(t => t.IsPaid).ToList();
                var deliveredTickets = soldTickets.Where(t => t.IsDelivered).ToList();

                var totalPortionsSold = soldTickets.Sum(t => t.Quantity);
                var totalRevenue = paidTickets.Sum(t => t.UnitCost * t.Quantity);
                var totalProductionCost = totalPortionsSold * config.ProductionCost;
                var totalProfit = totalRevenue - totalProductionCost;
                var totalCommissions = sellerCommissions.Sum(s => s.TotalCommission);
                var groupProfit = totalProfit - totalCommissions;
                var pendingPayments = soldTickets.Where(t => !t.IsPaid).Sum(t => t.UnitCost * t.Quantity);
                
                _logger.LogInformation($"Cálculos: TotalPortions={totalPortionsSold}, ProductionCost={config.ProductionCost}, TotalProductionCost={totalProductionCost}");

                return new FinancialSummaryDto
                {
                    TotalPortionsSold = totalPortionsSold,
                    TotalPortionsPaid = paidTickets.Sum(t => t.Quantity),
                    TotalPortionsDelivered = deliveredTickets.Sum(t => t.Quantity),
                    TotalRevenue = totalRevenue,
                    TotalProductionCost = totalProductionCost,
                    TotalProfit = totalProfit,
                    GroupProfit = groupProfit,
                    TotalCommissions = totalCommissions,
                    PendingPayments = pendingPayments,
                    SellerCommissions = sellerCommissions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar resumen financiero");
                throw;
            }
        }

        public async Task<List<SellerCommissionDto>> GetSellerCommissionsAsync()
        {
            try
            {
                var tickets = await _ticketRepository.GetAllAsync();
                var config = await GetCommissionConfigAsync();

                var sellerGroups = tickets
                    .Where(t => t.Sold)
                    .GroupBy(t => new { t.SellerId, t.Seller?.Name, t.Seller?.Section })
                    .ToList();

                var commissions = new List<SellerCommissionDto>();

                foreach (var group in sellerGroups)
                {
                    var sellerId = group.Key.SellerId;
                    var sellerName = group.Key.Name ?? "Desconocido";
                    var sellerSection = group.Key.Section ?? "";
                    var totalPortions = group.Sum(t => t.Quantity);

                    var commission = CalculateSellerCommission(sellerId, sellerName, sellerSection, totalPortions, config);
                    commission.TotalSales = group.Where(t => t.IsPaid).Sum(t => t.UnitCost * t.Quantity);
                    
                    commissions.Add(commission);
                }

                return commissions.OrderByDescending(c => c.TotalPortionsSold).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular comisiones de vendedores");
                throw;
            }
        }

        public SellerCommissionDto CalculateSellerCommission(int sellerId, string sellerName, string sellerSection, int portionsSold, CommissionConfig config)
        {
            var commission = new SellerCommissionDto
            {
                SellerId = sellerId,
                SellerName = sellerName,
                SellerSection = sellerSection,
                TotalPortionsSold = portionsSold
            };

            var profitPerPortion = config.ProfitPerPortion;

            // Calcular porciones por tier
            if (portionsSold <= config.Tier1Limit)
            {
                commission.Tier1Portions = portionsSold;
                commission.Tier2Portions = 0;
                commission.Tier3Portions = 0;
            }
            else if (portionsSold <= config.Tier2Limit)
            {
                commission.Tier1Portions = config.Tier1Limit;
                commission.Tier2Portions = portionsSold - config.Tier1Limit;
                commission.Tier3Portions = 0;
            }
            else
            {
                commission.Tier1Portions = config.Tier1Limit;
                commission.Tier2Portions = config.Tier2Limit - config.Tier1Limit;
                commission.Tier3Portions = portionsSold - config.Tier2Limit;
            }

            // Calcular comisiones
            commission.Tier1Commission = commission.Tier1Portions * profitPerPortion * (1 - config.Tier1GroupPercentage);
            commission.Tier2Commission = commission.Tier2Portions * profitPerPortion * (1 - config.Tier2GroupPercentage);
            commission.Tier3Commission = commission.Tier3Portions * profitPerPortion * (1 - config.Tier3GroupPercentage);

            commission.TotalCommission = commission.Tier1Commission + commission.Tier2Commission + commission.Tier3Commission;

            // Calcular ganancia que va al grupo
            var tier1GroupProfit = commission.Tier1Portions * profitPerPortion * config.Tier1GroupPercentage;
            var tier2GroupProfit = commission.Tier2Portions * profitPerPortion * config.Tier2GroupPercentage;
            var tier3GroupProfit = commission.Tier3Portions * profitPerPortion * config.Tier3GroupPercentage;
            
            commission.GroupProfit = tier1GroupProfit + tier2GroupProfit + tier3GroupProfit;

            return commission;
        }

        public async Task<CommissionConfig> GetCommissionConfigAsync()
        {
            try
            {
                var config = await _commissionRepository.GetActiveConfigAsync();
                
                if (config == null)
                {
                    // Crear configuración por defecto
                    config = new CommissionConfig
                    {
                        ProductionCost = 200m, // Valor por defecto
                        SalePrice = 350m,      // Valor por defecto
                        Tier1Limit = 5,
                        Tier2Limit = 10,
                        Tier1GroupPercentage = 1.0m,
                        Tier2GroupPercentage = 0.5m,
                        Tier3GroupPercentage = 0.0m,
                        IsActive = true
                    };
                    
                    await _commissionRepository.SaveConfigAsync(config);
                }

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuración de comisiones");
                throw;
            }
        }

        public async Task SaveCommissionConfigAsync(CommissionConfig config)
        {
            try
            {
                await _commissionRepository.SaveConfigAsync(config);
                _logger.LogInformation($"Configuración de comisiones guardada: Costo={config.ProductionCost}, Precio={config.SalePrice}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar configuración de comisiones");
                throw;
            }
        }
    }
}