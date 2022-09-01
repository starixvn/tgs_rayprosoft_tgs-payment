using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JSLTSharp;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos;

internal class ProcessBankInfoTagsCommandService : ICommandService<ProcessBankInfoTagsCommand>
{
	private readonly IBankInfoIndexParserFactory _indexParserFactory;
	private readonly JsonTransform _jsonTransform;
	private readonly IPaymentRepository _paymentRepository;

	public ProcessBankInfoTagsCommandService(
		IPaymentRepository paymentRepository,
		IBankInfoIndexParserFactory indexParserFactory,
		JsonTransform jsonTransform)
	{
		_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
		_indexParserFactory = indexParserFactory ?? throw new ArgumentNullException(nameof(indexParserFactory));
		_jsonTransform = jsonTransform ?? throw new ArgumentNullException(nameof(jsonTransform));
	}

	public async ValueTask ExecuteAsync(ProcessBankInfoTagsCommand command, CancellationToken cancellationToken = default)
	{
		var bankInfo = await _paymentRepository
			.GetBankInfoByIdAsync(command.BankInfoId, cancellationToken)
			.ConfigureAwait(false);

		var transformation = _indexParserFactory.GetBankInfoTransformation(bankInfo!.BankType);
		var bankInfoDetail = JsonSerializer.Serialize(bankInfo.Detail);

		var tranformResult = _jsonTransform.Transform(bankInfoDetail, transformation);

		var dict = JsonSerializer.Deserialize<Dictionary<string, string?>>(tranformResult)!;

		await _paymentRepository
			.RemoveBankInfoTagsAsync(bankInfo.BrandId, bankInfo.Id, cancellationToken)
			.ConfigureAwait(false);

		foreach (var p in dict.Where(p => p.Value is not null))
			await _paymentRepository.SetBankInfoTagAsync(
				bankInfo.BrandId,
				bankInfo.Id,
				p.Key,
				bankInfo.PlayerCode,
				p.Value!,
				cancellationToken).ConfigureAwait(false);
	}
}
