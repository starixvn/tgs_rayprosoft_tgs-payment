using JSLTSharp;

namespace Tgs.Payment.WithdrawProcessors;

internal class FiveSevenPayV2Processor : PhpGoPayProcessor
{
	public override string Subject => "tgs.payment.fivesevenpay-v2.withdraw";

	public FiveSevenPayV2Processor(JsonTransform jsonTransform)
			: base(jsonTransform)
	{
	}
}
