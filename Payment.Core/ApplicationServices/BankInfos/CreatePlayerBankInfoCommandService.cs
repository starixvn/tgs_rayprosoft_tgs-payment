using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JSLTSharp;
using Tgs.Payment.ApplicationServices.Settings;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos;

internal class CreatePlayerBankInfoCommandService(
	IPaymentRepository paymentRepository,
	IPaymentClient paymentClient,
	IBankInfoIndexParserFactory bankInfoParserFactory,
	JsonTransform jsonTransform,
	IServiceCoordinator serviceCoordinator)
	: IGetService<CreatePlayerBankInfoCommand, Guid>
{
	public async ValueTask<Guid> GetAsync(CreatePlayerBankInfoCommand command, CancellationToken cancellationToken = default)
	{
		var transformation = bankInfoParserFactory.GetBankInfoTransformation(command.BankType);
		var bankInfoDetail = JsonSerializer.Serialize(command.Detail)!;

		var feature = JsonSerializer.Serialize(JsonDocument.Parse(
			  jsonTransform.Transform(bankInfoDetail, transformation)));

		var hash = feature is "{}"
			? null
			: SHA256.HashData(Encoding.UTF8.GetBytes(feature));

		var brandCheckIsEnabled = await serviceCoordinator.GetAsync<GetPlayerBankInfoDuplicateCheckIsEnabledQuery, bool>(
			new(command.BrandId),
			cancellationToken).ConfigureAwait(false);

		if (brandCheckIsEnabled
			&& hash is not null
			&& await paymentRepository.CheckPlayerBankInfoIsExistsAsync(
				command.BrandId,
				command.BankType,
				command.PaymentChannelId,
				hash,
				command.PlayerCode,
				cancellationToken).ConfigureAwait(false))
			throw new PlayerBankInfoDuplicateException();

		var paymentSettings = await paymentRepository.GetBrandPaymentSettingsAsync(
			command.BrandId,
			cancellationToken).ConfigureAwait(false);

		if (paymentSettings?.PlayerBankInfoCountLimit is not null)
		{
			var playerBankInfoCount = await paymentRepository.GetBankInfosByPlayerCodeAsync(
				command.PlayerCode,
				cancellationToken)
				.CountAsync(cancellationToken)
				.ConfigureAwait(false);

			if (playerBankInfoCount >= paymentSettings.PlayerBankInfoCountLimit.Value)
				throw new PlayerBankInfoCountReachedLimitException();
		}

		var bankInfo = new PlayerBankCreateInfo(
			Guid.NewGuid(),
			command.PlayerCode,
			command.BrandId,
			command.PaymentChannelId,
			command.BankType,
			command.Detail,
			command.IsDefault,
			hash);

		await paymentRepository
			.CreateBankInfoAsync(bankInfo, command.ModifyUser, cancellationToken)
			.ConfigureAwait(false);

		await paymentClient
			.EmitPlayerBankInfoAddedEventAsync(command.BrandId, bankInfo.Id, cancellationToken)
			.ConfigureAwait(false);

		return bankInfo.Id;
	}
}
