using System;
using System.Collections.Generic;
using System.Threading;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos
{
	internal class GetPlayerBankInfosQueryService : IQueryService<GetPlayerBankInfosQuery, PlayerBankInfo>
    {
        private readonly IPaymentRepository _paymentRepository;

        public GetPlayerBankInfosQueryService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        }

        public IAsyncEnumerable<PlayerBankInfo> QueryAsync(GetPlayerBankInfosQuery query, CancellationToken cancellationToken = default)
            => _paymentRepository.GetBankInfosByPlayerCodeAsync(query.PlayerCode);
    }
}