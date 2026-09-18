using Lending.Domain.Entities;
using Lending.Domain.ValueObjects;
using Lending.Domain.Constants;

namespace Lending.Domain.Services;

public class GeneralLimitsRule : ILoanRule
{
    public RuleResult Evaluate(LoanApplication application)
    {
        if (application.LoanAmount < 100_000m)
            return RuleResult.Fail(RuleNames.GeneralLimits, $"Loan amount of £{application.LoanAmount:N2} is below the minimum limit of £100,000.");
            
        if (application.LoanAmount > 1_500_000m)
            return RuleResult.Fail(RuleNames.GeneralLimits, $"Loan amount of £{application.LoanAmount:N2} exceeds the maximum limit of £1,500,000.");

        return RuleResult.Pass(RuleNames.GeneralLimits, $"Loan amount of £{application.LoanAmount:N2} is within the acceptable range (£100,000 - £1,500,000).");
    }
}
