namespace APBD25_Kolokwium.Models;

public class Booking
{
    public DateTime date { get; set; } 
    public Guest guest { get; set; }
    public Employee employee { get; set; }
    public List<Attraction> attractions { get; set; }
    
}