using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits
{
	internal class DepositFormQuerySumService
		: IGetService<DepositFormQuery, decimal>
	{
		private readonly IPaymentRepository _paymentRepository;

		public DepositFormQuerySumService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public ValueTask<decimal> GetAsync(DepositFormQuery query, CancellationToken cancellationToken = default)
			=> _paymentRepository.TotalDepositFormsAsync(new DepositFormQueryConditions(
				query.BrandId,
				query.UserName,
				query.ChannelName,
				query.ThirdPartyOrderNo,
				query.FormId,
				query.PaymentMethodId,
				query.Status,
				query.CreateOnRange,
				query.UpdateOnRange));
	}
}
