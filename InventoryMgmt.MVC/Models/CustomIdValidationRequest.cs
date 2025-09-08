using System.ComponentModel.DataAnnotations;

namespace InventoryMgmt.MVC.Models
{
    public class CustomIdValidationRequest
    {
        [Required]
        public int InventoryId { get; set; }
        
        [Required]
        public string CustomId { get; set; } = string.Empty;
        
        public int? ExcludeItemId { get; set; }
    }
}
