using Lending.Domain.Entities;
using Lending.Domain.ValueObjects;

namespace Lending.Domain.Services;

public interface ILoanRule
{
    RuleResult Evaluate(LoanApplication application);
}
