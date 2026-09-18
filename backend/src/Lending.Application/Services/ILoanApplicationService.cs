using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lending.Application.DTOs;

namespace Lending.Application.Services;

public interface ILoanApplicationService
{
    Task<LoanApplicationResponse> SubmitApplicationAsync(CreateLoanApplicationRequest request);
    Task<IEnumerable<LoanApplicationResponse>> GetAllApplicationsAsync();
    Task<LoanApplicationResponse?> GetApplicationByIdAsync(Guid id);
    Task<DashboardStatsResponse> GetDashboardStatsAsync();
    Task<IEnumerable<SimulationScenarioDto>?> GetSimulationAsync(Guid id);
}
