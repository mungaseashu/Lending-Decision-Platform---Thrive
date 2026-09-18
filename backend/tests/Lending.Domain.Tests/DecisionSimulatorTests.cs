using System.Linq;
using Xunit;
using Lending.Domain.Entities;
using Lending.Domain.Services;
using Lending.Domain.Enums;

namespace Lending.Domain.Tests;

public class DecisionSimulatorTests
{
    private readonly DecisionSimulator _simulator = new DecisionSimulator();
    private readonly LoanDecisionEngine _engine = new LoanDecisionEngine(new ILoanRule[]
    {
        new GeneralLimitsRule(),
        new LargeLoanRule(),
        new SmallLoanRule()
    });

    private LoanApplication Evaluate(decimal loan, decimal asset, int score)
    {
        var app = LoanApplication.Create(loan, asset, score);
        app.UpdateDecision(_engine.Evaluate(app));
        return app;
    }

    [Fact]
    public void GenerateScenarios_ApprovedApplication_ReturnsEmpty()
    {
        var app = Evaluate(500_000, 1_000_000, 800);
        var scenarios = _simulator.GenerateScenarios(app);
        Assert.Empty(scenarios);
    }

    [Fact]
    public void GenerateScenarios_FailsGeneralMinimum_Suggests100k()
    {
        var app = Evaluate(50_000, 1_000_000, 800);
        var scenarios = _simulator.GenerateScenarios(app).ToList();
        
        Assert.NotEmpty(scenarios);
        var sim = scenarios.Single(s => s.Title == "Increase Loan Amount");
        Assert.Equal(100_000m, sim.TargetLoanAmount);
    }

    [Fact]
    public void GenerateScenarios_LargeLoan_FailsScore_Suggests950()
    {
        // 1.2M loan, 2.4M asset (LTV 50% = Pass), Score 900 (Fail >= 950)
        var app = Evaluate(1_200_000, 2_400_000, 900);
        var scenarios = _simulator.GenerateScenarios(app).ToList();
        
        Assert.Single(scenarios);
        var sim = scenarios.Single();
        Assert.Equal("Improve Credit Score", sim.Title);
        Assert.Equal(950, sim.TargetCreditScore);
    }

    [Fact]
    public void GenerateScenarios_LargeLoan_FailsLtv_CalculatesRequiredAssetAndLoan()
    {
        // 1.2M loan, 1.5M asset (LTV 80% = Fail <= 60%), Score 950 (Pass)
        var app = Evaluate(1_200_000, 1_500_000, 950);
        var scenarios = _simulator.GenerateScenarios(app).ToList();
        
        Assert.Single(scenarios);
        var sim = scenarios.Single();
        Assert.Equal("Improve LTV to <= 60%", sim.Title);
        
        // Asset required = 1.2M / 0.6 = 2,000,000
        Assert.Equal(2_000_000m, sim.TargetAssetValue);
        
        // Loan max = 1.5M * 0.6 = 900,000 (Wait, if loan is 900k, it drops out of the large loan tier. 
        // Our simulator specifically checks maxLoan >= 1M to assign it).
        Assert.Null(sim.TargetLoanAmount); 
    }

    [Fact]
    public void GenerateScenarios_StandardLoan_CalculatesExactThresholds()
    {
        // Loan = £850,000, Asset = £900,000 => LTV = 94.44% (Fails >= 90%)
        var app = Evaluate(850_000, 900_000, 950);
        var scenarios = _simulator.GenerateScenarios(app).ToList();

        var sim = scenarios.Single(s => s.Title == "Reduce LTV below 90%");
        // Required asset = ceil(850k / 0.8999) = 944,550
        Assert.Equal(944550m, sim.TargetAssetValue);
        // Required loan = floor(900k * 0.8999) = 809,910
        Assert.Equal(809910m, sim.TargetLoanAmount);
    }
}
