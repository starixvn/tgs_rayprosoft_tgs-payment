using System;

namespace Tgs.Payment.ApplicationServices.BankInfos;

public record SetDefaultPlayerBankInfoCommand(
	string PlayerCode,
	Guid BankInfoId,
	string ModifyUser);
