using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos
{
	internal class GetAgentBankInfosQueryService : IQueryService<GetAgentBankInfosQuery, AgentBankInfo>
    {
        private readonly IPaymentRepository _paymentRepository;

        public GetAgentBankInfosQueryService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        }

        public IAsyncEnumerable<AgentBankInfo> QueryAsync(
            GetAgentBankInfosQuery query,
            CancellationToken cancellationToken = default)
            => _paymentRepository.GetAgentBankInfosByAgentIdAsync(query.AgentId);
    }
}