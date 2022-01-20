using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Tgs.Payment;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests;

internal static class TestSiteHelper
{
	public static TestPaymentApplication GetPaymentServiceTestHost()
		=> GetPaymentServiceTestHost(services => { });

	public static TestPaymentApplication GetPaymentServiceTestHost(Action<IServiceCollection> configureServices)
		=> new(webBuilder =>
		{
			_ = webBuilder
				.ConfigureAppConfiguration((ctx, builder) =>
				{
					builder.Sources.Clear();
				})
				.ConfigureServices(services =>
				{
					_ = services
						.AddFakeNatsMessageQueue()
						.Replace(ServiceDescriptor.Singleton(sp =>
						{
							var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
							_ = dbContextOptionsBuilder
								.UseInMemoryDatabase(Guid.NewGuid().ToString())
								.ConfigureWarnings(x => x.Ignore(
									InMemoryEventId.TransactionIgnoredWarning));

							return new TgsContext(dbContextOptionsBuilder.Options);
						}));

					_ = services.AddSingleton(services =>
					  {
						  var fakeStore = Substitute.For<ITakeNumberStore>();

						  return fakeStore;
					  });

					configureServices(services);
				});
		});
}
