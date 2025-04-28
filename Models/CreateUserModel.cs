using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FileDigitilizationSystem.Models
{
    public class CreateUserModel
    {
       
        
            [Required(ErrorMessage = "First name is required")]
            [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last name is required")]
            [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            [Display(Name = "Email")]
            public string Email { get; set; }

        [Required]
        public string Role { get; set; }

        // Add this:
        public IEnumerable<SelectListItem> Roles { get; set; }

        [Required(ErrorMessage = "Password is required")]
            [DataType(DataType.Password)]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
                ErrorMessage = "Password must be at least 8 characters with 1 uppercase, 1 lowercase, and 1 number")]
            [Display(Name = "Password")]
            public string Password { get; set; }
   
}
}
