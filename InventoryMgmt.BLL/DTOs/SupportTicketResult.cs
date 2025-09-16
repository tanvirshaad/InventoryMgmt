namespace InventoryMgmt.BLL.DTOs
{
    /// <summary>
    /// Simple result class for support ticket operations
    /// </summary>
    public class SupportTicketResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorDetails { get; set; }

        public static SupportTicketResult SuccessResult(string message = "Operation completed successfully")
        {
            return new SupportTicketResult
            {
                Success = true,
                Message = message
            };
        }

        public static SupportTicketResult ErrorResult(string message, string errorDetails = null)
        {
            return new SupportTicketResult
            {
                Success = false,
                Message = message,
                ErrorDetails = errorDetails
            };
        }
    }
}