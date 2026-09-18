using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Lending.Domain.Enums;

namespace Lending.Domain.ValueObjects;

public record LoanDecision
{
    public LoanStatus Status { get; init; }
    public decimal CalculatedLtv { get; init; }
    public IReadOnlyList<RuleResult> RuleResults { get; init; }
    public IReadOnlyList<string> Reasons { get; init; }

    [JsonConstructor]
    public LoanDecision(LoanStatus status, decimal calculatedLtv, IReadOnlyList<RuleResult> ruleResults, IReadOnlyList<string> reasons)
    {
        Status = status;
        CalculatedLtv = calculatedLtv;
        RuleResults = ruleResults;
        Reasons = reasons;
    }

    public static LoanDecision Pending(decimal ltv) => 
        new LoanDecision(LoanStatus.Pending, ltv, new List<RuleResult>().AsReadOnly(), new List<string>().AsReadOnly());
        
    public static LoanDecision Approved(decimal ltv, IEnumerable<RuleResult> results) => 
        new LoanDecision(LoanStatus.Approved, ltv, results.ToList().AsReadOnly(), new List<string>().AsReadOnly());
        
    public static LoanDecision Declined(decimal ltv, IEnumerable<RuleResult> results, IEnumerable<string> reasons) => 
        new LoanDecision(LoanStatus.Declined, ltv, results.ToList().AsReadOnly(), reasons.ToList().AsReadOnly());
}
