using System;
using System.Collections.Generic;

namespace Tgs.Payment;

internal class WithdrawProcessorFactory : IWithdrawProcessorFactory
{
	private readonly IDictionary<string, Type> _processTypes;
	private readonly IServiceProvider _serviceProvider;

	public WithdrawProcessorFactory(IDictionary<string, Type> processTypes, IServiceProvider serviceProvider)
	{
		_processTypes = processTypes ?? throw new ArgumentNullException(nameof(processTypes));
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
	}

	public IWithdrawProcessor GetWithdrawProcessor(string paymentCode)
	{
		return _processTypes.TryGetValue(paymentCode, out var processorType)
			? (IWithdrawProcessor)_serviceProvider.GetService(processorType!)!
			: throw new WithdrawProcessorNotFoundException();
	}
}
