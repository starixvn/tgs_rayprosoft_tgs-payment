using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Settings;

internal class UpsertBrandPaymentSettingsCommandService : ICommandService<UpsertBrandPaymentSettingsCommand>
{
	private readonly IPaymentRepository _paymentRepository;

	public UpsertBrandPaymentSettingsCommandService(IPaymentRepository paymentRepository)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
	}

	public ValueTask ExecuteAsync(
		UpsertBrandPaymentSettingsCommand command,
		CancellationToken cancellationToken = default)
		=> _paymentRepository.UpsertBrandPaymentSettingsAsync(
			command.BrandId,
			command.ModifyData,
			cancellationToken);
}
