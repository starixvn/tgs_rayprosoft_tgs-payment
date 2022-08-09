namespace Tgs.Payment.Cache;

internal interface IPaymentCacheCleaner
{
	void Clean(string paymentCode);
}
