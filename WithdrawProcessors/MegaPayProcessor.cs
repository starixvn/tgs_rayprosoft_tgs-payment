using JSLTSharp;

namespace Tgs.Payment.WithdrawProcessors;

internal class MegaPayProcessor(JsonTransform jsonTransform) : AlfaPayProcessor(jsonTransform)
{
	public override string Subject => "tgs.payment.megapay.withdraw";
}