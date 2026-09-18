using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Lending.Application.Services;

namespace Lending.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly ILoanApplicationService _service;

    public DashboardController(ILoanApplicationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _service.GetDashboardStatsAsync();
        return Ok(stats);
    }
}
