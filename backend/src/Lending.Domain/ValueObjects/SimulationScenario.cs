namespace Lending.Domain.ValueObjects;

public record SimulationScenario
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal? TargetLoanAmount { get; init; }
    public decimal? TargetAssetValue { get; init; }
    public int? TargetCreditScore { get; init; }
}
