using JSLTSharp;

namespace Tgs.Payment.WithdrawProcessors;

internal class WinPayProcessor : PhpGoPayProcessor
{
	public override string Subject => "tgs.payment.winpay.withdraw";

	public WinPayProcessor(JsonTransform jsonTransform)
			: base(jsonTransform)
	{
	}
}
