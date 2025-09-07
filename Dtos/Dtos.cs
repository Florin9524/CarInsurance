namespace CarInsurance.Api.Dtos;

public record CarDto(long Id, string Vin, string? Make, string? Model, int Year, long OwnerId, string OwnerName, string? OwnerEmail);
public record InsuranceValidityResponse(long CarId, string Date, bool Valid);
public record ClaimDto(DateOnly ClaimDate, string Description, decimal Amount);

public record CarHistoryPolicyDto(string policy_provider, DateOnly start_date, DateOnly end_date, string Description);
public record CarHistoryClaimDto(DateOnly claim_date, string Description);
public record CarHistoryResponse(long CarId, string Vin, IEnumerable<CarHistoryPolicyDto> Policies, IEnumerable<CarHistoryClaimDto> Claims);