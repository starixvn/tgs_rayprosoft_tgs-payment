using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices
{
	internal class AllPaymentMethodsQueryService : IGetService<AllPaymentMethodsQuery, IDictionary<Guid, PaymentMethodInfo>>
	{
		private readonly IPaymentRepository _paymentRepository;

		public AllPaymentMethodsQueryService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public async ValueTask<IDictionary<Guid, PaymentMethodInfo>> GetAsync(AllPaymentMethodsQuery query, CancellationToken cancellationToken = default)
		{
			var allPaymentMethods = await _paymentRepository.GetAllPaymentMethodsAsync()
				.ToDictionaryAsync(method => method.Id)
				.ConfigureAwait(false);

			return new ReadOnlyDictionary<Guid, PaymentMethodInfo>(allPaymentMethods);
		}
	}
}
