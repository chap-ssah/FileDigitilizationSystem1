using System;
using System.ComponentModel.DataAnnotations;

namespace FileDigitilizationSystem.Models
{
    public class FileRecord
    {
        public int Id { get; set; }

        [Required]
        public string Reference { get; set; }

        [Required]
        public string ApplicantName { get; set; }

        public string ApplicantId { get; set; }

        [Required]
        public string ApplicantType { get; set; }

        [Required]
        public string Province { get; set; }

        [Required]
        public string Location { get; set; }

        public string LandUseType { get; set; }
        public string SpecialStatus { get; set; }
        public string Shelf { get; set; }
        public string Row { get; set; }
        public string EmployerInfo { get; set; }

        public string Status { get; set; } = "Pending";
        public bool IsDigital { get; set; }
        public string? FilePath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }
    }
}
