using Microsoft.EntityFrameworkCore;
using Lending.Domain.Entities;
using Lending.Infrastructure.Data.Configurations;

namespace Lending.Infrastructure.Data;

public class LendingDbContext : DbContext
{
    public LendingDbContext(DbContextOptions<LendingDbContext> options) : base(options)
    {
    }

    public DbSet<LoanApplication> LoanApplications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new LoanApplicationConfiguration());
    }
}
