namespace Tgs.Payment.Clients
{
	public class PlayerInfo
	{
		public Guid BrandId { get; init; }

		public decimal DepositedAmount { get; init; }

		public bool IdentityAuthentication { get; init; }

		public bool PhoneAuthentication { get; init; }

		public string PlayerCode { get; init; } = default!;

		public Guid? VipLevelId { get; init; }
	}
}
