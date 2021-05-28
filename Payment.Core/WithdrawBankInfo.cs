using System;

namespace Tgs.Payment
{
	public record WithdrawBankInfo<TDetail>(
		Guid Id,
		Guid PaymentChannelId,
		string BankType,
		TDetail? Detail);
}
