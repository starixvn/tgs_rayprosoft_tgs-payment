using System;
using Tgs.Payment.ManualDeposit;
using TGS.Payment.ManualDeposit;

namespace Tgs.Payment.WithdrawProcessors;

internal class ManualPayProcessor(ManualDepositSerializer manualDepositSerializer) : IWithdrawProcessor
{
	public string Subject => "tgs.payment.manual.withdraw";

	public string MakeWithdrawSettings(
		Guid formId,
		decimal amount,
		string bankInfo,
		BrandPaymentInfo brandPayment)
	{
		var applyInfo = new ManualWithdrawApplyInfo();

		return manualDepositSerializer.Serialize(applyInfo);
	}
}
