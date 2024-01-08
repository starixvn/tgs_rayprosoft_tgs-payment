using JSLTSharp;

namespace Tgs.Payment.WithdrawProcessors;

internal class HPayProcessor(JsonTransform jsonTransform) : GCPayProcessor(jsonTransform)
{
	public override string Subject => "tgs.payment.hpay.withdraw";
}
