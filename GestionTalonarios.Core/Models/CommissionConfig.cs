using System.ComponentModel.DataAnnotations.Schema;

namespace GestionTalonarios.Core.Models
{
    public class CommissionConfig
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("production_cost")]
        public decimal ProductionCost { get; set; }

        [Column("sale_price")]
        public decimal SalePrice { get; set; }

        [Column("tier1_limit")]
        public int Tier1Limit { get; set; } = 5; // Primeras 5 porciones: 100% grupo

        [Column("tier2_limit")]
        public int Tier2Limit { get; set; } = 10; // Porciones 6-10: 50% grupo, 50% vendedor

        [Column("tier1_group_percentage")]
        public decimal Tier1GroupPercentage { get; set; } = 1.0m; // 100%

        [Column("tier2_group_percentage")]
        public decimal Tier2GroupPercentage { get; set; } = 0.5m; // 50%

        [Column("tier3_group_percentage")]
        public decimal Tier3GroupPercentage { get; set; } = 0.0m; // 0% (100% vendedor)

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public decimal ProfitPerPortion => SalePrice - ProductionCost;
    }
}