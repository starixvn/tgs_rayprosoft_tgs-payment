using System;
using System.Text.Json;

namespace Tgs.Payment.Clients
{
	public class AgentBankInfo
	{
		public Guid AgentId { get; init; } = default!;

		public string BankType { get; init; } = default!;

		public Guid BrandId { get; init; }

		public JsonDocument Detail { get; init; } = default!;

		public Guid Id { get; init; }

		public Guid PaymentChannelId { get; init; }
	}
}
