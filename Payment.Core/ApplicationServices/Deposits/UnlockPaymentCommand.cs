namespace Tgs.Payment.ApplicationServices.Deposits;
public record UnlockPaymentCommand(string PaymentCode, string PlayerCode);
