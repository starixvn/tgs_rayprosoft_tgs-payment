using JSLTSharp;

namespace Tgs.Payment.WithdrawProcessors;

internal class EzPayProcessor(JsonTransform jsonTransform) : GCPayProcessor(jsonTransform)
{
	public override string Subject => "tgs.payment.ezpay.withdraw";
}
