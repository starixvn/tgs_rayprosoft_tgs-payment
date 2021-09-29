using System;

namespace Tgs.Payment.Clients
{
	public class WithdrawFormCreateInfo
    {
        public decimal Amount { get; init; }

        public Guid BankInfoId { get; init; }

        public Guid BrandId { get; init; }

        public string? UserId { get; init; }

        public string? UserName { get; init; }

        public UserType UserType { get; init; }
    }
}