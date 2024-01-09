using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JSLTSharp;
using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BankInfos;

internal class ProcessBankInfoTagsCommandService(
	IPaymentRepository paymentRepository,
	IBankInfoIndexParserFactory indexParserFactory,
	JsonTransform jsonTransform,
	ILogger<ProcessBankInfoTagsCommandService> logger) : ICommandService<ProcessBankInfoTagsCommand>
{
	public async ValueTask ExecuteAsync(ProcessBankInfoTagsCommand command, CancellationToken cancellationToken = default)
	{
		var bankInfo = await paymentRepository
			.GetBankInfoByIdAsync(command.BankInfoId, cancellationToken)
			.ConfigureAwait(false);

		if (bankInfo is null)
			return;

		var transformation = indexParserFactory.GetBankInfoTransformation(bankInfo.BankType);
		var bankInfoDetail = JsonSerializer.Serialize(bankInfo.Detail);

		var transformResult = jsonTransform.Transform(bankInfoDetail, transformation);

		logger.LogInformation("TransformResult: {transformResult}", transformResult);

		var dict = JsonSerializer.Deserialize<Dictionary<string, string?>>(transformResult)!;

		await paymentRepository
			.RemoveBankInfoTagsAsync(bankInfo.BrandId, bankInfo.Id, cancellationToken)
			.ConfigureAwait(false);

		foreach (var p in dict.Where(p => p.Value is not null))
			await paymentRepository.SetBankInfoTagAsync(
				bankInfo.BrandId,
				bankInfo.Id,
				p.Key,
				bankInfo.PlayerCode,
				p.Value!,
				cancellationToken).ConfigureAwait(false);
	}
}
