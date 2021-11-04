using System;
using System.Collections.Generic;

namespace Tgs.Payment.Clients
{
	public class WithdrawFormQueryConditions
	{
		public Guid BrandId { get; init; }

		public string? ChannelName { get; init; }

		public (DateTimeOffset Begin, DateTimeOffset End)? CreateOnRange { get; init; }

		public Guid? FormId { get; init; }

		public Guid? PaymentMethodId { get; init; }

		public IEnumerable<PaymentFlowState> Status { get; init; } = Array.Empty<PaymentFlowState>();

		public string? ThirdPartyOrderNo { get; init; }

		public string? OrderNo { get; init; }

		public (DateTimeOffset Begin, DateTimeOffset End)? UpdateOnRange { get; init; }

		public string? UserId { get; init; }

		public string? UserName { get; init; }

		public UserType? UserType { get; init; }
	}
}
