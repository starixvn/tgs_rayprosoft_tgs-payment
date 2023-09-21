namespace Tgs.Payment.ViewModels;

public class BrandPaymentSettingsViewModel
{
	internal static readonly BrandPaymentSettingsViewModel _Default = new()
	{
	};

	public int? PlayerBankInfoCountLimit { get; set; }
}
