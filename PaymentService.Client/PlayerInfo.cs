namespace Tgs.Payment.Clients;

public record PlayerInfo(Guid BrandId, decimal DepositedAmount, bool IdentityAuthentication, bool PhoneAuthentication, string PlayerCode, Guid? VipLevelId);
