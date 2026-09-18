using Lending.Domain.Entities;
using Lending.Domain.ValueObjects;
using Lending.Domain.Constants;

namespace Lending.Domain.Services;

public class SmallLoanRule : ILoanRule
{
    public RuleResult Evaluate(LoanApplication application)
    {
        if (application.LoanAmount >= 1_000_000m)
            return RuleResult.Pass(RuleNames.StandardLoan, "Loan is >= £1M; standard loan requirements do not apply.");

        if (application.Ltv < 60m)
        {
            if (application.CreditScore < 750)
                return RuleResult.Fail(RuleNames.StandardLoan, $"LTV {application.Ltv}% (< 60%) requires score >= 750 (Actual: {application.CreditScore}).");
        }
        else if (application.Ltv < 80m)
        {
            if (application.CreditScore < 800)
                return RuleResult.Fail(RuleNames.StandardLoan, $"LTV {application.Ltv}% (< 80%) requires score >= 800 (Actual: {application.CreditScore}).");
        }
        else if (application.Ltv < 90m)
        {
            if (application.CreditScore < 900)
                return RuleResult.Fail(RuleNames.StandardLoan, $"LTV {application.Ltv}% (< 90%) requires score >= 900 (Actual: {application.CreditScore}).");
        }
        else
        {
            return RuleResult.Fail(RuleNames.StandardLoan, $"LTV {application.Ltv}% (>= 90%) is automatically declined.");
        }

        return RuleResult.Pass(RuleNames.StandardLoan, $"LTV {application.Ltv}% falls within acceptable tier for score {application.CreditScore}.");
    }
}
