namespace Tgs.Payment;

internal class BankInfoIndexParserFactory : IBankInfoIndexParserFactory
{
	public string GetBankInfoTransformation(string bankType)
	{
		return "{}";
	}
}
