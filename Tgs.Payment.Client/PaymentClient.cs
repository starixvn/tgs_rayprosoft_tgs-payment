using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.Clients;

internal class PaymentClient : IPaymentClient
{
	private readonly ILogger<PaymentClient> _logger;
	private readonly IMessageSender _messageSender;

	public PaymentClient(IMessageSender messageSender, ILogger<PaymentClient> logger)
	{
		_messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public ValueTask ApproveDepositAsync(Guid formId, Guid brandId, decimal? actualAmount, CancellationToken cancellationToken = default)
	{
		var request = new ApproveDepositRequest
		{
			FormId = formId.ToString(),
			BrandId = brandId.ToString(),
			ActualAmount = actualAmount
		};

		return _messageSender.SendAsync(
			"tgs.payment.deposit.approve",
			new ReadOnlyMemory<byte>(request.ToByteArray()),
			cancellationToken: cancellationToken);
	}

	public async ValueTask<string> AskAsync(string subject, string requestInfo, CancellationToken cancellationToken = default)
	{
		var request = new GeneralRequest
		{
			Content = requestInfo
		};

		var result = await _messageSender.RequestAsync<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>(
			subject,
			request.ToByteArray(),
			cancellationToken: cancellationToken).ConfigureAwait(false);

		var response = GeneralResponse.Parser.ParseFrom(result.Span);

		_logger.LogInformation("{response.Result}", response.Result);

		return response.Result;
	}

	public async ValueTask<string> DepositAsync(string subject, decimal amount, string? applyInfo, CancellationToken cancellationToken = default)
	{
		var request = new DepositRequest
		{
			ApplyInfo = applyInfo ?? string.Empty,
			Amount = amount.ToString()
		};

		var result = await _messageSender.RequestAsync<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>(
			subject,
			request.ToByteArray(),
			cancellationToken: cancellationToken).ConfigureAwait(false);

		var response = DepositResponse.Parser.ParseFrom(result.Span);

		_logger.LogInformation("{response.Result}", response.Result);

		return response.Result;
	}

	public ValueTask EmitDepositFormCreatedEventAsync(Guid formId, Guid brandId, CancellationToken cancellationToken = default)
	{
		var request = new FormEvent
		{
			FormId = formId.ToString(),
			BrandId = brandId.ToString()
		};

		return _messageSender.PublishAsync(
			"tgs.payment.deposit.created",
			new ReadOnlyMemory<byte>(request.ToByteArray()),
			cancellationToken: cancellationToken);
	}

	public async ValueTask EmitDepositFormStatusChangedEventAsync(Guid formId, Guid brandId, string? status, CancellationToken cancellationToken = default)
	{
		var request = new FormEvent
		{
			FormId = formId.ToString(),
			BrandId = brandId.ToString()
		};

		var requestEx = new FormEventEx
		{
			FormId = formId,
			BrandId = brandId,
			Status = status
		};

		await Task.WhenAll(
			_messageSender.PublishAsync(
				"tgs.payment.deposit.status",
				new ReadOnlyMemory<byte>(request.ToByteArray()),
				cancellationToken: cancellationToken).AsTask(),
			_messageSender.PublishAsync(
				"tgs.payment.deposit.status_ex",
				new ReadOnlyMemory<byte>(requestEx.ToByteArray()),
				cancellationToken: cancellationToken).AsTask()).ConfigureAwait(false);
	}

	public ValueTask EmitPaymentChannelCreatedEventAsync(string paymentChannelName, CancellationToken cancellationToken = default)
	{
		var request = new PaymentChannelChangedEvent
		{
			Name = paymentChannelName
		};

		return _messageSender.PublishAsync(
			"tgs.payment-channel.created",
			new ReadOnlyMemory<byte>(request.ToByteArray()),
			cancellationToken);
	}

	public ValueTask EmitPaymentCreatedEventAsync(string paymentCode, CancellationToken cancellationToken = default)
	{
		var request = new PaymentChangedEvent
		{
			Code = paymentCode
		};

		return _messageSender.PublishAsync(
			"tgs.payment.created",
			new ReadOnlyMemory<byte>(request.ToByteArray()),
			cancellationToken);
	}

	public ValueTask EmitPaymentMethodChannelChangedEventAsync(Guid pmcId, CancellationToken cancellationToken = default)
	{
		var request = new PaymentMethodChannelChangedEvent
		{
			Id = pmcId
		};

		return _messageSender.PublishAsync(
			"tgs.payment-method-channel.changed",
			new ReadOnlyMemory<byte>(request.ToByteArray()),
			cancellationToken);
	}

	public ValueTask EmitPaymentMethodCreatedEventAsync(string paymentMethodName, CancellationToken cancellationToken = default)
	{
		var request = new PaymentMethodChangedEvent
		{
			Name = paymentMethodName
		};

		return _messageSender.PublishAsync(
			"tgs.payment-method.created",
			new ReadOnlyMemory<byte>(request.ToByteArray()),
			cancellationToken);
	}

	public ValueTask EmitPlayerBankInfoAddedEventAsync(Guid brandId, Guid bankInfoId, CancellationToken cancellationToken = default)
	{
		var request = new PlayerBankInfoChangedEvent
		{
			Id = bankInfoId,
			BrandId = brandId
		};

		return _messageSender.PublishAsync(
			"tgs.player.bankinfo.added",
			new ReadOnlyMemory<byte>(request.ToByteArray()),
			cancellationToken);
	}

	public ValueTask EmitPlayerBankInfoDeletedEventAsync(Guid brandId, Guid bankInfoId, CancellationToken cancellationToken = default)
	{
		var request = new PlayerBankInfoChangedEvent
		{
			Id = bankInfoId,
			BrandId = brandId
		};

		return _messageSender.PublishAsync(
			"tgs.player.bankinfo.deleted",
			new ReadOnlyMemory<byte>(request.ToByteArray()),
			cancellationToken);
	}

	public ValueTask EmitWithdrawFormCreatedEventAsync(Guid formId, Guid brandId, CancellationToken cancellationToken = default)
	{
		var request = new FormEvent
		{
			FormId = formId.ToString(),
			BrandId = brandId.ToString()
		};

		return _messageSender.PublishAsync(
			"tgs.payment.withdraw.created",
			new ReadOnlyMemory<byte>(request.ToByteArray()),
			cancellationToken);
	}

	public async ValueTask EmitWithdrawFormStatusChangedEventAsync(Guid formId, Guid brandId, string? status, CancellationToken cancellationToken = default)
	{
		var request = new FormEvent
		{
			FormId = formId.ToString(),
			BrandId = brandId.ToString()
		};

		var requestEx = new FormEventEx
		{
			FormId = formId,
			BrandId = brandId,
			Status = status ?? string.Empty
		};

		await Task.WhenAll(
			_messageSender.PublishAsync(
				"tgs.payment.withdraw.status",
				new ReadOnlyMemory<byte>(request.ToByteArray()),
				cancellationToken: cancellationToken).AsTask(),
			_messageSender.PublishAsync(
				"tgs.payment.withdraw.status_ex",
				new ReadOnlyMemory<byte>(requestEx.ToByteArray()),
				cancellationToken: cancellationToken).AsTask())
			.ConfigureAwait(false);
	}

	public ValueTask PaidWithdrawAsync(Guid formId, Guid brandId, CancellationToken cancellationToken = default)
	{
		var request = new PaidWithdrawRequest
		{
			FormId = formId,
			BrandId = brandId
		};

		return _messageSender.SendAsync(
			"tgs.payment.withdraw.paid_ex",
			new ReadOnlyMemory<byte>(request.ToByteArray()),
			cancellationToken);
	}

	public ValueTask RebuildPlayerBankInfoHashAsync(Guid brandId, Guid bankInfoId, CancellationToken cancellationToken = default)
	{
		var request = new PlayerBankInfoChangedEvent
		{
			BrandId = brandId,
			Id = bankInfoId
		};

		return _messageSender.SendAsync(
			"tgs.player.bankinfo.rebuild",
			new ReadOnlyMemory<byte>(request.ToByteArray()),
			cancellationToken);
	}

	public ValueTask RejectDepositAsync(Guid formId, Guid brandId, CancellationToken cancellationToken = default)
	{
		var request = new RejectDepositRequest
		{
			FormId = formId,
			BrandId = brandId
		};

		return _messageSender.SendAsync(
			"tgs.payment.deposit.reject",
			new ReadOnlyMemory<byte>(request.ToByteArray()),
			cancellationToken: cancellationToken);
	}

	public ValueTask RejectWithdrawAsync(Guid formId, Guid brandId, CancellationToken cancellationToken = default)
	{
		var request = new RejectWithdrawRequest
		{
			FormId = formId,
			BrandId = brandId
		};

		return _messageSender.SendAsync(
			"tgs.payment.withdraw.reject",
			new ReadOnlyMemory<byte>(request.ToByteArray()),
			cancellationToken: cancellationToken);
	}

	public ValueTask SaveWithdrawFormThirdPartyOrderNoAsync(Guid formId, string thirdPartyOrderNo, CancellationToken cancellationToken = default)
	{
		var request = new FormThirdNoRequest
		{
			FormId = formId,
			ThirdPartyOrderNo = thirdPartyOrderNo
		};

		return _messageSender.SendAsync(
			"tgs.payment.withdraw.external_id",
			new ReadOnlyMemory<byte>(request.ToByteArray()),
			cancellationToken: cancellationToken);
	}

	public ValueTask ScheduleAsync(string subject, Guid formId, DateTime scheduleTime, CancellationToken cancellationToken = default)
	{
		var request = new ScheduleRequest
		{
			Subject = subject,
			FormId = formId,
			ScheduleTime = Timestamp.FromDateTime(scheduleTime)
		};

		return _messageSender.SendAsync(
			"tgs.payment.countdown.schedule",
			new ReadOnlyMemory<byte>(request.ToByteArray()),
			cancellationToken);
	}

	public async ValueTask<string> WithdrawAsync(string subject, decimal amount, string? applyInfo, CancellationToken cancellationToken = default)
	{
		var request = new WithdrawRequest
		{
			ApplyInfo = applyInfo ?? string.Empty,
			Amount = amount.ToString()
		};

		var result = await _messageSender.RequestAsync<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>(
			subject,
			request.ToByteArray(),
			cancellationToken: cancellationToken).ConfigureAwait(false);

		var response = WithdrawResponse.Parser.ParseFrom(result.Span);

		_logger.LogInformation("{response.Result}", response.Result);

		return response.Result;
	}

	public async ValueTask<decimal?> GetMerchantBalanceAsync(string paymentCode, string brandPaymentExtendData, CancellationToken cancellationToken = default)
	{
		var subject = $"tgs.payment.dispatch_balance.{paymentCode.ToLower()}";

		var request = new PaymentBalanceRequest
		{
			BrandPaymentSettings = brandPaymentExtendData
		};

		var response = await _messageSender.RequestAsync<PaymentBalanceRequest, PaymentBalanceResponse>(
			subject,
			request,
			cancellationToken).ConfigureAwait(false);

		return (decimal?)response.Balance;
	}
}
