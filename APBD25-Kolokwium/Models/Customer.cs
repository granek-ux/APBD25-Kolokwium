using System.ComponentModel.DataAnnotations;

namespace APBD25_Kolokwium.Models;

public class Customer
{
    [Required]
    public string FirstName { get; set; }
    [Required]
    [MaxLength(20)]
    public string LastName { get; set; }
}