using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Lending.Domain.Entities;
using Lending.Domain.Enums;
using Lending.Domain.Services;

namespace Lending.Domain.Tests;

public class LoanDecisionEngineTests
{
    private readonly LoanDecisionEngine _engine;

    public LoanDecisionEngineTests()
    {
        var rules = new List<ILoanRule>
        {
            new GeneralLimitsRule(),
            new LargeLoanRule(),
            new SmallLoanRule()
        };
        
        _engine = new LoanDecisionEngine(rules);
    }

    [Theory]
    // General limits bounds
    [InlineData(99_999, 200_000, 999, LoanStatus.Declined)]
    [InlineData(1_500_001, 3_000_000, 999, LoanStatus.Declined)]
    [InlineData(100_000, 200_000, 750, LoanStatus.Approved)] // Exactly 100k
    [InlineData(1_500_000, 3_000_000, 950, LoanStatus.Approved)] // Exactly 1.5M
    
    // Large loans (>= 1M)
    [InlineData(1_000_000, 2_000_000, 950, LoanStatus.Approved)] // Exactly 1M, LTV 50%, Score 950
    [InlineData(1_000_000, 1_666_667, 950, LoanStatus.Approved)] // Exactly 1M, LTV exactly 60% (1,000,000 / 1,666,666.66)
    [InlineData(1_000_000, 2_000_000, 949, LoanStatus.Declined)] // Exactly 1M, Score 949
    
    // Small loans (< 1M) - LTV < 60%
    [InlineData(500_000, 1_000_000, 750, LoanStatus.Approved)]
    [InlineData(500_000, 1_000_000, 749, LoanStatus.Declined)]
    
    // Small loans (< 1M) - 60% <= LTV < 80%
    [InlineData(600_000, 1_000_000, 800, LoanStatus.Approved)] // Exactly 60% LTV
    [InlineData(600_000, 1_000_000, 799, LoanStatus.Declined)]
    [InlineData(799_999, 1_000_000, 800, LoanStatus.Approved)]
    
    // Small loans (< 1M) - 80% <= LTV < 90%
    [InlineData(800_000, 1_000_000, 900, LoanStatus.Approved)] // Exactly 80% LTV
    [InlineData(800_000, 1_000_000, 899, LoanStatus.Declined)]
    [InlineData(899_999, 1_000_000, 900, LoanStatus.Approved)]
    
    // Small loans (< 1M) - LTV >= 90%
    [InlineData(900_000, 1_000_000, 999, LoanStatus.Declined)] // Exactly 90% LTV
    [InlineData(950_000, 1_000_000, 999, LoanStatus.Declined)]
    public void Engine_EvaluatesCorrectly(decimal loan, decimal asset, int score, LoanStatus expectedStatus)
    {
        // Arrange
        var app = LoanApplication.Create(loan, asset, score);

        // Act
        var decision = _engine.Evaluate(app);

        // Assert
        Assert.Equal(expectedStatus, decision.Status);
    }
    
    [Fact]
    public void LargeLoanRule_MultipleFailures_ReturnsBothReasons()
    {
        // Arrange
        // Loan 1M, Asset 1M (LTV 100%), Score 800 -> Fails both conditions for Large Loan
        var app = LoanApplication.Create(1_000_000, 1_000_000, 800);
        
        // Act
        var decision = _engine.Evaluate(app);

        // Assert
        Assert.Equal(LoanStatus.Declined, decision.Status);
        
        // Check rule results
        var largeRuleResult = decision.RuleResults.First(r => r.RuleName.Contains("Large Loan"));
        Assert.False(largeRuleResult.IsPassed);
        Assert.Contains("exceeds the maximum 60% allowed", largeRuleResult.Reason);
        Assert.Contains("does not meet the 950 minimum", largeRuleResult.Reason);
        Assert.Single(decision.Reasons); // One combined reason string added from this rule
    }

    [Fact]
    public void GeneralLimitsRule_TriggersFailure_SmallLoanRuleStillEvaluatesAndFails()
    {
        // Arrange
        // Loan 50k (Fails general), Asset 50k (LTV 100% - Fails small loan), Score 500
        var app = LoanApplication.Create(50_000, 50_000, 500);
        
        // Act
        var decision = _engine.Evaluate(app);

        // Assert
        Assert.Equal(LoanStatus.Declined, decision.Status);
        Assert.Equal(2, decision.Reasons.Count);
        
        Assert.Contains(decision.Reasons, r => r.Contains("below the minimum limit"));
        Assert.Contains(decision.Reasons, r => r.Contains("automatically declined"));
    }

    [Fact]
    public void CreateApplication_WithInvalidAssetValue_ThrowsException_BeforeEngineExecution()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => LoanApplication.Create(500_000, 0, 800));
        Assert.Throws<ArgumentOutOfRangeException>(() => LoanApplication.Create(500_000, -100, 800));
    }
}
