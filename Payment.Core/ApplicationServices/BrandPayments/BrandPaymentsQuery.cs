using System;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

public record BrandPaymentsQuery(
	Guid BrandId, 
	string PlayerCode,
	bool IdentityAuthentication,
	bool PhoneAuthentication,
	Guid? VipLevelId,
	decimal DepositedAmount);
