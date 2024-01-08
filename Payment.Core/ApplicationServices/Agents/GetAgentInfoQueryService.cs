using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Agents;

internal class GetAgentInfoQueryService(IPaymentRepository repository) : IGetService<GetAgentInfoQuery, AgentInfo>
{
	public ValueTask<AgentInfo> GetAsync(GetAgentInfoQuery query, CancellationToken cancellationToken = default)
		=> repository.GetAgentInfoByAgentIdAsync(query.AgentId, cancellationToken);
}
