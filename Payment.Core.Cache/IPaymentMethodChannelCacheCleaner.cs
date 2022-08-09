namespace Tgs.Payment.Cache;

public interface IPaymentMethodChannelCacheCleaner
{
	void Clean(Guid pmcId);
}
