using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.Settings;

internal class UpsertBrandPaymentSettingsCommandService(IPaymentRepository paymentRepository) : ICommandService<UpsertBrandPaymentSettingsCommand>
{
	public ValueTask ExecuteAsync(
		UpsertBrandPaymentSettingsCommand command,
		CancellationToken cancellationToken = default)
		=> paymentRepository.UpsertBrandPaymentSettingsAsync(
			command.BrandId,
			command.ModifyData,
			cancellationToken);
}
