using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws
{
	internal class WithdrawFormQueryService
		: IQueryService<WithdrawFormQuery, WithdrawFormInfo>, IGetService<WithdrawFormQuery, int>
	{
		private readonly IPaymentRepository _paymentRepository;

		public WithdrawFormQueryService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public ValueTask<int> GetAsync(WithdrawFormQuery query, CancellationToken cancellationToken = default)
			=> _paymentRepository.CountWithdrawFormsAsync(
				new WithdrawFormQueryConditions(
						query.BrandId,
						query.UserName,
						query.ChannelName,
						query.ThirdPartyOrderNo,
						query.Id,
						query.PaymentMethodId,
						query.Status,
						query.CreateOnRange,
						query.UpdateOnRange));

		public IAsyncEnumerable<WithdrawFormInfo> QueryAsync(WithdrawFormQuery query, CancellationToken cancellationToken = default)
			=> query.Paging == null
				? _paymentRepository.QueryWithdrawFormsAsync(
					new WithdrawFormQueryConditions(
						query.BrandId,
						query.UserName,
						query.ChannelName,
						query.ThirdPartyOrderNo,
						query.Id,
						query.PaymentMethodId,
						query.Status,
						query.CreateOnRange,
						query.UpdateOnRange))
				: _paymentRepository.QueryWithdrawFormsAsync(
					new WithdrawFormQueryConditions(
						query.BrandId,
						query.UserName,
						query.ChannelName,
						query.ThirdPartyOrderNo,
						query.Id,
						query.PaymentMethodId,
						query.Status,
						query.CreateOnRange,
						query.UpdateOnRange),
					query.Paging);
	}
}