using APBD25_CW9.Exceptions;
using APBD25_Kolokwium.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APBD25_Kolokwium.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpController : ControllerBase
    {
        private IDBService dbService;

        public EmpController(IDBService dbService)
        {
            this.dbService = dbService;
        }

        [HttpGet("/test/{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            try
            {
                var task = await dbService.Get(id, cancellationToken);

                return Ok(task);

            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (BadRequestException e)
            {
                return BadRequest(e.Message);
            }

        }
    }
}
