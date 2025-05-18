using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionTalonarios.Core.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public int SellerId { get; set; }
        public int ClientId { get; set; }
        public decimal UnitCost { get; set; }
        public int Quantity { get; set; }

        // Estos son bool, no nullable - está bien porque la DB siempre tiene 0 o 1
        public bool IsPaid { get; set; }
        public bool IsDelivered { get; set; }

        public bool Sold {  get; set; }

        // Estos son nullable - coinciden con la posibilidad de NULL en la DB
        public string? Observations { get; set; }
        public DateTime? WithdrawalTime { get; set; }
        public DateTime SaleDate { get; set; }
        public DateTime? PaymentDate { get; set; }

        // Propiedades de navegación
        public Seller? Seller { get; set; }
        public Client? Client { get; set; }
    }
}
