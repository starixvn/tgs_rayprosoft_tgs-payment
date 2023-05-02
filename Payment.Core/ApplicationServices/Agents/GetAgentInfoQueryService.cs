using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Agents;

internal class GetAgentInfoQueryService : IGetService<GetAgentInfoQuery, AgentInfo>
{
	private readonly IPaymentRepository _repository;

	public GetAgentInfoQueryService(IPaymentRepository repository)
	{
		_repository = repository ?? throw new ArgumentNullException(nameof(repository));
	}

	public ValueTask<AgentInfo> GetAsync(GetAgentInfoQuery query, CancellationToken cancellationToken = default)
	=> _repository.GetAgentInfoByAgentIdAsync(query.AgentId);
}
