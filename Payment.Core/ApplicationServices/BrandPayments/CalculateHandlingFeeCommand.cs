using System;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

public record CalculateHandlingFeeCommand(Guid BrandPaymentId, decimal Amount);
