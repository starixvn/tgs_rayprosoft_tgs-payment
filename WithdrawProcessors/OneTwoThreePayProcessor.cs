using JSLTSharp;

namespace Tgs.Payment.WithdrawProcessors;

internal class OneTwoThreePayProcessor(JsonTransform jsonTransform) : PhpGoPayProcessor(jsonTransform)
{
	public override string Subject => "tgs.payment.123pay.withdraw";
}
