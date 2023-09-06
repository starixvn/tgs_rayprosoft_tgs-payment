using System;

namespace Tgs.Payment.ApplicationServices.Withdraws;

public record CreateWithdrawFormCommand(
	WithdrawUser User,
	Guid BrandId,
	Guid BankInfoId,
	decimal Amount,
	string? Note);
