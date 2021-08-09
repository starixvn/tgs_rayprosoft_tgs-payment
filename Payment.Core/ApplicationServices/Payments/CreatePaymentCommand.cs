using System.Text.Json;

namespace Tgs.Payment.ApplicationServices.Payments
{
	public record CreatePaymentCommand(
		string Code,
		string? Description,
		string? ExtendedData);
}
