using EBatterySwap.Infrastructure.Persistence;
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

    /// <summary>Checks that the API can open a connection to SQL Server.</summary>
    [HttpGet("db")]
    public async Task<IActionResult> GetDatabase(
        ApplicationDbContext dbContext,
        ILogger<HealthController> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            if (await dbContext.Database.CanConnectAsync(cancellationToken))
            {
                return Ok(new
                {
                    status = "healthy",
                    database = "connected"
                });
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Database health check failed.");
        }

        return Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "Database unavailable",
            detail: "The API could not connect to the database.");
    }
}
