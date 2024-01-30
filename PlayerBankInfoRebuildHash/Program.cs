using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NATS.Client.Core;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

var builder = Host.CreateApplicationBuilder(args);
{
	_ = builder.Services
		.AddKeyedSingleton("Nats", (sp, _) => new NatsConnection(NatsOpts.Default with
		{
			Url = builder.Configuration.GetConnectionString("Nats")!
		}))
		.AddDbContext<TgsContext>(builder => builder
			.UseNpgsql("Name=Tgs"))
		.AddPaymentClient()
		.AddMessageQueue()
		.AddNatsMessageQueue(config => config
			.ConfigureResolveConnection(sp => sp.GetRequiredKeyedService<NatsConnection>("Nats")))
		.AddNatsGlobPatternExchange("*");
}

using var app = builder.Build();

using var hostScope = app.Services.CreateAsyncScope();
await using (app.Services.CreateAsyncScope().ConfigureAwait(false))
{
	var dbContext = hostScope.ServiceProvider.GetRequiredService<TgsContext>();
	var paymentClient = hostScope.ServiceProvider.GetRequiredService<IPaymentClient>();

	var allBankInfos = dbContext.PlayerBankInfos.ToAsyncEnumerable();

	await foreach (var bankInfo in allBankInfos)
		await paymentClient
			.RebuildPlayerBankInfoHashAsync(bankInfo.BrandId, bankInfo.Id)
			.ConfigureAwait(false);
}
