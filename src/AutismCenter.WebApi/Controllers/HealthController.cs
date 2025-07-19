using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutismCenter.Infrastructure.Data;

namespace AutismCenter.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HealthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            // Test database connectivity
            await _context.Database.CanConnectAsync();
            
            return Ok(new 
            { 
                Status = "Healthy", 
                Database = "Connected",
                Timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new 
            { 
                Status = "Unhealthy", 
                Database = "Disconnected",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow 
            });
        }
    }
}