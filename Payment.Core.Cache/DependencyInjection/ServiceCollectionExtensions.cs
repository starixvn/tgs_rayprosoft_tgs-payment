using Tgs.Payment;
using Tgs.Payment.ApplicationServices.Payments;
using Tgs.ServiceUtility;
using Tgs.Payment.Cache;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPaymentCache(this IServiceCollection services)
		=> services
			.Decorate<IQueryService<GetAllPaymentsQuery, PaymentInfo>, GetAllCachedPaymentsQueryService>()
			.AddTransient<IPaymentCacheCleaner, GetAllCachedPaymentsQueryService>();
}
