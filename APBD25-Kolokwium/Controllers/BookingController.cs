using APBD25_CW9.Exceptions;
using APBD25_Kolokwium.Models;
using APBD25_Kolokwium.Models.DTO;
using APBD25_Kolokwium.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APBD25_Kolokwium.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private IDBService dbService;

        public BookingController(IDBService dbService)
        {
            this.dbService = dbService;
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            try
            {
                var booking = await dbService.GetBooking(id, cancellationToken);
                return Ok(booking);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ReservationDto reservationDto, CancellationToken cancellationToken)
        {
            try
            {
                await dbService.Post(reservationDto, cancellationToken);
                return Created();
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (BadRequestException e)
            {
                return BadRequest(e.Message);
            }
            catch (ConflictException e)
            {
                return Conflict(e.Message);
            }
        }
    }
}
