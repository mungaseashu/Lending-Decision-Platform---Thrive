using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Lending.Application.DTOs;
using Lending.Domain.Enums;

namespace Lending.Api.Tests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostApplication_ValidApproved_Returns201CreatedAndCompleteDecision()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateLoanApplicationRequest
        {
            LoanAmount = 750_000,
            AssetValue = 1_200_000,
            CreditScore = 825
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/loan-applications", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LoanApplicationResponse>();
        Assert.NotNull(result);
        Assert.NotEqual(System.Guid.Empty, result.Id);
        Assert.Equal(750_000m, result.LoanAmount);
        Assert.Equal(1_200_000m, result.AssetValue);
        Assert.Equal(825, result.CreditScore);
        Assert.Equal(62.5m, result.Ltv);
        
        Assert.NotNull(result.Decision);
        Assert.Equal(LoanStatus.Approved.ToString(), result.Decision.Status);
        Assert.Empty(result.Decision.Reasons);
    }

    [Fact]
    public async Task PostApplication_ValidDeclined_Returns201CreatedAndDeclineReasons()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateLoanApplicationRequest
        {
            LoanAmount = 1_500_001, // Fails general limit
            AssetValue = 1_200_000, // Fails small loan limit (or rather large loan limit)
            CreditScore = 500
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/loan-applications", request);

        // Assert
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<LoanApplicationResponse>();
        Assert.NotNull(result);
        Assert.Equal(LoanStatus.Declined.ToString(), result.Decision.Status);
        Assert.NotEmpty(result.Decision.Reasons);
    }

    [Fact]
    public async Task PostApplication_InvalidInput_Returns400BadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateLoanApplicationRequest
        {
            LoanAmount = 0, // Invalid
            AssetValue = -100, // Invalid
            CreditScore = 1500 // Invalid
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/loan-applications", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboard_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/dashboard");

        // Assert
        response.EnsureSuccessStatusCode();
        var stats = await response.Content.ReadFromJsonAsync<DashboardStatsResponse>();
        Assert.NotNull(stats);
        Assert.True(stats.TotalApplicants >= 0);
    }
}
