using Tgs.Payment;
using Tgs.Payment.ApplicationServices.Payments;
using Tgs.Payment.Cache;
using Tgs.ServiceUtility;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPaymentCache(this IServiceCollection services)
		=> services
			.Decorate<IQueryService<GetAllPaymentsQuery, PaymentInfo>, GetAllCachedPaymentsQueryService>()
			.AddTransient<IPaymentCacheCleaner, GetAllCachedPaymentsQueryService>()
			.Decorate<IQueryService<AllPaymentMethodsQuery, PaymentMethodInfo>, AllCachedPaymentMethodsQueryService>()
			.AddTransient<IPaymentMethodCacheCleaner, AllCachedPaymentMethodsQueryService>()
			.Decorate<IQueryService<AllPaymentChannelsQuery, PaymentChannelInfo>, AllCachedPaymentChannelsQueryService>()
			.AddTransient<IPaymentChannelCacheCleaner, AllCachedPaymentChannelsQueryService>()
			.Decorate<IQueryService<AllPaymentMethodChannelsQuery, PaymentMethodChannelInfo>, AllCachedPaymentMethodChannelsQueryService>()
			.AddTransient<IPaymentMethodChannelCacheCleaner, AllCachedPaymentMethodChannelsQueryService>();
}
