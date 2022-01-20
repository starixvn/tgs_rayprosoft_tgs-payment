using System;

namespace Tgs.Payment.ApplicationServices.Deposits;

public record SignOffDepositFormCommand(
	Guid FormId,
	ReviewState ReviewState,
	string? ReviewUserId,
	string? ReviewUserName,
	string? Remark);
