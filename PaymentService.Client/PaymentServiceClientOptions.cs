using System;
using System.Text.Json;

namespace Tgs.Payment.Clients
{
	public class PaymentServiceClientOptions
	{
		public JsonSerializerOptions? JsonSerializerOptions { get; set; }

		public Uri PaymentServiceBaseUrl { get; set; } = default!;
	}
}
