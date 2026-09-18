using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Lending.Domain.Entities;
using Lending.Domain.ValueObjects;

namespace Lending.Infrastructure.Data.Configurations;

public class LoanApplicationConfiguration : IEntityTypeConfiguration<LoanApplication>
{
    public void Configure(EntityTypeBuilder<LoanApplication> builder)
    {
        builder.ToTable("LoanApplications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LoanAmount)
            .IsRequired()
            .HasColumnType("TEXT"); // SQLite stores decimal as TEXT

        builder.Property(x => x.AssetValue)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(x => x.CreditScore)
            .IsRequired();

        builder.Property(x => x.Ltv)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Convert the Value Object to a JSON string for SQLite persistence
        builder.Property(x => x.Decision)
            .IsRequired()
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<LoanDecision>(v, (JsonSerializerOptions)null)!
            )
            .HasColumnType("TEXT");
    }
}
