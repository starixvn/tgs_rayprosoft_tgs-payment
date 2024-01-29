using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.Payment;

public static class PaymentUtility
{
	public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		Converters = {
			new JsonStringEnumConverter()
		},
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
	};

	public static readonly TimeSpan ReportOffset = TimeSpan.FromHours(8D);
}
