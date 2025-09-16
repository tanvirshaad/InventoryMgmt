using System;
using System.Collections.Generic;

namespace InventoryMgmt.BLL.DTOs
{
    /// <summary>
    /// DTO for support tickets that will be uploaded to cloud storage and processed by Power Automate
    /// </summary>
    public class SupportTicketDto
    {
        /// <summary>
        /// Username of the person who reported the issue
        /// </summary>
        public string ReportedBy { get; set; }
        
        /// <summary>
        /// Title of the inventory related to the ticket (if applicable)
        /// </summary>
        public string Inventory { get; set; }
        
        /// <summary>
        /// The URL from which the user created the ticket
        /// </summary>
        public string Link { get; set; }
        
        /// <summary>
        /// Brief description of the issue
        /// </summary>
        public string Summary { get; set; }
        
        /// <summary>
        /// Priority level of the ticket (High, Average, Low)
        /// </summary>
        public string Priority { get; set; }
        
        /// <summary>
        /// Timestamp when the ticket was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// List of admin email addresses to notify
        /// </summary>
        public List<string> AdminEmails { get; set; }
        
        /// <summary>
        /// Additional details or context about the issue
        /// </summary>
        public string AdditionalInfo { get; set; }
    }
}