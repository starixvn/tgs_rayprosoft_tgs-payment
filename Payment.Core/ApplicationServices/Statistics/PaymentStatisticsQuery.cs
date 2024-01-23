using System;

namespace Tgs.Payment.ApplicationServices.Statistics;
public record PaymentStatisticsQuery(
	DateOnly Start,
	DateOnly End,
	string[]? UserIds);
