using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JSLTSharp;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos;

internal class CreatePlayerBankInfoCommandService : IGetService<CreatePlayerBankInfoCommand, Guid>
{
	private readonly IBankInfoIndexParserFactory _bankInfoParserFactory;
	private readonly JsonTransform _jsonTransform;
	private readonly IPaymentClient _paymentClient;
	private readonly IPaymentRepository _paymentRepository;

	public CreatePlayerBankInfoCommandService(
		IPaymentRepository paymentRepository,
		IPaymentClient paymentClient,
		IBankInfoIndexParserFactory bankInfoParserFactory,
		JsonTransform jsonTransform)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
		_bankInfoParserFactory = bankInfoParserFactory ?? throw new ArgumentNullException(nameof(bankInfoParserFactory));
		_jsonTransform = jsonTransform ?? throw new ArgumentNullException(nameof(jsonTransform));
	}

	public async ValueTask<Guid> GetAsync(CreatePlayerBankInfoCommand command, CancellationToken cancellationToken = default)
	{
		var transformation = _bankInfoParserFactory.GetBankInfoTransformation(command.BankType);
		var bankInfoDetail = JsonSerializer.Serialize(command.Detail)!;

		var feature = JsonSerializer.Serialize(JsonDocument.Parse(
			  _jsonTransform.Transform(bankInfoDetail, transformation)));

		var hash = feature is "{}"
			? null
			: SHA256.HashData(Encoding.UTF8.GetBytes(feature));

		//if (hash is not null
		//	&& await _paymentRepository.CheckPlayerBankInfoIsExistsAsync(
		//		command.BrandId,
		//		command.BankType,
		//		hash,
		//		cancellationToken).ConfigureAwait(false))
		//	throw new PlayerBankInfoDuplicateException();

		var bankInfo = new PlayerBankCreateInfo(
			Guid.NewGuid(),
			command.PlayerCode,
			command.BrandId,
			command.PaymentChannelId,
			command.BankType,
			command.Detail,
			command.IsDefault,
			hash);

		await _paymentRepository
			.CreateBankInfoAsync(bankInfo, command.ModifyUser, cancellationToken)
			.ConfigureAwait(false);

		await _paymentClient
			.EmitPlayerBankInfoAddedEventAsync(command.BrandId, bankInfo.Id, cancellationToken)
			.ConfigureAwait(false);

		return bankInfo.Id;
	}
}
