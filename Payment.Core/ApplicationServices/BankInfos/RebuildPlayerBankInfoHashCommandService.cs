using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JSLTSharp;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos;

internal class RebuildPlayerBankInfoHashCommandService : ICommandService<RebuildPlayerBankInfoHashCommand>
{
	private readonly IBankInfoIndexParserFactory _bankInfoParserFactory;
	private readonly JsonTransform _jsonTransform;
	private readonly IPaymentRepository _paymentRepository;

	public RebuildPlayerBankInfoHashCommandService(
		IPaymentRepository paymentRepository,
		IBankInfoIndexParserFactory bankInfoParserFactory,
		JsonTransform jsonTransform)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_bankInfoParserFactory = bankInfoParserFactory ?? throw new ArgumentNullException(nameof(bankInfoParserFactory));
		_jsonTransform = jsonTransform ?? throw new ArgumentNullException(nameof(jsonTransform));
	}

	public async ValueTask ExecuteAsync(RebuildPlayerBankInfoHashCommand command, CancellationToken cancellationToken = default)
	{
		var bankInfo = await _paymentRepository.GetBankInfoByIdAsync(
			command.BankInfoId,
			cancellationToken).ConfigureAwait(false);

		if (bankInfo is null)
			return;

		var transformation = _bankInfoParserFactory.GetBankInfoTransformation(bankInfo.BankType);
		var bankInfoDetail = JsonSerializer.Serialize(bankInfo.Detail)!;

		var feature = JsonSerializer.Serialize(JsonDocument.Parse(
			  _jsonTransform.Transform(bankInfoDetail, transformation)));

		var hash = feature is "{}"
			? null
			: SHA256.HashData(Encoding.UTF8.GetBytes(feature));

		await _paymentRepository.UpdatePlayerBankInfoHashAsync(
			bankInfo.Id,
			hash,
			cancellationToken).ConfigureAwait(false);
	}
}
