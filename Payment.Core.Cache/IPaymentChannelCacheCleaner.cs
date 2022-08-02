namespace Tgs.Payment.Cache;

internal interface IPaymentChannelCacheCleaner
{
	void Clean(string channelCode);
}
