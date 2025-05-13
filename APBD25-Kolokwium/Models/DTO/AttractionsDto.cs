using Microsoft.Build.Framework;

namespace APBD25_Kolokwium.Models.DTO;

public class AttractionsDto
{
    [Required]
    public string name { get; set; }
    public int amount { get; set; }
}