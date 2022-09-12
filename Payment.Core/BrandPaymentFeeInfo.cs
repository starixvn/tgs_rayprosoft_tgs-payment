using System;

namespace Tgs.Payment;

public record BrandPaymentFeeInfo(
	Guid BrandPaymentId,
	decimal HandlingFeeRateTenPercentile,
	bool ReducePlayerAmount);
