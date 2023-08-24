using System;
using System.Text.Json;

namespace Tgs.Payment.ApplicationServices.BrandPayments;
public record UpdateBrandAutoWithdrawCommand(
	Guid BrandId,
	bool EnableAutoWithdraw,
	decimal AmountUpperLimit,
	JsonDocument BrandPaymentCandidates,
	string UserId);
