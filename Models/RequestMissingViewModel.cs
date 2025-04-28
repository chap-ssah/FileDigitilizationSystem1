using System;
using System.ComponentModel.DataAnnotations;

namespace FileDigitilizationSystem.Models
{
    public class RequestMissingViewModel
    {
        [Required, StringLength(100)]
        public string Reference { get; set; }

        [Required]
        [Display(Name = "Suburb")]
        [StringLength(100)]
        public string Suburb { get; set; }

        [Required]
        [Display(Name = "Applicant Type")]
        public string ApplicantType { get; set; }

        [StringLength(100)]
        [Display(Name = "Applicant Name")]
        public string ApplicantName { get; set; }

        [StringLength(50)]
        public string Province { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
    }
}