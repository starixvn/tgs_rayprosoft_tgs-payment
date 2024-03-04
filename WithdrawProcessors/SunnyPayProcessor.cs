using JSLTSharp;

namespace Tgs.Payment.WithdrawProcessors;

internal class SunnyPayProcessor(JsonTransform jsonTransform) : PhpGoPayProcessor(jsonTransform)
{
	public override string Subject => "tgs.payment.sunnypay.withdraw";
}
