using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Deposits;

internal class SuccessRateQueryService : IGetService<SuccessRateQuery, decimal>
{
	private readonly IPaymentRepository _paymentRepository;

	public SuccessRateQueryService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public async ValueTask<decimal> GetAsync(SuccessRateQuery query, CancellationToken cancellationToken = default)
	{
		var now = DateTimeOffset.UtcNow;
		var today = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.FromHours(8d));

		var conditions = new DepositFormQueryConditions(
			BrandId: query.BrandId,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			Status: Enum.GetValues<PaymentFlowState>().AsEnumerable(),
			CreateOnRange: (today, today.AddDays(1d)),
			null);

		var allDepositForms = await _paymentRepository
			.QueryDepositFormsAsync(conditions, new Paging.Paging<DepositFormInfo> { Page = 1, RowCount = 1000 }, cancellationToken)
			.Select(d => new { d.Status, d.Id })
			.ToArrayAsync(cancellationToken);

		if (!allDepositForms.Any())
			return 0m;

		var approved = allDepositForms.Where(d => d.Status == PaymentFlowState.Approved).Count();

		return Math.Round((decimal)approved / allDepositForms.Length * 100, MidpointRounding.AwayFromZero);
	}
}
