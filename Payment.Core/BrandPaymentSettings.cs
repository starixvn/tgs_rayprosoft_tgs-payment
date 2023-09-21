namespace Tgs.Payment;
public record BrandPaymentSettings(int? PlayerBankInfoCountLimit)
{
	internal static readonly BrandPaymentSettings _Default = new(default(int?));
}
