namespace GestionTalonarios.Core.DTOs
{
    public class SellerImportDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public int RowNumber { get; set; }
    }
}