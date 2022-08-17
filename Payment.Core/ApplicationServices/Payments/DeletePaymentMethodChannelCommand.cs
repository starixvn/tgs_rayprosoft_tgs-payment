using System;

namespace Tgs.Payment.ApplicationServices.Payments;
public record DeletePaymentMethodChannelCommand(Guid PaymentMethodChannelId);
