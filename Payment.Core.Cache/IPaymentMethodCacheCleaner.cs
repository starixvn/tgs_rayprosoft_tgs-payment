namespace Tgs.Payment.Cache;

internal interface IPaymentMethodCacheCleaner
{
	void Clean(string name);
}
