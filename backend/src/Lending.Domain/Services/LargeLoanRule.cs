using Lending.Domain.Entities;
using Lending.Domain.ValueObjects;
using Lending.Domain.Constants;

namespace Lending.Domain.Services;

public class LargeLoanRule : ILoanRule
{
    public RuleResult Evaluate(LoanApplication application)
    {
        if (application.LoanAmount < 1_000_000m)
            return RuleResult.Pass(RuleNames.LargeLoan, "Loan is under £1M; large loan requirements do not apply.");

        var failReasons = new System.Collections.Generic.List<string>();

        if (application.Ltv > 60m)
            failReasons.Add($"LTV of {application.Ltv}% exceeds the maximum 60% allowed for loans >= £1M.");
            
        if (application.CreditScore < 950)
            failReasons.Add($"Credit score of {application.CreditScore} does not meet the 950 minimum required for loans >= £1M.");

        if (failReasons.Count > 0)
            return RuleResult.Fail(RuleNames.LargeLoan, string.Join(" ", failReasons));

        return RuleResult.Pass(RuleNames.LargeLoan, $"Large loan conditions met: LTV ({application.Ltv}%) <= 60% and Score ({application.CreditScore}) >= 950.");
    }
}
