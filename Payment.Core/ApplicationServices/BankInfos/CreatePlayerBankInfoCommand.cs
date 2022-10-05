using System;
using System.Text.Json;

namespace Tgs.Payment.ApplicationServices.BankInfos;

public record CreatePlayerBankInfoCommand(
	Guid BrandId,
	string PlayerCode,
	Guid PaymentChannelId,
	string BankType,
	JsonDocument Detail,
	bool IsDefault,
	string ModifyUser);
