using APBD25_Kolokwium.Models;
using APBD25_Kolokwium.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace APBD25_Kolokwium.Services;

public interface IDBService
{
    
    public Task<Booking> GetBooking(int id,CancellationToken cancellationToken);
    public Task<int> Post(ReservationDto reservationDto, CancellationToken cancellationToken);
    
}