using Microsoft.AspNetCore.Identity;

namespace FileDigitilizationSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Optional: Add additional properties for admin and other users
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; } = true; // Default to active
        public DateTime? DeactivationDate { get; set; }

        public DateTime? LastLogin { get; set; }
    }
}
