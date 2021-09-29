using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits
{
	internal class DepositFormQueryService
		: IQueryService<DepositFormQuery, DepositFormInfo>, IGetService<DepositFormQuery, int>
	{
		private readonly IPaymentRepository _paymentRepository;

		public DepositFormQueryService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public ValueTask<int> GetAsync(DepositFormQuery query, CancellationToken cancellationToken = default)
			=> _paymentRepository.CountDepositFormsAsync(new DepositFormQueryConditions(
				query.BrandId,
				query.UserName,
				query.ChannelName,
				query.ThirdPartyOrderNo,
				query.PaymentMethodId,
				query.Status,
				query.CreateOnRange,
				query.UpdateOnRange));

		public IAsyncEnumerable<DepositFormInfo> QueryAsync(DepositFormQuery query, CancellationToken cancellationToken = default)
			=> query.Paging == null
				? _paymentRepository.QueryDepositFormsAsync(
					new DepositFormQueryConditions(
						query.BrandId,
						query.UserName,
						query.ChannelName,
						query.ThirdPartyOrderNo,
						query.PaymentMethodId,
						query.Status,
						query.CreateOnRange,
						query.UpdateOnRange))
				: _paymentRepository.QueryDepositFormsAsync(
					new DepositFormQueryConditions(
						query.BrandId,
						query.UserName,
						query.ChannelName,
						query.ThirdPartyOrderNo,
						query.PaymentMethodId,
						query.Status,
						query.CreateOnRange,
						query.UpdateOnRange),
					query.Paging);
	}
}