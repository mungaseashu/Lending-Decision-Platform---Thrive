using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Lending.Application.DTOs;
using Lending.Application.Services;

namespace Lending.Api.Controllers;

[ApiController]
[Route("api/loan-applications")]
public class LoanApplicationsController : ControllerBase
{
    private readonly ILoanApplicationService _service;

    public LoanApplicationsController(ILoanApplicationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitApplication([FromBody] CreateLoanApplicationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _service.SubmitApplicationAsync(request);
        return CreatedAtAction(nameof(GetApplication), new { id = response.Id }, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetApplications()
    {
        var applications = await _service.GetAllApplicationsAsync();
        return Ok(applications);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetApplication(Guid id)
    {
        var application = await _service.GetApplicationByIdAsync(id);
        
        if (application == null)
        {
            return NotFound(new ProblemDetails 
            { 
                Status = 404, 
                Title = "Not Found", 
                Detail = $"Loan application with ID {id} was not found." 
            });
        }

        return Ok(application);
    }

    [HttpGet("{id}/simulate")]
    public async Task<IActionResult> GetSimulation(Guid id)
    {
        var scenarios = await _service.GetSimulationAsync(id);
        if (scenarios == null)
        {
            return NotFound(new ProblemDetails 
            { 
                Status = 404, 
                Title = "Not Found", 
                Detail = $"Loan application with ID {id} was not found." 
            });
        }
        return Ok(scenarios);
    }
}
