using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lending.Application.DTOs;

public class CreateLoanApplicationRequest
{
    [Required]
    [Range(0.01, 1000000000000, ErrorMessage = "Loan amount must be greater than zero and realistic (<= 1 Trillion).")]
    public decimal LoanAmount { get; set; }

    [Required]
    [Range(0.01, 1000000000000, ErrorMessage = "Asset value must be greater than zero and realistic (<= 1 Trillion).")]
    public decimal AssetValue { get; set; }

    [Required]
    [Range(1, 999, ErrorMessage = "Credit score must be between 1 and 999.")]
    public int CreditScore { get; set; }
}

public class LoanApplicationResponse
{
    public Guid Id { get; set; }
    public decimal LoanAmount { get; set; }
    public decimal AssetValue { get; set; }
    public int CreditScore { get; set; }
    public decimal Ltv { get; set; }
    public LoanDecisionDto Decision { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class LoanDecisionDto
{
    public string Status { get; set; } = string.Empty;
    public decimal CalculatedLtv { get; set; }
    public List<RuleResultDto> RuleResults { get; set; } = new();
    public List<string> Reasons { get; set; } = new();
}

public class RuleResultDto
{
    public string RuleName { get; set; } = string.Empty;
    public bool IsPassed { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class DashboardStatsResponse
{
    public int TotalApplicants { get; set; }
    public int SuccessfulApplicants { get; set; }
    public int DeclinedApplicants { get; set; }
    public decimal TotalValueOfLoansWritten { get; set; }
    public decimal MeanAverageLtv { get; set; }
}

public class SimulationScenarioDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? TargetLoanAmount { get; set; }
    public decimal? TargetAssetValue { get; set; }
    public int? TargetCreditScore { get; set; }
}
