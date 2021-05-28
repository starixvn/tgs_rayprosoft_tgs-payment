using System;

namespace Tgs.Payment
{
	public record BankInfo(
		Guid Id,
		string PlayerCode,
		Guid BrandId,
		Guid PaymentChannelId,
		string BankType,
		string Detail);
}
