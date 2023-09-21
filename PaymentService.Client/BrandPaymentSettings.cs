namespace Tgs.Payment.Clients;
internal record BrandPaymentSettings(int? PlayerBankInfoCountLimit)
{
	internal static readonly BrandPaymentSettings _Default = new(default(int?));
}
