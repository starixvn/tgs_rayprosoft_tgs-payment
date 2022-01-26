namespace Tgs.Payment.ApplicationServices.Payments;

public record CreatePaymentMethodCommand(
	string Code,
	string BankType);
