namespace GestionTalonarios.Core.DTOs
{
    public class TicketImportDto
    {
        public int Nro_Ticket { get; set; }
        public string Seccion { get; set; } = string.Empty;
        public string Vendedor { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public int Tradicionales { get; set; }
        public int Veganas { get; set; }
        public string Esta_Pago { get; set; } = "No";
        
        // Propiedades calculadas/derivadas
        public bool IsPaid => Esta_Pago?.ToLower() == "si" || Esta_Pago?.ToLower() == "sí";
        public bool IsDelivered { get; set; } = false;
        public bool Sold { get; set; } = true;
        public string? Observations { get; set; }
        public DateTime SaleDate { get; set; } = DateTime.Now;
        
        // Propiedades para el procesamiento
        public int RowNumber { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();
        public int SellerId { get; set; } // Se resuelve durante la validación
        public int ClientId { get; set; } // Se resuelve durante la validación
        public decimal UnitCost { get; set; } // Se obtiene del sistema
        
        public bool IsValid => !ValidationErrors.Any();
        public int Quantity => Tradicionales + Veganas;
    }
}