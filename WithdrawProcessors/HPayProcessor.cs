using JSLTSharp;

namespace Tgs.Payment.WithdrawProcessors;

internal class HPayProcessor : GCPayProcessor
{
	public override string Subject => "tgs.payment.hpay.withdraw";

	public HPayProcessor(JsonTransform jsonTransform)
		: base(jsonTransform)
	{
	}
}
