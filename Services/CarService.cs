using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public class CarService(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task<List<CarDto>> ListCarsAsync()
    {
        return await _db.Cars.Include(c => c.Owner)
            .Select(c => new CarDto(c.Id, c.Vin, c.Make, c.Model, c.YearOfManufacture,
                                    c.OwnerId, c.Owner.Name, c.Owner.Email))
            .ToListAsync();
    }

    public async Task<bool> IsInsuranceValidAsync(long carId, DateOnly date)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        return await _db.Policies.AnyAsync(p =>
            p.CarId == carId &&
            p.StartDate <= date &&
            p.EndDate >= date
        );
    }

    public async Task<long> RegisterClaimAsync(long carId, ClaimDto claimDto)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        var claim = new InsuranceClaim
        {
            CarId = carId,
            ClaimDate = claimDto.ClaimDate,
            Description = claimDto.Description,
            Amount = claimDto.Amount
        };
        _db.Claims.Add(claim);
        await _db.SaveChangesAsync();
        return claim.Id;
    }

    public async Task<CarHistoryResponse> GetCarHistoryAsync(long carId)
    {
        var car = await _db.Cars.FirstOrDefaultAsync(c => c.Id == carId);
        if (car == null) throw new KeyNotFoundException($"Car {carId} not found");

        // Add insurance policies
        var policies = await _db.Policies
            .Where(p => p.CarId == carId)
            .OrderBy(p => p.StartDate)
            .Select(p => new CarHistoryPolicyDto(
                p.Provider ?? "Unknown Provider",
                p.StartDate,
                p.EndDate,
                $"Insurance policy with {p.Provider ?? "Unknown Provider"} (valid until {p.EndDate})"
            ))
            .ToListAsync();

        // Add claims
        var claims = await _db.Claims
            .Where(c => c.CarId == carId)
            .OrderBy(c => c.ClaimDate)
            .Select(c => new CarHistoryClaimDto(
                c.ClaimDate,
                $"Insurance claim: {c.Description} (Amount: {c.Amount:C})"
            ))
            .ToListAsync();

        return new CarHistoryResponse(
            car.Id,
            car.Vin,
            policies.OrderBy(p => p.start_date),
            claims.OrderBy(c => c.claim_date)
        );
    }
}
