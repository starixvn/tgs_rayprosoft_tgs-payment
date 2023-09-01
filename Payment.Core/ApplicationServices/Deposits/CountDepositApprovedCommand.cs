using System;

namespace Tgs.Payment.ApplicationServices.Deposits;
public record CountDepositApprovedCommand(Guid BrandId, Guid FormId, PaymentFlowState Status);
