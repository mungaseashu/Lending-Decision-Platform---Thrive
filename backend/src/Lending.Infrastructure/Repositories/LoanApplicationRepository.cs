using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Lending.Application.Repositories;
using Lending.Domain.Entities;
using Lending.Infrastructure.Data;

namespace Lending.Infrastructure.Repositories;

public class LoanApplicationRepository : ILoanApplicationRepository
{
    private readonly LendingDbContext _dbContext;

    public LoanApplicationRepository(LendingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(LoanApplication application)
    {
        await _dbContext.LoanApplications.AddAsync(application);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<LoanApplication?> GetByIdAsync(Guid id)
    {
        return await _dbContext.LoanApplications
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<LoanApplication>> GetAllAsync()
    {
        return await _dbContext.LoanApplications
            .ToListAsync();
    }

    public async Task<(int Total, int Approved, int Declined, decimal TotalValue, decimal MeanLtv)> GetDashboardAggregatesAsync()
    {
        int total = 0;
        int approved = 0;
        decimal totalValue = 0m;
        decimal sumLtv = 0m;

        await foreach (var app in _dbContext.LoanApplications.AsAsyncEnumerable())
        {
            total++;
            sumLtv += app.Ltv;
            if (app.Decision.Status == Lending.Domain.Enums.LoanStatus.Approved)
            {
                approved++;
                totalValue += app.LoanAmount;
            }
        }

        var declined = total - approved;
        var meanLtv = total > 0 ? sumLtv / total : 0m;

        return (total, approved, declined, totalValue, meanLtv);
    }
}
