using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Tgs.MessageQueue;
using Tgs.Payment.Protos;

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

		return _messageSender
			.SendAsync("tgs.payment.deposit.approve", request.ToByteArray(), cancellationToken: cancellationToken);
	}

	public async ValueTask<string> AskAsync(string subject, string requestInfo, CancellationToken cancellationToken = default)
	{
		var request = new GeneralRequest
		{
			Content = requestInfo
		};

		var result = await _messageSender
			.RequestAsync(subject, request.ToByteArray(), cancellationToken: cancellationToken)
			.ConfigureAwait(false);

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

		var result = await _messageSender
			.RequestAsync(subject, request.ToByteArray(), cancellationToken: cancellationToken)
			.ConfigureAwait(false);

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

		return _messageSender.PublishAsync("tgs.payment.deposit.created", request.ToByteArray(), cancellationToken: cancellationToken);
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
			_messageSender.PublishAsync("tgs.payment.deposit.status", request.ToByteArray(), cancellationToken: cancellationToken).AsTask(),
			_messageSender.PublishAsync("tgs.payment.deposit.status_ex", requestEx.ToByteArray(), cancellationToken: cancellationToken).AsTask())
			.ConfigureAwait(false);
	}

	public ValueTask EmitPaymentCreatedEventAsync(string paymentCode, CancellationToken cancellationToken = default)
	{
		var request = new PaymentChangedEvent
		{
			Code = paymentCode
		};

		return _messageSender.PublishAsync("tgs.payment.created", request.ToByteArray(), cancellationToken);
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
			request.ToByteArray(),
			cancellationToken: cancellationToken);
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
			_messageSender.PublishAsync("tgs.payment.withdraw.status", request.ToByteArray(), cancellationToken: cancellationToken).AsTask(),
			_messageSender.PublishAsync("tgs.payment.withdraw.status_ex", requestEx.ToByteArray(), cancellationToken: cancellationToken).AsTask())
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
			request.ToByteArray(),
			cancellationToken: cancellationToken);
	}

	public ValueTask RejectDepositAsync(Guid formId, Guid brandId, CancellationToken cancellationToken = default)
	{
		var request = new RejectDepositRequest
		{
			FormId = formId,
			BrandId = brandId
		};

		return _messageSender
			.SendAsync("tgs.payment.deposit.reject", request.ToByteArray(), cancellationToken: cancellationToken);
	}

	public ValueTask RejectWithdrawAsync(Guid formId, Guid brandId, CancellationToken cancellationToken = default)
	{
		var request = new RejectWithdrawRequest
		{
			FormId = formId,
			BrandId = brandId
		};

		return _messageSender
			.SendAsync("tgs.payment.withdraw.reject", request.ToByteArray(), cancellationToken: cancellationToken);
	}

	public ValueTask SaveWithdrawFormThirdPartyOrderNoAsync(Guid formId, string thirdPartyOrderNo, CancellationToken cancellationToken = default)
	{
		var request = new FormThirdNoRequest
		{
			FormId = formId,
			ThirdPartyOrderNo = thirdPartyOrderNo
		};

		return _messageSender.SendAsync("tgs.payment.withdraw.external_id", request.ToByteArray(), cancellationToken: cancellationToken);
	}

	public ValueTask ScheduleAsync(string subject, Guid formId, DateTime scheduleTime, CancellationToken cancellationToken = default)
	{
		var request = new ScheduleRequest
		{
			Subject = subject,
			FormId = formId,
			ScheduleTime = Timestamp.FromDateTime(scheduleTime)
		};

		return _messageSender.SendAsync("tgs.payment.countdown.schedule", request.ToByteArray(), cancellationToken: cancellationToken);
	}

	public async ValueTask<string> WithdrawAsync(string subject, decimal amount, string? applyInfo, CancellationToken cancellationToken = default)
	{
		var request = new WithdrawRequest
		{
			ApplyInfo = applyInfo ?? string.Empty,
			Amount = amount.ToString()
		};

		var result = await _messageSender
			.RequestAsync(subject, request.ToByteArray(), cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		var response = WithdrawResponse.Parser.ParseFrom(result.Span);

		_logger.LogInformation("{response.Result}", response.Result);

		return response.Result;
	}
}
