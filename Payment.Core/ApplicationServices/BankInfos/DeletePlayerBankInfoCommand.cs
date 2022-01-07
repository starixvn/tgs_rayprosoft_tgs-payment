using System;

namespace Tgs.Payment.ApplicationServices.BankInfos;

public record DeletePlayerBankInfoCommand(
	string PlayerCode,
	Guid BankInfoId);
