using System;

namespace Tgs.Payment.ApplicationServices.Statistics;
public record PaymentStatistics5minQuery(
	DateTime Start,
	DateTime End);
