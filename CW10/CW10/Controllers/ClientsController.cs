using CW10.Data;
using CW10.Exceptions;
using CW10.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CW10.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController(IDbService service) : ControllerBase
{
    [HttpDelete]
    [Route("/{idClient}")]
    public async Task<IActionResult> DeleteClient([FromRoute] string idClient)
    {
        try
        {
            await service.DeleteClientAsync(idClient);
            return Ok(new
                { message = $"Client {idClient} succesfully deleted" }); //nie wiem jaki powinien byc kod bledu
        }
        catch (NotFound ex)
        {
            return NotFound(ex.Message);
        }
        catch (ClientWithTripsException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}