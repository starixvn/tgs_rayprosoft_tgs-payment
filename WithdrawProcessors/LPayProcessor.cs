using JSLTSharp;

namespace Tgs.Payment.WithdrawProcessors;

internal class LPayProcessor(JsonTransform jsonTransform) : PhpGoPayProcessor(jsonTransform)
{
	public override string Subject => "tgs.payment.lpay.withdraw";
}
