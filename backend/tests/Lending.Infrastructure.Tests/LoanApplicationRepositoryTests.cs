using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Lending.Domain.Entities;
using Lending.Domain.ValueObjects;
using Lending.Domain.Enums;
using Lending.Infrastructure.Data;
using Lending.Infrastructure.Repositories;

namespace Lending.Infrastructure.Tests;

public class LoanApplicationRepositoryTests : IDisposable
{
    private readonly LendingDbContext _dbContext;
    private readonly LoanApplicationRepository _repository;

    public LoanApplicationRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<LendingDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _dbContext = new LendingDbContext(options);
        _dbContext.Database.OpenConnection();
        _dbContext.Database.EnsureCreated();

        _repository = new LoanApplicationRepository(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Database.CloseConnection();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task AddAndGetById_PersistsAndRetrievesApplicationWithDecision()
    {
        // Arrange
        var app = LoanApplication.Create(500_000m, 1_000_000m, 800);
        
        // Update decision to simulate engine execution
        var rules = new[] { RuleResult.Fail("TestRule", "Credit score too low") };
        var reasons = new[] { "Credit score too low" };
        var decision = LoanDecision.Declined(app.Ltv, rules, reasons);
        
        app.UpdateDecision(decision);

        // Act
        await _repository.AddAsync(app);
        var retrievedApp = await _repository.GetByIdAsync(app.Id);

        // Assert
        Assert.NotNull(retrievedApp);
        Assert.Equal(app.Id, retrievedApp.Id);
        Assert.Equal(500_000m, retrievedApp.LoanAmount);
        Assert.Equal(1_000_000m, retrievedApp.AssetValue);
        Assert.Equal(800, retrievedApp.CreditScore);
        Assert.Equal(50m, retrievedApp.Ltv);
        
        // Assert Decision was mapped correctly via JSON conversion
        Assert.NotNull(retrievedApp.Decision);
        Assert.Equal(LoanStatus.Declined, retrievedApp.Decision.Status);
        Assert.Equal(50m, retrievedApp.Decision.CalculatedLtv);
        
        Assert.Single(retrievedApp.Decision.RuleResults);
        Assert.Equal("TestRule", retrievedApp.Decision.RuleResults.First().RuleName);
        Assert.False(retrievedApp.Decision.RuleResults.First().IsPassed);
        
        Assert.Single(retrievedApp.Decision.Reasons);
        Assert.Equal("Credit score too low", retrievedApp.Decision.Reasons.First());
    }

    [Fact]
    public async Task GetAllAsync_RetrievesMultipleApplications()
    {
        // Arrange
        var app1 = LoanApplication.Create(100_000m, 200_000m, 800);
        var app2 = LoanApplication.Create(200_000m, 300_000m, 900);
        
        await _repository.AddAsync(app1);
        await _repository.AddAsync(app2);

        // Act
        var apps = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(apps);
        Assert.Equal(2, apps.Count());
        Assert.Contains(apps, a => a.Id == app1.Id);
        Assert.Contains(apps, a => a.Id == app2.Id);
    }
}
