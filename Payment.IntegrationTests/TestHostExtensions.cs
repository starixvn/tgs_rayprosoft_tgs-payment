using System.Text.Json;
using System.Text.Json.Serialization;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests;

internal static class TestHostExtensions
{
	public static PaymentServiceClient GetPaymentServiceClient(this TestPaymentApplication web)
		=> new(
			web.CreateClient(),
			new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				Converters = {
						new JsonStringEnumConverter()
				}
			});
}
