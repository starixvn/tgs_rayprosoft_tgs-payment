using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos;

internal class GetAgentBankInfosQueryService(IPaymentRepository paymentRepository) : IQueryService<GetAgentBankInfosQuery, AgentBankInfo>
{
	public IAsyncEnumerable<AgentBankInfo> QueryAsync(
		GetAgentBankInfosQuery query,
		CancellationToken cancellationToken = default)
		=> paymentRepository.GetAgentBankInfosByAgentIdAsync(query.AgentId, cancellationToken);
}
