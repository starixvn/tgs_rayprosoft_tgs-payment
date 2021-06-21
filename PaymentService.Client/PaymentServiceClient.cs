using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace Tgs.Payment.Clients
{
	internal class PaymentServiceClient : IPaymentServiceClient
	{
		private readonly HttpClient _httpClient;
		private readonly JsonSerializerOptions? _serializerOptions;

		public PaymentServiceClient(HttpClient httpClient, JsonSerializerOptions? serializerOptions)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_serializerOptions = serializerOptions;
		}

		public async ValueTask<Guid> CreateDepositFormAsync(DepositFormCreateInfo createInfo)
		{
			var requestContent = new StringContent(
				JsonSerializer.Serialize(createInfo, _serializerOptions));

			requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);

			var response = await _httpClient
				.PutAsync(
					"/deposit",
					requestContent)
				.ConfigureAwait(false);

			response.EnsureSuccessStatusCode();

			var contentJson = await response.Content
				.ReadAsStringAsync()
				.ConfigureAwait(false);

			return JsonSerializer.Deserialize<Guid>(contentJson, _serializerOptions);
		}

		public async ValueTask<Guid> CreatePlayerBankInfoAsync(Guid brandId, string playerCode, BankCreateInfo info)
		{
			var requestContent = new StringContent(
				JsonSerializer.Serialize(info, _serializerOptions));

			requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);

			var response = await _httpClient
				.PutAsync(
					$"/bankinfo/player/{brandId}/{playerCode}",
					requestContent)
				.ConfigureAwait(false);

			response.EnsureSuccessStatusCode();

			var contentJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			return JsonSerializer.Deserialize<Guid>(contentJson, _serializerOptions);
		}

		public async IAsyncEnumerable<PaymentChannelInfo> GetAllPaymentChannelsAsync()
		{
			var response = await _httpClient
				.GetAsync("/payment/channel")
				.ConfigureAwait(false);

			response.EnsureSuccessStatusCode();

			var contentJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			var paymentChannels = JsonSerializer.Deserialize<PaymentChannelInfo[]>(contentJson, _serializerOptions);

			foreach (var pc in paymentChannels!)
				yield return pc;
		}

		public async IAsyncEnumerable<PaymentMethodInfo> GetAllPaymentMethosAsync()
		{
			var response = await _httpClient
				.GetAsync("/payment/method")
				.ConfigureAwait(false);

			response.EnsureSuccessStatusCode();

			var contentJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			var paymentMethods = JsonSerializer.Deserialize<PaymentMethodInfo[]>(
				  contentJson,
				  _serializerOptions);

			foreach (var pm in paymentMethods!)
				yield return pm;
		}

		public async IAsyncEnumerable<PaymentInfo> GetAllPaymentsAsync()
		{
			var response = await _httpClient
				.GetAsync("/payment")
				.ConfigureAwait(false);

			response.EnsureSuccessStatusCode();

			var contentJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			var payments = JsonSerializer.Deserialize<PaymentInfo[]>(contentJson, _serializerOptions);

			foreach (var payment in payments!)
				yield return payment;
		}

		public async ValueTask<DepositFormInfo?> GetDepositFormAsync(Guid brandId, Guid formId)
		{
			var response = await _httpClient
				.GetAsync($"/deposit/{brandId}/{formId}")
				.ConfigureAwait(false);

			response.EnsureSuccessStatusCode();

			var contentJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			return JsonSerializer.Deserialize<DepositFormInfo>(contentJson, _serializerOptions);
		}

		public async ValueTask<PaymentInfo?> GetPaymentByIdAsync(Guid paymentId)
		{
			var response = await _httpClient
				.GetAsync($"/payment/{paymentId}")
				.ConfigureAwait(false);

			response.EnsureSuccessStatusCode();

			var contentJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			return JsonSerializer.Deserialize<PaymentInfo>(contentJson, _serializerOptions);
		}

		public async IAsyncEnumerable<PlayerBankInfo> GetPlayerBankInfosAsync(string playerCode)
		{
			var response = await _httpClient
				.GetAsync($"/bankinfo/player/{playerCode}")
				.ConfigureAwait(false);

			response.EnsureSuccessStatusCode();

			var contentJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			var bankInfos = JsonSerializer.Deserialize<PlayerBankInfo[]>(contentJson, _serializerOptions);

			foreach (var bankInfo in bankInfos!)
				yield return bankInfo;
		}

		public async ValueTask<PagingResponse<DepositFormInfo>> QueryDepositFormsAsync(
			DepositFormQueryConditions conditions,
			Paging paging)
		{
			var queryStrings = HttpUtility.ParseQueryString(string.Empty);

			if (!string.IsNullOrWhiteSpace(conditions.ChannelName))
				queryStrings.Add("channel", conditions.ChannelName);

			if (conditions.PaymentMethodId != null)
				queryStrings.Add("method", conditions.PaymentMethodId.ToString());

			if (!string.IsNullOrWhiteSpace(conditions.UserName))
				queryStrings.Add("user_name", conditions.UserName);

			foreach (var state in conditions.Status)
				queryStrings.Add("status", state.ToString());

			if (conditions.CreateOnRange != null)
			{
				queryStrings.Add("bc", conditions.CreateOnRange.Value.Begin.ToString("o"));
				queryStrings.Add("ec", conditions.CreateOnRange.Value.End.ToString("o"));
			}

			if (conditions.UpdateOnRange != null)
			{
				queryStrings.Add("bu", conditions.UpdateOnRange.Value.Begin.ToString("O"));
				queryStrings.Add("eu", conditions.UpdateOnRange.Value.End.ToString("o"));
			}

			if (paging.ShowAll == true)
				queryStrings.Add("all", "true");
			else
			{
				if (paging.Page != null)
					queryStrings.Add("page", paging.Page.ToString());

				if (paging.Rows != null)
					queryStrings.Add("rows", paging.Rows.ToString());
			}

			if (!string.IsNullOrWhiteSpace(paging.SortBy))
			{
				queryStrings.Add("sort", paging.SortBy);

				if (paging.IsDesc == true)
					queryStrings.Add("desc", "true");
			}

			var response = await _httpClient
				.GetAsync($"/deposit/{conditions.BrandId}?{queryStrings}")
				.ConfigureAwait(false);

			response.EnsureSuccessStatusCode();

			var contentJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			return JsonSerializer.Deserialize<PagingResponse<DepositFormInfo>>(contentJson, _serializerOptions)!;
		}

		public async ValueTask ReviewDepositFormAsync(Guid formId, ReviewState state, string? remark = null)
		{
			var request = new SignOffForm
			{
				ReviewState = state,
				Remark = remark
			};

			var requestContent = new StringContent(
				JsonSerializer.Serialize(request, _serializerOptions));

			requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);

			var response = await _httpClient
				.PostAsync(
					$"/deposit/{formId}",
					requestContent)
				.ConfigureAwait(false);

			response.EnsureSuccessStatusCode();
		}

		public async ValueTask SubmitDepositFormStatusAsync(Guid formId, PaymentFlowState state, string? paymentReturn = null)
		{
			var request = new DepositFormStatus
			{
				Status = state,
				PaymentReturn = paymentReturn
			};

			var requestContent = new StringContent(
				JsonSerializer.Serialize(request, _serializerOptions));

			requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);

			var response = await _httpClient
				.PatchAsync(
					$"/deposit/{formId}",
					requestContent)
				.ConfigureAwait(false);

			response.EnsureSuccessStatusCode();
		}
	}
}
