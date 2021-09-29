using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Tgs.MessageQueue;
using Tgs.Payment.Protos;

namespace Tgs.Payment.Clients
{
	internal class PaymentClient : IPaymentClient
	{
		private readonly ILogger<PaymentClient> _logger;
		private readonly IMessageSender _messageSender;

		public PaymentClient(IMessageSender messageSender, ILogger<PaymentClient> logger)
		{
			_messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public ValueTask ApproveDepositAsync(Guid formId, Guid brandId)
		{
			var request = new ApproveDepositRequest
			{
				FormId = formId.ToString(),
				BrandId = brandId.ToString()
			};

			return _messageSender
				.SendAsync("tgs.payment.approve", request.ToByteArray());
		}

		public async ValueTask<string> AskAsync(string subject, string requestInfo)
		{
			var request = new GeneralRequest
			{
				Content = requestInfo
			};

			var result = await _messageSender
				.RequestAsync(subject, request.ToByteArray())
				.ConfigureAwait(false);

			var response = GeneralResponse.Parser.ParseFrom(result);

			_logger.LogInformation(response.Result);

			return response.Result;
		}

		public async ValueTask<string> DepositAsync(string subject, decimal amount, string? applyInfo)
		{
			var request = new DepositRequest
			{
				ApplyInfo = applyInfo ?? string.Empty,
				Amount = amount.ToString()
			};

			var result = await _messageSender
				.RequestAsync(subject, request.ToByteArray())
				.ConfigureAwait(false);

			var response = DepositResponse.Parser.ParseFrom(result);

			_logger.LogInformation(response.Result);

			return response.Result;
		}

		public ValueTask EmitDepositFormCreatedEventAsync(Guid formId, Guid brandId)
		{
			var request = new FormEvent
			{
				FormId = formId.ToString(),
				BrandId = brandId.ToString()
			};

			return _messageSender.PublishAsync("tgs.payment.deposit.created", request.ToByteArray());
		}

		public ValueTask EmitDepositFormStatusChangedEventAsync(Guid formId, Guid brandId)
		{
			var request = new FormEvent
			{
				FormId = formId.ToString(),
				BrandId = brandId.ToString()
			};

			return _messageSender.PublishAsync("tgs.payment.deposit.status", request.ToByteArray());
		}

		public ValueTask EmitWithdrawFormCreatedEventAsync(Guid formId, Guid brandId)
		{
			var request = new FormEvent
			{
				FormId = formId.ToString(),
				BrandId = brandId.ToString()
			};

			return _messageSender.PublishAsync("tgs.payment.withdraw.created", request.ToByteArray());
		}

		public ValueTask EmitWithdrawFormStatusChangedEventAsync(Guid formId, Guid brandId)
		{
			var request = new FormEvent
			{
				FormId = formId.ToString(),
				BrandId = brandId.ToString()
			};

			return _messageSender.PublishAsync("tgs.payment.withdraw.status", request.ToByteArray());
		}

		public ValueTask EmitWithdrawFormThirdPartyOrderNoAsync(Guid formId, string? thirdPartyOrderNo)
		{
			var request = new FormThirdNoEvent
			{
				FormId = formId.ToString(),
				ThirdPartyOrderNo = thirdPartyOrderNo
			};

			return _messageSender.PublishAsync("tgs.payment.withdraw.external_id", request.ToByteArray());
		}

		public async ValueTask<string> WithdrawAsync(string subject, decimal amount, string? applyInfo)
		{
			var request = new WithdrawRequest
			{
				ApplyInfo = applyInfo ?? string.Empty,
				Amount = amount.ToString()
			};

			var result = await _messageSender
				.RequestAsync(subject, request.ToByteArray())
				.ConfigureAwait(false);

			var response = WithdrawResponse.Parser.ParseFrom(result);

			_logger.LogInformation(response.Result);

			return response.Result;
		}
	}
}