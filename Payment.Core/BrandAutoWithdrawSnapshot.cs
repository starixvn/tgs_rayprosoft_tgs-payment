using System.Text.Json;

namespace Tgs.Payment;
public record BrandAutoWithdrawSnapshot(
	bool EnableAutoWithdraw,
	decimal AmountUpperLimit,
	JsonDocument BrandPaymentCandidates);
