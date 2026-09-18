using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lending.Application.DTOs;
using Lending.Application.Repositories;
using Lending.Domain.Entities;
using Lending.Domain.Enums;
using Lending.Domain.Services;

namespace Lending.Application.Services;

public class LoanApplicationService : ILoanApplicationService
{
    private readonly ILoanApplicationRepository _repository;
    private readonly LoanDecisionEngine _decisionEngine;
    private readonly DecisionSimulator _simulator;

    public LoanApplicationService(ILoanApplicationRepository repository, LoanDecisionEngine decisionEngine, DecisionSimulator simulator)
    {
        _repository = repository;
        _decisionEngine = decisionEngine;
        _simulator = simulator;
    }

    public async Task<LoanApplicationResponse> SubmitApplicationAsync(CreateLoanApplicationRequest request)
    {
        var application = LoanApplication.Create(request.LoanAmount, request.AssetValue, request.CreditScore);
        
        var decision = _decisionEngine.Evaluate(application);
        application.UpdateDecision(decision);
        
        await _repository.AddAsync(application);

        return MapToResponse(application);
    }

    public async Task<IEnumerable<LoanApplicationResponse>> GetAllApplicationsAsync()
    {
        var applications = await _repository.GetAllAsync();
        return applications.Select(MapToResponse);
    }

    public async Task<LoanApplicationResponse?> GetApplicationByIdAsync(Guid id)
    {
        var application = await _repository.GetByIdAsync(id);
        if (application == null) return null;
        
        return MapToResponse(application);
    }

    public async Task<DashboardStatsResponse> GetDashboardStatsAsync()
    {
        var aggregates = await _repository.GetDashboardAggregatesAsync();

        return new DashboardStatsResponse
        {
            TotalApplicants = aggregates.Total,
            SuccessfulApplicants = aggregates.Approved,
            DeclinedApplicants = aggregates.Declined,
            TotalValueOfLoansWritten = aggregates.TotalValue,
            MeanAverageLtv = aggregates.MeanLtv
        };
    }

    public async Task<IEnumerable<SimulationScenarioDto>?> GetSimulationAsync(Guid id)
    {
        var app = await _repository.GetByIdAsync(id);
        if (app == null) return null;
        var scenarios = _simulator.GenerateScenarios(app);
        return scenarios.Select(s => new SimulationScenarioDto {
            Title = s.Title,
            Description = s.Description,
            TargetLoanAmount = s.TargetLoanAmount,
            TargetAssetValue = s.TargetAssetValue,
            TargetCreditScore = s.TargetCreditScore
        });
    }

    private static LoanApplicationResponse MapToResponse(LoanApplication app)
    {
        return new LoanApplicationResponse
        {
            Id = app.Id,
            LoanAmount = app.LoanAmount,
            AssetValue = app.AssetValue,
            CreditScore = app.CreditScore,
            Ltv = app.Ltv,
            Decision = new LoanDecisionDto
            {
                Status = app.Decision.Status.ToString(),
                CalculatedLtv = app.Decision.CalculatedLtv,
                RuleResults = app.Decision.RuleResults.Select(r => new RuleResultDto
                {
                    RuleName = r.RuleName,
                    IsPassed = r.IsPassed,
                    Reason = r.Reason
                }).ToList(),
                Reasons = app.Decision.Reasons.ToList()
            },
            CreatedAt = app.CreatedAt
        };
    }
}
