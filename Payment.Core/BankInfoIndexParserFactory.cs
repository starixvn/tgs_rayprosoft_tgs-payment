using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Tgs.Payment;

internal class BankInfoIndexParserFactory(IOptions<BankInfoParserOptions> optionsAccessor) : IBankInfoIndexParserFactory
{
	private readonly ImmutableDictionary<string, string> _bankTypes = optionsAccessor.Value.ToImmutableDictionary(
		p => p.Key,
		p => JsonSerializer.Serialize(p.Value));

	public string GetBankInfoTransformation(string bankType)
		=> _bankTypes.TryGetValue(bankType, out var result) ? result : "{}";
}
