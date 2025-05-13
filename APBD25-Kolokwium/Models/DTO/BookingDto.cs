using System.ComponentModel.DataAnnotations;

namespace APBD25_Kolokwium.Models.DTO;

public class BookingDto
{
    [Required]
    public int bookingId { get; set; }
    [Required]
    public int guestId { get; set; }
    [Required]
    [MaxLength(22)]
    public string employeeNumber { get; set; }
    [Required]
    public List<AttractionsDto> attractions { get; set; }
}