using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.MessageQueue;

internal class PlayerBankInfoDeletedHandler : IMessageHandler
{
	private readonly IPaymentRepository _paymentRepository;

	public PlayerBankInfoDeletedHandler(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
	{
		var request = PlayerBankInfoChangedEvent.Parser.ParseFrom(data.Span);

		return _paymentRepository.RemoveBankInfoTagsAsync(request.BrandId, request.Id, cancellationToken);
	}
}
