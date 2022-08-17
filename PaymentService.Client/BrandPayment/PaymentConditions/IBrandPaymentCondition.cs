namespace Tgs.Payment.Clients.BrandPayment.PaymentConditions;

public interface IBrandPaymentCondition
{
	bool IsValid(BrandPaymentInfo brandPayment, PlayerInfo player);
}
