using System;
using System.Collections.Generic;
using Xunit;
using Lending.Domain.Entities;
using Lending.Domain.Enums;
using Lending.Domain.ValueObjects;

namespace Lending.Domain.Tests;

public class LoanApplicationTests
{
    [Fact]
    public void Create_ValidInputs_CreatesApplicationWithCorrectLtvAndPendingStatus()
    {
        // Arrange
        decimal loanAmount = 500_000m;
        decimal assetValue = 1_000_000m;
        int creditScore = 800;

        // Act
        var app = LoanApplication.Create(loanAmount, assetValue, creditScore);
        
        // Assert
        Assert.NotEqual(Guid.Empty, app.Id);
        Assert.Equal(loanAmount, app.LoanAmount);
        Assert.Equal(assetValue, app.AssetValue);
        Assert.Equal(creditScore, app.CreditScore);
        Assert.Equal(50m, app.Ltv); 
        Assert.Equal(50m, app.Decision.CalculatedLtv);
        Assert.Equal(LoanStatus.Pending, app.Decision.Status);
        Assert.Empty(app.Decision.Reasons);
        Assert.Empty(app.Decision.RuleResults);
        Assert.True(app.CreatedAt <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData(0, 100_000, 800)]
    [InlineData(-1000, 100_000, 800)]
    public void Create_InvalidLoanAmount_ThrowsArgumentOutOfRangeException(decimal loan, decimal asset, int score)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LoanApplication.Create(loan, asset, score));
    }

    [Theory]
    [InlineData(100_000, 0, 800)]
    [InlineData(100_000, -5000, 800)]
    public void Create_InvalidAssetValue_ThrowsArgumentOutOfRangeException(decimal loan, decimal asset, int score)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LoanApplication.Create(loan, asset, score));
    }

    [Theory]
    [InlineData(100_000, 200_000, 0)]
    [InlineData(100_000, 200_000, 1000)]
    public void Create_InvalidCreditScore_ThrowsArgumentOutOfRangeException(decimal loan, decimal asset, int score)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LoanApplication.Create(loan, asset, score));
    }

    [Fact]
    public void UpdateDecision_SetsNewDecision()
    {
        // Arrange
        var app = LoanApplication.Create(500_000m, 1_000_000m, 800);
        var decision = LoanDecision.Declined(app.Ltv, new List<RuleResult>(), new[] { "Credit score too low" });

        // Act
        app.UpdateDecision(decision);

        // Assert
        Assert.Equal(LoanStatus.Declined, app.Decision.Status);
        Assert.Contains("Credit score too low", app.Decision.Reasons);
    }

    [Fact]
    public void UpdateDecision_NullDecision_ThrowsArgumentNullException()
    {
        // Arrange
        var app = LoanApplication.Create(500_000m, 1_000_000m, 800);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => app.UpdateDecision(null!));
    }
}
