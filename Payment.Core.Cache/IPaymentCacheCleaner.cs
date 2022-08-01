namespace Tgs.Payment.Cache;

public interface IPaymentCacheCleaner
{
	void Clean(string paymentCode);
}
