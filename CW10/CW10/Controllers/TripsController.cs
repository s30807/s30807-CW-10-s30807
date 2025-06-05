using CW10.Exceptions;
using CW10.Services;
using Microsoft.AspNetCore.Mvc;

namespace CW10.Controllers;

[ApiController]
[Route("api/trips")]
public class TripsController(IDbService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllTripsDescByDateAsync([FromQuery] int page,[FromQuery] int pageSize)
    {
        return Ok(await service.GetAllTripsAsync(page, pageSize));
    }
    
    [HttpPost("{idTrip}/client")]
    public async Task<IActionResult> AddClientToTrip(int idTrip, [FromBody] PostClientToTripDTO request)
    {
        try
        {
            var result = await service.AddClientToTripAsync(idTrip, request);
            return Ok(new
            {
                message = $"Client {request.FirstName} {request.LastName} added to trip {request.TripName}.",
                data = result
            });
        }
        catch (NotFound ex)
        {
            return NotFound(ex.Message);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Unexpected error: {ex.Message}");
        }
    }
}