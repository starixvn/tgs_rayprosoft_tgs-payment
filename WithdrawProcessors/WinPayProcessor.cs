using JSLTSharp;
using Microsoft.Extensions.Options;

namespace Tgs.Payment.WithdrawProcessors;

internal class WinPayProcessor : PhpGoPayProcessor
{
	public override string Subject => "tgs.payment.winpay.withdraw";

	public WinPayProcessor(IOptions<PaymentOptions> jsonOptionsAccessor, JsonTransform jsonTransform)
			: base(jsonOptionsAccessor, jsonTransform)
	{
	}
}
