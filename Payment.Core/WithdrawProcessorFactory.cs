using System;
using System.Collections.Generic;

namespace Tgs.Payment;

internal class WithdrawProcessorFactory(IDictionary<string, Type> processTypes, IServiceProvider serviceProvider) : IWithdrawProcessorFactory
{
	public IWithdrawProcessor GetWithdrawProcessor(string paymentCode)
		=> processTypes.TryGetValue(paymentCode, out var processorType)
			? (IWithdrawProcessor)serviceProvider.GetService(processorType!)!
			: throw new WithdrawProcessorNotFoundException();
}
