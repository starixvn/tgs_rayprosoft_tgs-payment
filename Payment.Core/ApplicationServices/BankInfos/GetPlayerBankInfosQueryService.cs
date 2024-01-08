using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos;

internal class GetPlayerBankInfosQueryService(IPaymentRepository paymentRepository) : IQueryService<GetPlayerBankInfosQuery, PlayerBankInfo>
{
	public IAsyncEnumerable<PlayerBankInfo> QueryAsync(GetPlayerBankInfosQuery query, CancellationToken cancellationToken = default)
		=> paymentRepository.GetBankInfosByPlayerCodeAsync(query.PlayerCode, cancellationToken);
}
