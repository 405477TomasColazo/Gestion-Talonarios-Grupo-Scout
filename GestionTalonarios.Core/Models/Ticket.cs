using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;

namespace GestionTalonarios.Core.Models
{
    public class Ticket
    {
        [Key]
        [Column("code")]
        public int Code { get; set; }

        [Column("seller_id")]
        public int SellerId { get; set; }

        [Column("client_id")]
        public int ClientId { get; set; }

        [Column("unit_cost")]
        public decimal UnitCost { get; set; }

        [Column("traditional_qty")]
        public int TraditionalQty { get; set; }

        [Column("vegan_qty")]
        public int VeganQty { get; set; }

        [Computed] // Esta propiedad es calculada, no existe en la BD
        public int Quantity => TraditionalQty + VeganQty;

        [Column("is_paid")]
        public bool IsPaid { get; set; }

        [Column("is_delivered")]
        public bool IsDelivered { get; set; }

        [Column("sold")]
        public bool Sold { get; set; }

        [Column("observations")]
        public string? Observations { get; set; }

        [Column("withdrawal_time")]
        public DateTime? WithdrawalTime { get; set; }

        [Column("sale_date")]
        public DateTime SaleDate { get; set; }

        [Column("payment_date")]
        public DateTime? PaymentDate { get; set; }

        [Write(false)]
        public Seller? Seller { get; set; }

        [Write(false)]
        public Client? Client { get; set; }
    }
}

