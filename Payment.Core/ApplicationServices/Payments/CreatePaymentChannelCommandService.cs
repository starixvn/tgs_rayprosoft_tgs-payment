using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments;

internal class CreatePaymentChannelCommandService : ICommandService<CreatePaymentChannelCommand>
{
	private readonly IPaymentClient _paymentClient;
	private readonly IPaymentRepository _paymentRepository;

	public CreatePaymentChannelCommandService(IPaymentRepository paymentRepository, IPaymentClient paymentClient)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
	}

	public async ValueTask ExecuteAsync(
		CreatePaymentChannelCommand command,
		CancellationToken cancellationToken = default)
	{
		await _paymentRepository
			.CreatePaymentChannelAsync(command.ChannelCode, cancellationToken)
			.ConfigureAwait(false);

		await _paymentClient
			.EmitPaymentChannelCreatedEventAsync(command.ChannelCode, cancellationToken)
			.ConfigureAwait(false);
	}
}
