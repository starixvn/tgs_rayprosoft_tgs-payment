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

		public ValueTask ApproveDepositAsync(Guid formId, Guid brandId, decimal? actualAmount)
		{
			var request = new ApproveDepositRequest
			{
				FormId = formId.ToString(),
				BrandId = brandId.ToString(),
				ActualAmount = actualAmount
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

		public async ValueTask EmitDepositFormStatusChangedEventAsync(Guid formId, Guid brandId, string status)
		{
			var request = new FormEvent
			{
				FormId = formId.ToString(),
				BrandId = brandId.ToString()
			};

			var requestEx = new FormEventEx
			{
				FormId = formId.ToString(),
				BrandId = brandId.ToString(),
				Status = status
			};

			await Task.WhenAll(
				_messageSender.PublishAsync("tgs.payment.deposit.status", request.ToByteArray()).AsTask(),
				_messageSender.PublishAsync("tgs.payment.deposit.status_ex", requestEx.ToByteArray()).AsTask())
				.ConfigureAwait(false);
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

		public async ValueTask EmitWithdrawFormStatusChangedEventAsync(Guid formId, Guid brandId, string status)
		{
			var request = new FormEvent
			{
				FormId = formId.ToString(),
				BrandId = brandId.ToString()
			};

			var requestEx = new FormEventEx
			{
				FormId = formId.ToString(),
				BrandId = brandId.ToString(),
				Status = status
			};

			await Task.WhenAll(
				_messageSender.PublishAsync("tgs.payment.withdraw.status", request.ToByteArray()).AsTask(),
				_messageSender.PublishAsync("tgs.payment.withdraw.status_ex", requestEx.ToByteArray()).AsTask())
				.ConfigureAwait(false);
		}

		public ValueTask SaveWithdrawFormThirdPartyOrderNoAsync(Guid formId, string thirdPartyOrderNo)
		{
			var request = new FormThirdNoRequest
			{
				FormId = formId,
				ThirdPartyOrderNo = thirdPartyOrderNo
			};

			return _messageSender.SendAsync("tgs.payment.withdraw.external_id", request.ToByteArray());
		}

		public ValueTask TagWithdrawFormPaidAsync(Guid formId)
		{
			var request = new TagPaidRequest
			{
				FormId = formId
			};

			return _messageSender.SendAsync("tgs.payment.withdraw.paid", request.ToByteArray());
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
