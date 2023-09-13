using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

var app = Host.CreateDefaultBuilder()
	.ConfigureAppConfiguration(builder => builder
		.AddJsonFile("appsettings.json")
		.AddEnvironmentVariables())
	.ConfigureServices(services => services
		.AddDbContext<TgsContext>(builder => builder
			.UseNpgsql("Name=Tgs"))
		.AddPaymentClient()
		.AddPhoenixToolkitsMessageQueue()
		.AddNatsMessageQueue(config => config
			.ConfigQueueOptions((options, sp) =>
			{
				var config = sp.GetRequiredService<IConfiguration>();

				options.Url = config.GetConnectionString("Nats")!;
			}))
		.AddNatsGlobPatternExchange("*"))
	.Build();

await using var hostScope = app.Services.CreateAsyncScope();

var dbContext = hostScope.ServiceProvider.GetRequiredService<TgsContext>();
var paymentClient = hostScope.ServiceProvider.GetRequiredService<IPaymentClient>();

var allBankInfos = dbContext.PlayerBankInfos.ToAsyncEnumerable();

await foreach (var bankInfo in allBankInfos)
	await paymentClient
		.RebuildPlayerBankInfoHashAsync(bankInfo.BrandId, bankInfo.Id)
		.ConfigureAwait(false);
