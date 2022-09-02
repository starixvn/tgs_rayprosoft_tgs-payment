using System;

namespace Tgs.Payment;

public record BrandPaymentInfo(
	Guid Id,
	Guid PaymentMethodChannelId,
	Guid PaymentId,
	Guid MethodId,
	Guid ChannelId,
	string ChannelName,
	string ChannelCode,
	string ExtendedData,
	ValueRange<decimal> TransferLimit,
	bool IsAllPlayers,
	bool AuthenticateIdentity,
	bool AuthenticatePhone,
	ValueRange<decimal> DepositedAmountLimit,
	ConditionMatchType ConditionMatchType,
	TransferType TransferType,
	bool Enabled,
	int DisplayOrder,
	Guid BrandId);
