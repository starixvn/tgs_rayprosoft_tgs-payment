using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using TGS.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Withdraws
{
	internal class SignOffWithdrawFormCommandService : ICommandService<SignOffWithdrawFormCommand>
    {
        private readonly IPaymentClient _paymentClient;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IWithdrawProcessorFactory _withdrawProcessorFactory;

        public SignOffWithdrawFormCommandService(
            IPaymentRepository paymentRepository,
            IWithdrawProcessorFactory withdrawProcessorFactory,
            IPaymentClient paymentClient)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _withdrawProcessorFactory = withdrawProcessorFactory ?? throw new ArgumentNullException(nameof(withdrawProcessorFactory));
            _paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
        }

        public async ValueTask ExecuteAsync(SignOffWithdrawFormCommand command, CancellationToken cancellationToken = default)
        {
            var formData = await _paymentRepository
                .GetWithdrawFormByIdAsync(command.FormId)
                .ConfigureAwait(false);

            var formStatus = (PaymentFlowState)formData.Status;

            if (formStatus != PaymentFlowState.WaitApprove)
                return;

            if (command.Status == ReviewState.Approve)
            {
                var brandPayment = await _paymentRepository
                    .GetBrandPaymentByIdAsync(command.BrandPaymentId!.Value)
                    .ConfigureAwait(false);

                var payment = await _paymentRepository
                    .GetPaymentAsync(brandPayment.PaymentId)
                    .ConfigureAwait(false);

                var processor = _withdrawProcessorFactory.GetWithdrawProcessor(payment.Code);

                var withdrawSettings = processor.MakeWithdrawSettings(
                    command.FormId,
                    formData.Amount,
                    formData.BankInfo,
                    brandPayment);

                var applyInfo = await _paymentClient
                    .WithdrawAsync(processor.Subject, formData.Amount, withdrawSettings)
                    .ConfigureAwait(false);

                await _paymentRepository
                    .SignOffWithdrawFormAsync(
                        command.FormId,
                        command.BrandPaymentId.Value,
                        applyInfo,
                        command.Status,
                        command.Remark)
                    .ConfigureAwait(false);
            }
            else
            {
                await _paymentRepository
                    .SignOffWithdrawFormAsync(
                        command.FormId,
                        null,
                        null,
                        command.Status,
                        command.Remark)
                    .ConfigureAwait(false);
            }

            await _paymentClient
                .EmitWithdrawFormStatusChangedEventAsync(formData.Id, formData.BrandId)
                .ConfigureAwait(false);
        }
    }
}