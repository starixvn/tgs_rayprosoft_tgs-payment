using System;

namespace Tgs.Payment.ApplicationServices
{
	public record DeletePlayerBankInfoCommand(
		string PlayerCode,
		Guid BankInfoId);
}
