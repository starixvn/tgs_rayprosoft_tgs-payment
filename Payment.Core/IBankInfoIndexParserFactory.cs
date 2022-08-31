namespace Tgs.Payment;

public interface IBankInfoIndexParserFactory
{
	string GetBankInfoTransformation(string bankType);
}
