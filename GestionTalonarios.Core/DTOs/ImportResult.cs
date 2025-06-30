namespace GestionTalonarios.Core.DTOs
{
    public class ImportResult
    {
        public int TotalRows { get; set; }
        public int SuccessfulImports { get; set; }
        public int FailedImports { get; set; }
        public List<ImportError> Errors { get; set; } = new List<ImportError>();
        public TimeSpan ProcessingTime { get; set; }
        public bool IsSuccess => FailedImports == 0;
        public string Summary => $"Procesadas: {TotalRows}, Exitosas: {SuccessfulImports}, Fallidas: {FailedImports}";
    }

    public class ImportError
    {
        public int RowNumber { get; set; }
        public int? Code { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string? FieldName { get; set; }
    }
}