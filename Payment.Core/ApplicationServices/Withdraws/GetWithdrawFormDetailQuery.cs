using System;

namespace Tgs.Payment.ApplicationServices.Withdraws;

public record GetWithdrawFormDetailQuery(
	Guid FormId,
	Guid BrandId);
