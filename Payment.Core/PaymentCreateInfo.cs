namespace Tgs.Payment
{
	public record PaymentCreateInfo(
		string Code,
		string? Description,
		string? ExtendedData);
}
