using System.Collections.Generic;
using System.Linq;
using Lending.Domain.Entities;
using Lending.Domain.ValueObjects;

namespace Lending.Domain.Services;

public class LoanDecisionEngine
{
    private readonly IEnumerable<ILoanRule> _rules;

    public LoanDecisionEngine(IEnumerable<ILoanRule> rules)
    {
        _rules = rules;
    }

    public LoanDecision Evaluate(LoanApplication application)
    {
        var ruleResults = new List<RuleResult>();
        var failureReasons = new List<string>();

        foreach (var rule in _rules)
        {
            var result = rule.Evaluate(application);
            ruleResults.Add(result);

            if (!result.IsPassed)
            {
                failureReasons.Add(result.Reason);
            }
        }

        if (failureReasons.Any())
        {
            return LoanDecision.Declined(application.Ltv, ruleResults, failureReasons);
        }

        return LoanDecision.Approved(application.Ltv, ruleResults);
    }
}
