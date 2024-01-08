using JSLTSharp;

namespace Tgs.Payment.WithdrawProcessors;

internal class FiveSevenPayV2Processor(JsonTransform jsonTransform) : PhpGoPayProcessor(jsonTransform)
{
	public override string Subject => "tgs.payment.fivesevenpay-v2.withdraw";
}
