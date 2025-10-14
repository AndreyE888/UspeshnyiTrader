namespace UspeshnyiTrader.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string? ExceptionMessage { get; set; }

        public string? StackTrace { get; set; }

        public bool IsDevelopment => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        public int StatusCode { get; set; }

        public string? OriginalPath { get; set; }

        public DateTime ErrorTime { get; set; } = DateTime.UtcNow;
    }
}