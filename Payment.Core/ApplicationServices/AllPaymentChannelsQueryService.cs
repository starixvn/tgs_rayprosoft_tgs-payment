using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices
{
	internal class AllPaymentChannelsQueryService : IGetService<AllPaymentChannelsQuery, IDictionary<Guid, PaymentChannelInfo>>
	{
		private readonly IPaymentRepository _payamentRepository;

		public AllPaymentChannelsQueryService(IPaymentRepository paymentRepository)
		{
			_payamentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public async ValueTask<IDictionary<Guid, PaymentChannelInfo>> GetAsync(AllPaymentChannelsQuery query, CancellationToken cancellationToken = default)
		{
			var allPaymentChannels = await _payamentRepository.GetAllPaymentChannelsAsync()
				.ToDictionaryAsync(method => method.Id)
				.ConfigureAwait(false);

			return new ReadOnlyDictionary<Guid, PaymentChannelInfo>(allPaymentChannels);
		}
	}
}
