using System;
using Tgs.Payment.ManualDeposit;
using TGS.Payment.ManualDeposit;

namespace Tgs.Payment.WithdrawProcessors
{
	internal class ManualPayProcessor : IWithdrawProcessor
	{
		private readonly ManualDepositSerializer _manualDepositSerializer;

		public string Subject => "tgs.payment.manual.withdraw";

		public ManualPayProcessor(ManualDepositSerializer manualDepositSerializer)
		{
			_manualDepositSerializer = manualDepositSerializer ?? throw new ArgumentNullException(nameof(manualDepositSerializer));
		}

		public string MakeWithdrawSettings(
			Guid formId,
			decimal amount,
			string bankInfo,
			BrandPaymentInfo brandPayment)
		{
			var applyInfo = new ManualWithdrawApplyInfo();

			return _manualDepositSerializer.Serialize(applyInfo);
		}
	}
}
