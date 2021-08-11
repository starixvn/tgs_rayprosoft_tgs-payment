using System;
using System.Threading;
using System.Threading.Tasks;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Payments
{
	internal class CreatePaymentChannelCommandService : ICommandService<CreatePaymentChannelCommand>
	{
		private readonly IPaymentRepository _paymentRepository;

		public CreatePaymentChannelCommandService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		}

		public ValueTask ExecuteAsync(
			CreatePaymentChannelCommand command,
			CancellationToken cancellationToken = default)
			=> _paymentRepository.CreatePaymentChannelAsync(command.ChannelCode);
	}
}
