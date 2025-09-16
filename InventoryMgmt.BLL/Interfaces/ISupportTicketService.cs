using InventoryMgmt.BLL.DTOs;
using System.Threading.Tasks;

namespace InventoryMgmt.BLL.Interfaces
{
    /// <summary>
    /// Interface for support ticket service operations
    /// </summary>
    public interface ISupportTicketService
    {
        /// <summary>
        /// Creates a support ticket JSON file and uploads it to cloud storage
        /// </summary>
        /// <param name="formModel">The form data submitted by the user</param>
        /// <param name="username">Username of the current user</param>
        /// <param name="inventoryTitle">Title of the relevant inventory (if applicable)</param>
        /// <returns>Result of the upload operation</returns>
        Task<SupportTicketResult> CreateAndUploadTicketAsync(SupportTicketFormDto formModel, string username, string inventoryTitle = null);
        
        /// <summary>
        /// Gets the list of admin email addresses to be notified
        /// </summary>
        /// <returns>Array of admin email addresses</returns>
        Task<string[]> GetAdminEmailsAsync();
    }
}