using System;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

public record BrandPaymentsQuery(
	Guid? BrandId,
	TransferType? TransferType,
	bool? Enabled,
	string? Keyword);
