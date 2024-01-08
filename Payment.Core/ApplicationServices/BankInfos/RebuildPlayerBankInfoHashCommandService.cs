using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JSLTSharp;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos;

internal class RebuildPlayerBankInfoHashCommandService(
	IPaymentRepository paymentRepository,
	IBankInfoIndexParserFactory bankInfoParserFactory,
	JsonTransform jsonTransform)
	: ICommandService<RebuildPlayerBankInfoHashCommand>
{
	public async ValueTask ExecuteAsync(RebuildPlayerBankInfoHashCommand command, CancellationToken cancellationToken = default)
	{
		var bankInfo = await paymentRepository.GetBankInfoByIdAsync(
			command.BankInfoId,
			cancellationToken).ConfigureAwait(false);

		if (bankInfo is null)
			return;

		var transformation = bankInfoParserFactory.GetBankInfoTransformation(bankInfo.BankType);
		var bankInfoDetail = JsonSerializer.Serialize(bankInfo.Detail)!;

		var feature = JsonSerializer.Serialize(JsonDocument.Parse(
			  jsonTransform.Transform(bankInfoDetail, transformation)));

		var hash = feature is "{}"
			? null
			: SHA256.HashData(Encoding.UTF8.GetBytes(feature));

		await paymentRepository.UpdatePlayerBankInfoHashAsync(
			bankInfo.Id,
			hash,
			cancellationToken).ConfigureAwait(false);
	}
}
