namespace Tgs.Payment.Cache;

public interface IPaymentMethodCacheCleaner
{
	void Clean(string name);
}
