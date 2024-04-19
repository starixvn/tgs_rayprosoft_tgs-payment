using JSLTSharp;

namespace Tgs.Payment.WithdrawProcessors;

internal class AlfaPayIdrProcessor(JsonTransform jsonTransform) : AlfaPayProcessor(jsonTransform)
{
	public override string Subject => "tgs.payment.alfapay-idr.withdraw";
}
