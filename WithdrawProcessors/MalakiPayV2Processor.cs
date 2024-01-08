using JSLTSharp;

namespace Tgs.Payment.WithdrawProcessors;

internal class MalakiPayV2Processor(JsonTransform jsonTransform) : PhpGoPayProcessor(jsonTransform)
{
	public override string Subject => "tgs.payment.malakipay-v2.withdraw";
}
