using System;
using Lending.Domain.Enums;
using Lending.Domain.ValueObjects;

namespace Lending.Domain.Entities;

public class LoanApplication
{
    public Guid Id { get; private set; }
    public decimal LoanAmount { get; private set; }
    public decimal AssetValue { get; private set; }
    public int CreditScore { get; private set; }
    public decimal Ltv { get; private set; }
    public LoanDecision Decision { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    // Required for Entity Framework Core hydration
    private LoanApplication() 
    {
    }

    private LoanApplication(decimal loanAmount, decimal assetValue, int creditScore)
    {
        if (loanAmount <= 0) 
            throw new ArgumentOutOfRangeException(nameof(loanAmount), "Loan amount must be greater than zero.");
        if (assetValue <= 0) 
            throw new ArgumentOutOfRangeException(nameof(assetValue), "Asset value must be greater than zero.");
        if (creditScore < 1 || creditScore > 999) 
            throw new ArgumentOutOfRangeException(nameof(creditScore), "Credit score must be between 1 and 999.");

        Id = Guid.NewGuid();
        LoanAmount = loanAmount;
        AssetValue = assetValue;
        CreditScore = creditScore;
        Ltv = (loanAmount / assetValue) * 100m;
        Decision = LoanDecision.Pending(Ltv);
        CreatedAt = DateTime.UtcNow;
    }

    public static LoanApplication Create(decimal loanAmount, decimal assetValue, int creditScore)
    {
        return new LoanApplication(loanAmount, assetValue, creditScore);
    }
    
    public void UpdateDecision(LoanDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);
        Decision = decision;
    }
}
