using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Tgs.Payment;

internal class BankInfoIndexParserFactory : IBankInfoIndexParserFactory
{
	private readonly IImmutableDictionary<string, string> _bankTypes;

	public BankInfoIndexParserFactory(IOptions<BankInfoParserOptions> optionsAccessor)
	{
		_bankTypes = optionsAccessor.Value.ToImmutableDictionary(
			p => p.Key,
			p => JsonSerializer.Serialize(p.Value));
	}

	public string GetBankInfoTransformation(string bankType)
		=> _bankTypes.TryGetValue(bankType, out var result) ? result : "{}";
}
