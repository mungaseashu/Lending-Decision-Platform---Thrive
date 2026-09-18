using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lending.Domain.Entities;

namespace Lending.Application.Repositories;

public interface ILoanApplicationRepository
{
    Task AddAsync(LoanApplication application);
    Task<LoanApplication?> GetByIdAsync(Guid id);
    Task<IEnumerable<LoanApplication>> GetAllAsync();
    Task<(int Total, int Approved, int Declined, decimal TotalValue, decimal MeanLtv)> GetDashboardAggregatesAsync();
}
