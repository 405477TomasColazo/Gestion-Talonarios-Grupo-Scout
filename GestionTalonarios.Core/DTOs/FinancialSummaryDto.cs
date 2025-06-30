namespace GestionTalonarios.Core.DTOs
{
    public class FinancialSummaryDto
    {
        public int TotalPortionsSold { get; set; }
        public int TotalPortionsPaid { get; set; }
        public int TotalPortionsDelivered { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProductionCost { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal GroupProfit { get; set; }
        public decimal TotalCommissions { get; set; }
        public decimal PendingPayments { get; set; }
        public DateTime ReportDate { get; set; } = DateTime.Now;
        public List<SellerCommissionDto> SellerCommissions { get; set; } = new List<SellerCommissionDto>();
    }
}