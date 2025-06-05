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
}