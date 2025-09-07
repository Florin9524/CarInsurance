using CarInsurance.Api.Data;
using CarInsurance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public class PolicyExpirationService : BackgroundService
{
    private readonly ILogger<PolicyExpirationService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly HashSet<long> _processedPolicies = new();
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(59);

    public PolicyExpirationService(
        ILogger<PolicyExpirationService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // var oneHourAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-6));
                // _logger.LogCritical("Car Insurance bck service: " + oneHourAgo);
                var oneHourAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-1));

                // Find policies that expired exactly 1 hour ago and haven't been processed
                var expiredPolicies = await db.Policies
                    .Include(p => p.Car)
                    .Where(p => p.EndDate == oneHourAgo && !_processedPolicies.Contains(p.Id))
                    .ToListAsync(stoppingToken);

                foreach (var policy in expiredPolicies)
                {
                    _logger.LogWarning(
                        "Policy {PolicyId} for car {CarVin} expired at {ExpiryDate}",
                        policy.Id,
                        policy.Car.Vin,
                        policy.EndDate);

                    _processedPolicies.Add(policy.Id);
                }

                // var processedPoliciesDetails = await db.Policies
                //     .Include(p => p.Car)
                //     .Where(p => _processedPolicies.Contains(p.Id))
                //     .ToListAsync(stoppingToken);

                // foreach (var policy in processedPoliciesDetails)
                // {
                //     _logger.LogInformation(
                //         "Previously processed: Policy {PolicyId} for car {CarVin} expired at {ExpiryDate}",
                //         policy.Id,
                //         policy.Car.Vin,
                //         policy.EndDate);
                // }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired policies");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}
