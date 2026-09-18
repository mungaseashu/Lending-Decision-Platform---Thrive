using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Lending.Domain.Entities;
using Lending.Domain.Enums;
using Lending.Domain.Services;

namespace Lending.Domain.Tests.Comprehensive;

public class ComprehensiveDecisionTests
{
    private readonly LoanDecisionEngine _engine;

    public ComprehensiveDecisionTests()
    {
        _engine = new LoanDecisionEngine(new List<ILoanRule>
        {
            new GeneralLimitsRule(),
            new LargeLoanRule(),
            new SmallLoanRule()
        });
    }

    [Theory]
    // 1. LOAN AMOUNT BOUNDARIES (Assuming Valid Asset 10M, Score 999)
    [InlineData(99_999, 10_000_000, 999, LoanStatus.Declined)]
    [InlineData(100_000, 10_000_000, 999, LoanStatus.Approved)]
    [InlineData(100_001, 10_000_000, 999, LoanStatus.Approved)]
    [InlineData(999_999, 10_000_000, 999, LoanStatus.Approved)] // Small loan bounds
    [InlineData(1_000_000, 10_000_000, 999, LoanStatus.Approved)] // Large loan bounds
    [InlineData(1_000_001, 10_000_000, 999, LoanStatus.Approved)]
    [InlineData(1_500_000, 10_000_000, 999, LoanStatus.Approved)]
    [InlineData(1_500_001, 10_000_000, 999, LoanStatus.Declined)]
    
    // 2. LTV & CREDIT SCORE BOUNDARIES (Small Loans: e.g. 600k Loan)
    // LTV exactly 60% is (600_000 / 1_000_000)
    
    // Below 60% LTV (e.g. 59.999% -> 599_999 / 1M) -> Requires score 750
    [InlineData(599_999, 1_000_000, 749, LoanStatus.Declined)]
    [InlineData(599_999, 1_000_000, 750, LoanStatus.Approved)]
    [InlineData(599_999, 1_000_000, 751, LoanStatus.Approved)]
    
    // Exactly 60% LTV (600_000 / 1M) -> Shifted to 60-80 bucket, Requires score 800
    [InlineData(600_000, 1_000_000, 799, LoanStatus.Declined)]
    [InlineData(600_000, 1_000_000, 800, LoanStatus.Approved)]
    [InlineData(600_000, 1_000_000, 801, LoanStatus.Approved)]
    
    // Just above 60% LTV (600_001 / 1M) -> Requires score 800
    [InlineData(600_001, 1_000_000, 799, LoanStatus.Declined)]
    [InlineData(600_001, 1_000_000, 800, LoanStatus.Approved)]
    
    // Below 80% LTV (799_999 / 1M) -> Requires score 800
    [InlineData(799_999, 1_000_000, 799, LoanStatus.Declined)]
    [InlineData(799_999, 1_000_000, 800, LoanStatus.Approved)]
    
    // Exactly 80% LTV (800_000 / 1M) -> Shifted to 80-90 bucket, Requires score 900
    [InlineData(800_000, 1_000_000, 899, LoanStatus.Declined)]
    [InlineData(800_000, 1_000_000, 900, LoanStatus.Approved)]
    [InlineData(800_000, 1_000_000, 901, LoanStatus.Approved)]
    
    // Just above 80% LTV (800_001 / 1M) -> Requires score 900
    [InlineData(800_001, 1_000_000, 899, LoanStatus.Declined)]
    [InlineData(800_001, 1_000_000, 900, LoanStatus.Approved)]

    // Below 90% LTV (899_999 / 1M) -> Requires score 900
    [InlineData(899_999, 1_000_000, 899, LoanStatus.Declined)]
    [InlineData(899_999, 1_000_000, 900, LoanStatus.Approved)]
    
    // Exactly 90% LTV (900_000 / 1M) -> Automatic Decline (>= 90%)
    [InlineData(900_000, 1_000_000, 999, LoanStatus.Declined)] // Even with max score
    
    // Just above 90% LTV (900_001 / 1M) -> Automatic Decline
    [InlineData(900_001, 1_000_000, 999, LoanStatus.Declined)]

