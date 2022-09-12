using System;

namespace Tgs.Payment;

public record BrandPaymentFeeInfo(
	decimal HandlingFeeRateTenPercentile,
	bool ReducePlayerAmount,
	Guid BrandPaymentId);
