using JSLTSharp;

namespace Tgs.Payment.WithdrawProcessors;

internal class MalakiPayV2Processor : PhpGoPayProcessor
{
	public override string Subject => "tgs.payment.malakipay-v2.withdraw";

	public MalakiPayV2Processor(JsonTransform jsonTransform)
			: base(jsonTransform)
	{
	}
}
