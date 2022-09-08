using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos;

internal class GetPlayersByBankInfoTagQueryService : IQueryService<GetPlayersByBankInfoTagQuery, string>
{
	private readonly IPaymentRepository _paymentRepository;

	public GetPlayersByBankInfoTagQueryService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public IAsyncEnumerable<string> QueryAsync(GetPlayersByBankInfoTagQuery query, CancellationToken cancellationToken = default)
		=> _paymentRepository.GetPlayerCodesByBankInfoTagAsync(
			query.BrandId,
			query.TagType,
			query.Tag,
			cancellationToken);
}
