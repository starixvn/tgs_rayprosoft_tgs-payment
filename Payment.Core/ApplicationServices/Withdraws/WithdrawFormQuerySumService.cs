using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws
{
	internal class WithdrawFormQuerySumService
		: IGetService<WithdrawFormQuery, decimal>
	{
		private readonly IPaymentRepository _paymentRepository;

		public WithdrawFormQuerySumService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public ValueTask<decimal> GetAsync(WithdrawFormQuery query, CancellationToken cancellationToken = default)
			=> _paymentRepository.TotalWithdrawFormsAsync(
				new WithdrawFormQueryConditions(
						query.BrandId,
						query.UserId,
						query.UserName,
						query.UserType,
						query.ChannelName,
						query.ThirdPartyOrderNo,
						query.FormId,
						query.PaymentMethodId,
						query.Status,
						query.CreateOnRange,
						query.UpdateOnRange));
	}
}
