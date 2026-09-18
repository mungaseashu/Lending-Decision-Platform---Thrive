using System.Text.Json.Serialization;

namespace Lending.Domain.ValueObjects;

public record RuleResult
{
    public string RuleName { get; init; }
    public bool IsPassed { get; init; }
    public string Reason { get; init; }
    
    [JsonConstructor]
    public RuleResult(string ruleName, bool isPassed, string reason)
    {
        RuleName = ruleName;
        IsPassed = isPassed;
        Reason = reason;
    }

    public static RuleResult Pass(string ruleName, string reason = "") => new RuleResult(ruleName, true, reason);
    public static RuleResult Fail(string ruleName, string reason) => new RuleResult(ruleName, false, reason);
}
