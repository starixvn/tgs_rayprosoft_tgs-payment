using System;

namespace Tgs.Payment.ApplicationServices.Withdraws;
public record AutoApproveWithdtawFormCommand(
	Guid BrandId,
	Guid FormId,
	PaymentFlowState Status);
