using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryMgmt.BLL.DTOs
{
    public class SalesforceAccountDto
    {
        public int UserId { get; set; }
        
        [Required(ErrorMessage = "Company name is required")]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }
        
        [Required(ErrorMessage = "Industry is required")]
        public string Industry { get; set; }
        
        [Display(Name = "Annual Revenue")]
        [Range(0, double.MaxValue, ErrorMessage = "Annual revenue must be a positive number")]
        public decimal? AnnualRevenue { get; set; }
        
        [Display(Name = "Company Phone")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string PhoneNumber { get; set; }
        
        [Display(Name = "Website")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string Website { get; set; }
        
        [Display(Name = "Description")]
        public string Description { get; set; }
        
        // Contact fields
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }
        
        [Display(Name = "Mobile Phone")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string MobilePhone { get; set; }
    }
}
