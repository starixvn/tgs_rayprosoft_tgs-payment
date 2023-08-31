using System;

namespace Tgs.Payment.ApplicationServices.Withdraws;
public record CountWithdrawPaidCommand(
	Guid BrandId,
	Guid FormId,
	string Status);
