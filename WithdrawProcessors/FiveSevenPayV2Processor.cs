using JSLTSharp;
using Microsoft.Extensions.Options;

namespace Tgs.Payment.WithdrawProcessors;

internal class FiveSevenPayV2Processor : PhpGoPayProcessor
{
	public override string Subject => "tgs.payment.fivesevenpay-v2.withdraw";

	public FiveSevenPayV2Processor(IOptions<PaymentOptions> jsonOptionsAccessor, JsonTransform jsonTransform)
			: base(jsonOptionsAccessor, jsonTransform)
	{
	}
}
