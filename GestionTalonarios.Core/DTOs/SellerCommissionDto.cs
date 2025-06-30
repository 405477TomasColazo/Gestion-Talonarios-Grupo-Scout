namespace GestionTalonarios.Core.DTOs
{
    public class SellerCommissionDto
    {
        public int SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public string SellerSection { get; set; } = string.Empty;
        public int TotalPortionsSold { get; set; }
        public int Tier1Portions { get; set; } // Porciones 1-5
        public int Tier2Portions { get; set; } // Porciones 6-10
        public int Tier3Portions { get; set; } // Porciones 11+
        public decimal Tier1Commission { get; set; } // Comisión tier 1 (0% para vendedor)
        public decimal Tier2Commission { get; set; } // Comisión tier 2 (50% para vendedor)
        public decimal Tier3Commission { get; set; } // Comisión tier 3 (100% para vendedor)
        public decimal TotalCommission { get; set; }
        public decimal TotalSales { get; set; }
        public decimal GroupProfit { get; set; } // Ganancia que va al grupo
    }
}