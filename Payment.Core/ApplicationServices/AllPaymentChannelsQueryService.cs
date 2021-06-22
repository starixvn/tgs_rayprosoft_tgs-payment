using System;
using System.Collections.Generic;
using System.Threading;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices
{
	internal class AllPaymentChannelsQueryService : IQueryService<AllPaymentChannelsQuery, PaymentChannelInfo>
	{
		private readonly IPaymentRepository _payamentRepository;

		public AllPaymentChannelsQueryService(IPaymentRepository paymentRepository)
		{
			_payamentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public IAsyncEnumerable<PaymentChannelInfo> QueryAsync(
			AllPaymentChannelsQuery query,
			CancellationToken cancellationToken = default)
			=> _payamentRepository.GetAllPaymentChannelsAsync();
	}
}
