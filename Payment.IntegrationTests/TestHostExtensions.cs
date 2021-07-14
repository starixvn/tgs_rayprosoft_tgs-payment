using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Tgs.Payment.Clients;

namespace Payment.IntegrationTests
{
	internal static class TestHostExtensions
	{
		public static PaymentServiceClient GetPaymentServiceClient(this IHost host)
			=> new PaymentServiceClient(
				host.GetTestClient(),
				new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					Converters = {
						new JsonStringEnumConverter()
					}
				});
	}
}
