using Microsoft.AspNetCore.Mvc;

namespace CRReservation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "API działa!", timestamp = DateTime.Now });
    }
}
