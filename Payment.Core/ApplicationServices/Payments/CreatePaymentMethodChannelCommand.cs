using System;

namespace Tgs.Payment.ApplicationServices.Payments;
public record CreatePaymentMethodChannelCommand(Guid PaymentId, Guid MethodId, Guid ChannelId);
