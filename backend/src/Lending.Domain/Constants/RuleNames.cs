namespace Lending.Domain.Constants;

public static class RuleNames
{
    public const string GeneralLimits = "General Loan Limits Rule";
    public const string LargeLoan = "Large Loan Threshold Rule (>= £1M)";
    public const string StandardLoan = "Standard Loan Rule (< £1M)";
}
