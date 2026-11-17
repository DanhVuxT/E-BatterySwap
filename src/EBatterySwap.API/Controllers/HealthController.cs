using Microsoft.AspNetCore.Mvc;

namespace EBatterySwap.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            service = "EBatterySwap.API"
        });
    }
}
