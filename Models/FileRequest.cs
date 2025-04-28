using System;
using System.ComponentModel.DataAnnotations;
using FileDigitilizationSystem.Models;

public class FileRequest
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Reference { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Suburb")]
    public string Suburb { get; set; }            // was Location

    [Required]
    [Display(Name = "Applicant Type")]
    public string ApplicantType { get; set; }    // new dropdown field

    [StringLength(100)]
    public string ApplicantName { get; set; }

    [StringLength(50)]
    public string Province { get; set; }

    [StringLength(500)]
    public string Notes { get; set; }

    [Required]
    public string RequesterId { get; set; }
    public ApplicationUser Requester { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool Handled { get; set; } = false;
}
