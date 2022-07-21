using System;

namespace Tgs.Payment.ApplicationServices.Deposits;
public record LockPaymentCommand(Guid FormId, string PaymentCode, string PlayerCode);
