using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Lending.Application.Repositories;
using Lending.Application.Services;
using Lending.Domain.Services;
using Lending.Infrastructure.Data;
using Lending.Infrastructure.Repositories;
using Lending.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<LendingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<ILoanApplicationRepository, LoanApplicationRepository>();

// Domain Rules & Engine
builder.Services.AddScoped<ILoanRule, GeneralLimitsRule>();
builder.Services.AddScoped<ILoanRule, LargeLoanRule>();
builder.Services.AddScoped<ILoanRule, SmallLoanRule>();
builder.Services.AddScoped<LoanDecisionEngine>();
builder.Services.AddScoped<DecisionSimulator>();

// Application Services
builder.Services.AddScoped<ILoanApplicationService, LoanApplicationService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Run migrations at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LendingDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapGet("/health", () => Microsoft.AspNetCore.Http.Results.Ok(new { Status = "Healthy", Timestamp = System.DateTime.UtcNow }));
app.MapControllers();

app.Run();

public partial class Program {}
