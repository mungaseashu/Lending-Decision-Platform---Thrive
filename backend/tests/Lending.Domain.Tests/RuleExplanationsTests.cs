using Lending.Domain.Constants;
using System.Linq;
using Xunit;
using Lending.Domain.Entities;
using Lending.Domain.Services;
using Lending.Domain.Enums;

namespace Lending.Domain.Tests;

public class RuleExplanationsTests
{
    private readonly LoanDecisionEngine _engine;

    public RuleExplanationsTests()
    {
        _engine = new LoanDecisionEngine(new ILoanRule[]
        {
            new GeneralLimitsRule(),
            new LargeLoanRule(),
            new SmallLoanRule()
        });
    }

    [Fact]
    public void ApprovedApplication_ReturnsSuccessfulRuleEvaluations_WithMeaningfulExplanations()
    {
        // Arrange
        // Small loan (500k), 1m asset => 50% LTV, score 800 => Approved
        var app = LoanApplication.Create(500_000, 1_000_000, 800);

        // Act
        var decision = _engine.Evaluate(app);

        // Assert
        Assert.Equal(LoanStatus.Approved, decision.Status);
        
        var generalRule = decision.RuleResults.Single(r => r.RuleName == RuleNames.GeneralLimits);
        Assert.True(generalRule.IsPassed);
        Assert.Contains("within the acceptable range", generalRule.Reason);

        var largeRule = decision.RuleResults.Single(r => r.RuleName == RuleNames.LargeLoan);
        Assert.True(largeRule.IsPassed);
        Assert.Contains("large loan requirements do not apply", largeRule.Reason);

        var smallRule = decision.RuleResults.Single(r => r.RuleName == RuleNames.StandardLoan);
        Assert.True(smallRule.IsPassed);
        Assert.Contains("falls within acceptable tier", smallRule.Reason);
    }

    [Fact]
    public void DeclinedApplication_ExplainsRelevantFailure_WithRequiredThreshold()
    {
        // Arrange
        // Loan 500k, 1m asset => 50% LTV, score 500 => Fail Standard rule
        var app = LoanApplication.Create(500_000, 1_000_000, 500);

        // Act
        var decision = _engine.Evaluate(app);

        // Assert
        Assert.Equal(LoanStatus.Declined, decision.Status);
        
        var smallRule = decision.RuleResults.Single(r => r.RuleName == RuleNames.StandardLoan);
        Assert.False(smallRule.IsPassed);
        Assert.Contains("requires score >= 750", smallRule.Reason);
        
        // Ensure explanation bubbled up
        Assert.Contains(decision.Reasons, r => r.Contains("requires score >= 750"));
    }

    [Fact]
    public void DeclinedLargeApplication_ExplainsBothLtvAndScoreFailures()
    {
        // Arrange
        // Loan 1.2M, Asset 1.5M => 80% LTV (fails <= 60%), Score 900 (fails >= 950)
        var app = LoanApplication.Create(1_200_000, 1_500_000, 900);

        // Act
        var decision = _engine.Evaluate(app);

        // Assert
        Assert.Equal(LoanStatus.Declined, decision.Status);

        var largeRule = decision.RuleResults.Single(r => r.RuleName == RuleNames.LargeLoan);
        Assert.False(largeRule.IsPassed);
        
        // Assert it explicitly states both violations and their specific thresholds
        Assert.Contains("exceeds the maximum 60% allowed", largeRule.Reason);
        Assert.Contains("does not meet the 950 minimum", largeRule.Reason);
    }
}