    // 3. HIGH VALUE LOANS (>= 1M)
    // Exactly 1M Loan, Exactly 60% LTV (Asset 1_666_666.66... hard to map exactly with decimals without repeating, so we'll use 1.2M / 2M)
    // Wait, the test uses loan amounts. If loan is 1.2M, it's > 1M.
    
    // Valid LTV and valid score (1.2M / 2M = 60%, score 950)
    [InlineData(1_200_000, 2_000_000, 950, LoanStatus.Approved)]
    [InlineData(1_200_000, 2_000_000, 951, LoanStatus.Approved)]
    
    // Valid LTV, Invalid score
    [InlineData(1_200_000, 2_000_000, 949, LoanStatus.Declined)]
    
    // Invalid LTV (> 60%), Valid score
    [InlineData(1_200_001, 2_000_000, 950, LoanStatus.Declined)]
    [InlineData(1_200_001, 2_000_000, 999, LoanStatus.Declined)]
    
    // Both Invalid
    [InlineData(1_200_001, 2_000_000, 949, LoanStatus.Declined)]

    // Exactly £1m
    [InlineData(1_000_000, 2_000_000, 950, LoanStatus.Approved)] // 50% LTV, 950 score
    [InlineData(1_000_000, 2_000_000, 949, LoanStatus.Declined)] // 50% LTV, 949 score
    public void DecisionEngine_Evaluates_Correctly(decimal loan, decimal asset, int score, LoanStatus expectedStatus)
    {
        var app = LoanApplication.Create(loan, asset, score);
        var decision = _engine.Evaluate(app);
        Assert.Equal(expectedStatus, decision.Status);
    }

    // 4. VALIDATION (Throws on Create)
    [Theory]
    [InlineData(100_000, 200_000, 0)]   // score below 1
    [InlineData(100_000, 200_000, 1000)] // score above 999
    [InlineData(100_000, 0, 800)]       // zero asset value
    [InlineData(100_000, -50_000, 800)] // negative asset value
    [InlineData(0, 200_000, 800)]       // zero loan
    [InlineData(-100_000, 200_000, 800)]// negative loan amount
    public void InvalidInputs_ThrowArgumentOutOfRangeException(decimal loan, decimal asset, int score)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LoanApplication.Create(loan, asset, score));
    }

    // 5. ADDITIONAL BEHAVIOR (Precision, Multiple Failures, Reasons)
    [Fact]
    public void LtvCalculation_UsesDecimalPrecision()
    {
        // 1,000,000 / 1,666,667 = 0.59999988... * 100 = 59.999988...
        // This is strictly < 60%
        var app = LoanApplication.Create(1_000_000m, 1_666_667m, 950);
        Assert.True(app.Ltv < 60m);
        
        var decision = _engine.Evaluate(app);
        Assert.Equal(LoanStatus.Approved, decision.Status);
    }

    [Fact]
    public void Engine_MultipleFailedRules_ReturnsCombinedReasons()
    {
        // Loan = 50k (Fails General)
        // Asset = 50k -> LTV 100% (Fails Small Loan >= 90)
        // Score = 100 (Way below requirements)
        var app = LoanApplication.Create(50_000m, 50_000m, 100);
        var decision = _engine.Evaluate(app);

        Assert.Equal(LoanStatus.Declined, decision.Status);
        
        // Assert we got multiple distinct reasons
        Assert.True(decision.Reasons.Count >= 2);
        
        Assert.Contains(decision.Reasons, r => r.Contains("minimum limit"));
        Assert.Contains(decision.Reasons, r => r.Contains("automatically declined"));
    }

    [Fact]
    public void Engine_LargeLoan_BothInvalid_ReturnsCombinedReasons()
    {
        // Loan = 1.2M, Asset = 1.2M (LTV 100%), Score = 800
        var app = LoanApplication.Create(1_200_000m, 1_200_000m, 800);
        var decision = _engine.Evaluate(app);

        Assert.Equal(LoanStatus.Declined, decision.Status);
        
        // Assert Large Loan rule generated reason covering both LTV and Score
        var largeRuleResult = decision.RuleResults.First(r => r.RuleName.Contains("Large Loan"));
        Assert.False(largeRuleResult.IsPassed);
        Assert.Contains("exceeds the maximum 60% allowed", largeRuleResult.Reason);
        Assert.Contains("does not meet the 950 minimum", largeRuleResult.Reason);
    }
}
