namespace Tgs.Payment;
public record AutoWithdrawInfo(
	bool EnableAutoWithdraw,
	decimal AmountUpperLimit,
	string BrandPaymentCandidates);
