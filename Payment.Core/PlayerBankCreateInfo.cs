using System;
using System.Text.Json;

namespace Tgs.Payment;

public record PlayerBankCreateInfo(
	Guid Id,
	string PlayerCode,
	Guid BrandId,
	Guid PaymentChannelId,
	string BankType,
	JsonDocument Detail,
	bool IsDefault,
	byte[]? DetailHash);
