using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.Payment;

internal static class PaymentUtility
{
	public static readonly JsonSerializerOptions SerializerOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		Converters = {
			new JsonStringEnumConverter()
		}
	};
}
