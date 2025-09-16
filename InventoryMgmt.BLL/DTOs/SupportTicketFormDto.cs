namespace InventoryMgmt.BLL.DTOs
{
    /// <summary>
    /// Model representing the support ticket form data submitted by users
    /// </summary>
    public class SupportTicketFormDto
    {
        /// <summary>
        /// Brief description of the issue
        /// </summary>
        public string Summary { get; set; }
        
        /// <summary>
        /// Priority level of the ticket (High, Average, Low)
        /// </summary>
        public string Priority { get; set; }
        
        /// <summary>
        /// Additional details or context about the issue
        /// </summary>
        public string AdditionalInfo { get; set; }
        
        /// <summary>
        /// The URL from which the ticket was created
        /// </summary>
        public string SourceUrl { get; set; }
        
        /// <summary>
        /// Optional inventory ID if the ticket is related to a specific inventory
        /// </summary>
        public int? InventoryId { get; set; }
    }
}