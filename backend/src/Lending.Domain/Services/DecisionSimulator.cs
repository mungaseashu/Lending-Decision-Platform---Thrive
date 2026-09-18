using System;
using System.Collections.Generic;
using Lending.Domain.Entities;
using Lending.Domain.Enums;
using Lending.Domain.ValueObjects;

namespace Lending.Domain.Services;

public class DecisionSimulator
{
    public IEnumerable<SimulationScenario> GenerateScenarios(LoanApplication app)
    {
        var scenarios = new List<SimulationScenario>();
        if (app.Decision.Status == LoanStatus.Approved) return scenarios;

        // General Limits
        if (app.LoanAmount < 100_000m)
        {
            scenarios.Add(new SimulationScenario {
                Title = "Increase Loan Amount",
                Description = "The minimum loan amount is £100,000.",
                TargetLoanAmount = 100_000m
            });
        }
        else if (app.LoanAmount > 1_500_000m)
        {
            scenarios.Add(new SimulationScenario {
                Title = "Decrease Loan Amount",
                Description = "The maximum loan amount is £1,500,000.",
                TargetLoanAmount = 1_500_000m
            });
        }

        // Large Loans (>= 1M)
        if (app.LoanAmount >= 1_000_000m && app.LoanAmount <= 1_500_000m)
        {
            if (app.CreditScore < 950)
            {
                scenarios.Add(new SimulationScenario {
                    Title = "Improve Credit Score",
                    Description = "Large loans require a minimum credit score of 950.",
                    TargetCreditScore = 950
                });
            }
            if (app.Ltv > 60m)
            {
                decimal requiredAsset = Math.Ceiling(app.LoanAmount / 0.60m);
                decimal maxLoan = Math.Floor(app.AssetValue * 0.60m);
                
                scenarios.Add(new SimulationScenario {
                    Title = "Improve LTV to <= 60%",
                    Description = "Large loans require an LTV of 60% or lower. You must either increase asset value or decrease the loan amount.",
                    TargetAssetValue = requiredAsset,
                    TargetLoanAmount = maxLoan >= 1_000_000m ? maxLoan : null // Only suggest loan decrease if it stays in large loan tier
                });
            }
        }

        // Standard Loans (< 1M)
        if (app.LoanAmount >= 100_000m && app.LoanAmount < 1_000_000m)
        {
            if (app.Ltv >= 90m)
            {
                decimal requiredAsset = Math.Ceiling(app.LoanAmount / 0.8999m); 
                decimal maxLoan = Math.Floor(app.AssetValue * 0.8999m);
                
                scenarios.Add(new SimulationScenario {
                    Title = "Reduce LTV below 90%",
                    Description = "LTV must be strictly under 90% for standard loans.",
                    TargetAssetValue = requiredAsset,
                    TargetLoanAmount = maxLoan
                });
            }
            else 
            {
                int neededScore = app.Ltv < 60m ? 750 : (app.Ltv < 80m ? 800 : 900);
                
                if (app.CreditScore < neededScore)
                {
                    scenarios.Add(new SimulationScenario {
                        Title = "Improve Credit Score",
                        Description = $"Based on your LTV of {app.Ltv:F2}%, a credit score of {neededScore} is required.",
                        TargetCreditScore = neededScore
                    });
                    
                    // Alternate fix: improve LTV to match current score
                    decimal? targetLtv = null;
                    if (app.CreditScore >= 800 && app.CreditScore < 900) targetLtv = 79.9m;
                    else if (app.CreditScore >= 750 && app.CreditScore < 800) targetLtv = 59.9m;
                    
                    if (targetLtv.HasValue)
                    {
                        decimal requiredAsset = Math.Ceiling(app.LoanAmount / (targetLtv.Value / 100m));
                        decimal maxLoan = Math.Floor(app.AssetValue * (targetLtv.Value / 100m));
                        
                        scenarios.Add(new SimulationScenario {
                            Title = $"Improve LTV to < {Math.Ceiling(targetLtv.Value)}%",
                            Description = $"With a credit score of {app.CreditScore}, you need an LTV strictly below {Math.Ceiling(targetLtv.Value)}%.",
                            TargetAssetValue = requiredAsset,
                            TargetLoanAmount = maxLoan
                        });
                    }
                }
            }
        }

        return scenarios;
    }
}
