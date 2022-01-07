using System;

namespace Tgs.Payment.ApplicationServices.Withdraws;

public record SignOffWithdrawFormCommand(
	Guid FormId,
	Guid? BrandPaymentId,
	ReviewState Status,
	string? ReviewUserId,
	string? ReviewUserName,
	string? Remark);
