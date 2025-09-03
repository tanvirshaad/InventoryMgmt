using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace InventoryMgmt.MVC.Models
{
    public class ImageUploadModel
    {
        [Required(ErrorMessage = "Please select an image")]
        public IFormFile Image { get; set; } = null!;
        
        public int? InventoryId { get; set; }
        public int? ItemId { get; set; }
        public string? FieldName { get; set; }
    }
}
