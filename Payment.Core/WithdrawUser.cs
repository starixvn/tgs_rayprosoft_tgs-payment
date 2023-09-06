namespace Tgs.Payment;
public record WithdrawUser(
	UserType UserType,
	string? UserId,
	string? UserName);
