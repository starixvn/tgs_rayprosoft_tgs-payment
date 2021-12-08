using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

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
				query.UserId,
				query.UserName,
				query.ChannelName,
				query.ThirdPartyOrderNo,
				query.OrderNo,
				query.FormId,
				query.PaymentMethodId,
				query.Status,
				query.CreateOnRange,
				query.UpdateOnRange));

		public IAsyncEnumerable<DepositFormInfo> QueryAsync(DepositFormQuery query, CancellationToken cancellationToken = default)
			=> _paymentRepository.QueryDepositFormsAsync(
				new DepositFormQueryConditions(
					query.BrandId,
					query.UserId,
					query.UserName,
					query.ChannelName,
					query.ThirdPartyOrderNo,
					query.OrderNo,
					query.FormId,
					query.PaymentMethodId,
					query.Status,
					query.CreateOnRange,
					query.UpdateOnRange),
				query.Paging);
	}
}
