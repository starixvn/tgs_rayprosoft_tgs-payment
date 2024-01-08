using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos;

internal class GetPlayersByBankInfoTagQueryService(IPaymentRepository paymentRepository) : IQueryService<GetPlayersByBankInfoTagQuery, string>
{
	public IAsyncEnumerable<string> QueryAsync(GetPlayersByBankInfoTagQuery query, CancellationToken cancellationToken = default)
		=> paymentRepository.GetPlayerCodesByBankInfoTagAsync(
			query.BrandId,
			query.TagType,
			query.Tag,
			cancellationToken);
}
