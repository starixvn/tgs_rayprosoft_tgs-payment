using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Stateless;
using Tgs.Payment.Clients.BrandPayment.PaymentConditions;

namespace Tgs.Payment.Clients;

internal class PaymentServiceClient : IPaymentServiceClient
{
	private static readonly JsonSerializerOptions _SerializerOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = true,
		Converters = {
			new JsonStringEnumConverter()
		}
	};

	private readonly HttpClient _httpClient;

	public PaymentServiceClient(HttpClient httpClient)
	{
		_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
	}

	public async ValueTask<int> CountUntreatedDepositFormsAsync(Guid brandId, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient
			.GetAsync($"/deposit/untreated/{brandId}", cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		return JsonSerializer.Deserialize<int>(contentJson, _SerializerOptions);
	}

	public async ValueTask<int> CountUntreatedWithdrawFormsAsync(Guid brandId, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient
			.GetAsync($"/withdraw/untreated/{brandId}", cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		return JsonSerializer.Deserialize<int>(contentJson, _SerializerOptions);
	}

	public async ValueTask<Guid> CreateAgentBankInfoAsync(
		Guid brandId,
		Guid agentId,
		string agentName,
		BankCreateInfo info,
		CancellationToken cancellationToken = default)
	{
		var requestContent = new StringContent(
			JsonSerializer.Serialize(info, _SerializerOptions));

		requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);

		var response = await _httpClient
			.PutAsync(
				$"/bankinfo/agent/{brandId}/{agentId}?name={agentName}",
				requestContent,
				cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		return JsonSerializer.Deserialize<Guid>(contentJson, _SerializerOptions);
	}

	public async ValueTask<Guid> CreateDepositFormAsync(Guid createFormId, DepositFormCreateInfo createInfo, CancellationToken cancellationToken = default)
	{
		var requestContent = new StringContent(
			JsonSerializer.Serialize(createInfo, _SerializerOptions));

		requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);

		var response = await _httpClient
			.PutAsync($"/deposit/{createFormId}", requestContent, cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		return JsonSerializer.Deserialize<Guid>(contentJson, _SerializerOptions);
	}

	public StateMachine<PaymentFlowState, PaymentTrigger> CreateDepositStateMachine(PaymentFlowState initialState = PaymentFlowState.Init)
	{
		var fsm = new StateMachine<PaymentFlowState, PaymentTrigger>(initialState);

		_ = fsm.Configure(PaymentFlowState.Init)
			.Permit(PaymentTrigger.DepositCreate, PaymentFlowState.WaitPayment)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		_ = fsm.Configure(PaymentFlowState.WaitPayment)
			.Permit(PaymentTrigger.PaymentDepositCompleted, PaymentFlowState.WaitApprove)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		_ = fsm.Configure(PaymentFlowState.WaitApprove)
			.Permit(PaymentTrigger.Approve, PaymentFlowState.Approved)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		return fsm;
	}

	public StateMachine<PaymentFlowState, PaymentTrigger> CreateManualDepositStateMachine(PaymentFlowState initialState = PaymentFlowState.Init)
	{
		var fsm = new StateMachine<PaymentFlowState, PaymentTrigger>(initialState);

		_ = fsm.Configure(PaymentFlowState.Init)
			.Permit(PaymentTrigger.DepositCreate, PaymentFlowState.WaitPayment)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		_ = fsm.Configure(PaymentFlowState.WaitPayment)
			.Permit(PaymentTrigger.UploadPayCertificate, PaymentFlowState.WaitApprove)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		_ = fsm.Configure(PaymentFlowState.WaitApprove)
			.Permit(PaymentTrigger.Approve, PaymentFlowState.Approved)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		return fsm;
	}

	public async ValueTask CreatePaymentAsync(PaymentCreateInfo createInfo, CancellationToken cancellationToken = default)
	{
		var requestContent = new StringContent(
			JsonSerializer.Serialize(createInfo, _SerializerOptions));

		requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);

		var response = await _httpClient
			.PutAsync("/payment", requestContent, cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();
	}

	public async ValueTask CreatePaymentChannelAsync(string channelCode, CancellationToken cancellationToken = default)
	{
		var requestContent = new StringContent(
			JsonSerializer.Serialize(channelCode, _SerializerOptions));

		requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);

		var response = await _httpClient
			.PutAsync("/payment/channel", requestContent, cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();
	}

	public async ValueTask CreatePaymentMethodAsync(PaymentMethodCreateInfo createInfo, CancellationToken cancellationToken = default)
	{
		var requestContent = new StringContent(
			JsonSerializer.Serialize(createInfo, _SerializerOptions));

		requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);

		var response = await _httpClient
			.PutAsync("/payment/method", requestContent, cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();
	}

	public async ValueTask<Guid> CreatePaymentMethodChannelAsync(
		Guid paymentId,
		Guid methodId,
		Guid channelId,
		CancellationToken cancellationToken = default)
	{
		var request = new
		{
			PaymentId = paymentId,
			MethodId = methodId,
			ChannelId = channelId
		};

		var requestContent = new StringContent(
			JsonSerializer.Serialize(request, _SerializerOptions));

		requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);

		var response = await _httpClient
			.PutAsync("/payment/pmc", requestContent, cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		return await response.Content
			.ReadFromJsonAsync<Guid>(_SerializerOptions, cancellationToken)
			.ConfigureAwait(false);
	}

	public async ValueTask<Guid> CreatePlayerBankInfoAsync(
			Guid brandId,
		string playerCode,
		BankCreateInfo info,
		string? modifyUser = null,
		CancellationToken cancellationToken = default)
	{
		var requestContent = new StringContent(
			JsonSerializer.Serialize(info, _SerializerOptions));

		requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);

		if (!string.IsNullOrWhiteSpace(modifyUser))
			requestContent.Headers.Add("ModifyUser", modifyUser);

		var response = await _httpClient
			.PutAsync($"/bankinfo/player/{brandId}/{playerCode}", requestContent, cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		return JsonSerializer.Deserialize<Guid>(contentJson, _SerializerOptions);
	}

	public async ValueTask<Guid> CreateWithdrawFormAsync(WithdrawFormCreateInfo createInfo, CancellationToken cancellationToken = default)
	{
		var requestContent = new StringContent(
			JsonSerializer.Serialize(createInfo, _SerializerOptions));

		requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);

		var response = await _httpClient
			.PutAsync("/withdraw", requestContent, cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		return JsonSerializer.Deserialize<Guid>(contentJson, _SerializerOptions);
	}

	public StateMachine<PaymentFlowState, PaymentTrigger> CreateWithdrawStateMachine(PaymentFlowState initialState = PaymentFlowState.Init)
	{
		var fsm = new StateMachine<PaymentFlowState, PaymentTrigger>(initialState);

		_ = fsm.Configure(PaymentFlowState.Init)
			.Permit(PaymentTrigger.WithdrawCreate, PaymentFlowState.WaitApprove)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		_ = fsm.Configure(PaymentFlowState.WaitApprove)
			.Permit(PaymentTrigger.Approve, PaymentFlowState.Approved)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		_ = fsm.Configure(PaymentFlowState.Approved)
			.Permit(PaymentTrigger.PaymentWithdrawCompleted, PaymentFlowState.Paid)
			.Permit(PaymentTrigger.Reject, PaymentFlowState.Reject);

		return fsm;
	}

	public async ValueTask DeleteAgentBankInfoAsync(Guid agentId, Guid bankInfoId, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient
			.DeleteAsync($"/bankinfo/agent/{agentId}/{bankInfoId}", cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();
	}

	public async ValueTask DeletePaymentMethodChannelAsync(Guid pmcId, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient
			.DeleteAsync($"/payment/pmc/{pmcId}", cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();
	}

	public async ValueTask DeletePlayerBankInfoAsync(string playerCode, Guid bankInfoId, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient
			.DeleteAsync($"/bankinfo/player/{playerCode}/{bankInfoId}", cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();
	}

	public async IAsyncEnumerable<BrandPaymentInfo> FilterBrandPaymentsAsync(IEnumerable<BrandPaymentInfo> brandPaymentInfos, PlayerInfo playerInfo, IEnumerable<Guid> blockList, IEnumerable<Guid> whiteList, ILookup<Guid, Guid> brandPaymentVipLevels, CancellationToken cancellationToken = default)
	{
		var blockListCondition = new BlockListCondition(blockList);
		var whiteListCondition = new WhiteListCondition(whiteList);
		var showOnAllPlayerCondition = new ShowOnAllPlayersCondition();

		foreach (var bp in brandPaymentInfos)
		{
			if (!bp.Enabled)
				continue;

			var depositedAmountCondition = new DepositedAmountCondition(bp.DepositedAmountLimit);
			var identityCondition = new IdentityCondition(bp.AuthenticateIdentity);
			var phoneCondition = new PhoneCondition(bp.AuthenticatePhone);
			var vipCondition = new VipCondition(brandPaymentVipLevels.Contains(bp.Id)
					? brandPaymentVipLevels[bp.Id]
					: Array.Empty<Guid>());

			var filterCondition = new BrandPaymentFilterCondition(
				blockListCondition,
				whiteListCondition,
				showOnAllPlayerCondition,
				bp.ConditionMatchType == ConditionMatchType.MatchAll
					? new MatchAllCondition(
						depositedAmountCondition,
						identityCondition,
						phoneCondition,
						vipCondition)
					: new MatchOneCondition(
						depositedAmountCondition,
						identityCondition,
						phoneCondition,
						vipCondition));

			if (filterCondition.IsValid(bp, playerInfo!))
				yield return bp;
		}
	}

	public async IAsyncEnumerable<AgentBankInfo> GetAgentBankInfosAsync(
		Guid agentId,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var response = await _httpClient
			.GetAsync($"/bankinfo/agent/{agentId}", cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		var bankInfos = JsonSerializer.Deserialize<AgentBankInfo[]>(contentJson, _SerializerOptions);

		foreach (var bankInfo in bankInfos!)
			yield return bankInfo;
	}

	public async IAsyncEnumerable<PaymentChannelInfo> GetAllPaymentChannelsAsync(
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var response = await _httpClient
			.GetAsync("/payment/channel", cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		var paymentChannels = JsonSerializer.Deserialize<PaymentChannelInfo[]>(contentJson, _SerializerOptions);

		foreach (var pc in paymentChannels!)
			yield return pc;
	}

	public async IAsyncEnumerable<PaymentMethodChannelInfo> GetAllPaymentMethodChannelsAsync(
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var response = await _httpClient
			.GetAsync("/payment/pmc", cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		var pmcs = JsonSerializer.Deserialize<PaymentMethodChannelInfo[]>(contentJson, _SerializerOptions);

		foreach (var pmc in pmcs!)
			yield return pmc;
	}

	public async IAsyncEnumerable<PaymentMethodInfo> GetAllPaymentMethodsAsync(
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var response = await _httpClient
			.GetAsync("/payment/method", cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		var paymentMethods = JsonSerializer.Deserialize<PaymentMethodInfo[]>(
			  contentJson,
			  _SerializerOptions);

		foreach (var pm in paymentMethods!)
			yield return pm;
	}

	public async IAsyncEnumerable<PaymentInfo> GetAllPaymentsAsync(
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var response = await _httpClient
			.GetAsync("/payment", cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		var payments = JsonSerializer.Deserialize<PaymentInfo[]>(contentJson, _SerializerOptions);

		foreach (var payment in payments!)
			yield return payment;
	}

	public async IAsyncEnumerable<BrandPaymentAcl> GetBrandPaymentAclsAsync(
		string playerCode,
		IEnumerable<Guid> brandPaymentIds,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var brandQueryString = string.Join(
			"&",
			brandPaymentIds.Select(id => $"brand={id}"));
		var response = await _httpClient
			.GetAsync($"/brand/payment/acl/{playerCode}?{brandQueryString}", cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		var acls = JsonSerializer.Deserialize<BrandPaymentAcl[]>(contentJson, _SerializerOptions);

		foreach (var acl in acls!)
			yield return acl;
	}

	public async ValueTask<BrandPaymentInfo?> GetBrandPaymentByIdAsync(Guid brandPaymentId, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient
			.GetAsync($"/brand/payment/{brandPaymentId}", cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		return JsonSerializer.Deserialize<BrandPaymentInfo>(contentJson, _SerializerOptions);
	}

	public async IAsyncEnumerable<BrandPaymentInfo> GetBrandPaymentsAsync(
		Guid brandId,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var response = await _httpClient
			.GetAsync($"/brand/payment?brand={brandId}", cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		var brandPayments = JsonSerializer.Deserialize<BrandPaymentInfo[]>(contentJson, _SerializerOptions);

		foreach (var bp in brandPayments!)
			yield return bp;
	}

	public async IAsyncEnumerable<BrandPaymentVip> GetBrandPaymentVipsAsync(
		IEnumerable<Guid> brandPaymentIds,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var brandQueryString = string.Join(
			"&",
			brandPaymentIds.Select(id => $"brand={id}"));
		var response = await _httpClient
			.GetAsync($"/brand/payment/vip?{brandQueryString}", cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		var vips = JsonSerializer.Deserialize<BrandPaymentVip[]>(contentJson, _SerializerOptions);

		foreach (var vip in vips!)
			yield return vip;
	}

	public async ValueTask<DepositFormInfo?> GetDepositFormAsync(Guid brandId, Guid formId, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient
			.GetAsync($"/deposit/{brandId}/{formId}", cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		if (response.StatusCode == HttpStatusCode.NoContent)
			return null;

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		return JsonSerializer.Deserialize<DepositFormInfo>(contentJson, _SerializerOptions);
	}

	public async ValueTask<DepositFormInfo?> GetDepositFormAsync(Guid formId, CancellationToken cancellationToken = default)
	{
		var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/deposit/{formId}");
		requestMessage.Headers.Add("FormQuery", string.Empty);

		var response = await _httpClient
			.SendAsync(requestMessage, cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		if (response.StatusCode == HttpStatusCode.NoContent)
			return null;

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		return JsonSerializer.Deserialize<DepositFormInfo>(contentJson, _SerializerOptions);
	}

	public async ValueTask<PaymentInfo?> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient
			.GetAsync($"/payment/{paymentId}", cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		return JsonSerializer.Deserialize<PaymentInfo>(contentJson, _SerializerOptions);
	}

	public async IAsyncEnumerable<PlayerBankInfo> GetPlayerBankInfosAsync(
		string playerCode,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var response = await _httpClient
			.GetAsync($"/bankinfo/player/{playerCode}", cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		var contentJson = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

		var bankInfos = JsonSerializer.Deserialize<PlayerBankInfo[]>(contentJson, _SerializerOptions);

		foreach (var bankInfo in bankInfos!)
			yield return bankInfo;
	}

	public async ValueTask<WithdrawFormInfo?> GetWithdrawFormAsync(Guid brandId, Guid formId, CancellationToken cancellationToken = default)
	{
		var response = await _httpClient
			.GetAsync($"/withdraw/{brandId}/{formId}", cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		return JsonSerializer.Deserialize<WithdrawFormInfo>(contentJson, _SerializerOptions);
	}

	public async ValueTask<WithdrawFormInfo?> GetWithdrawFormAsync(Guid formId, CancellationToken cancellationToken = default)
	{
		var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/withdraw/{formId}");
		requestMessage.Headers.Add("FormQuery", string.Empty);

		var response = await _httpClient
			.SendAsync(requestMessage, cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		return JsonSerializer.Deserialize<WithdrawFormInfo>(contentJson, _SerializerOptions);
	}

	public async ValueTask<Guid> LockDepositPaymentAsync(Guid formId, string paymentCode, string playerCode, CancellationToken cancellationToken = default)
	{
		var request = new
		{
			FormId = formId,
			PaymentCode = paymentCode,
			PlayerCode = playerCode
		};

		var requestContent = new StringContent(JsonSerializer.Serialize(request, _SerializerOptions));

		requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);

		var response = await _httpClient
			.PutAsync("/deposit/mutex", requestContent, cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		return await response.Content
			.ReadFromJsonAsync<Guid>(_SerializerOptions, cancellationToken)
			.ConfigureAwait(false);
	}

	public async ValueTask<DepositForms> QueryDepositFormsAsync(
			DepositFormQueryConditions conditions,
		Paging paging,
		CancellationToken cancellationToken = default)
	{
		var queryStrings = HttpUtility.ParseQueryString(string.Empty);

		if (!string.IsNullOrWhiteSpace(conditions.ChannelName))
			queryStrings.Add("channel", conditions.ChannelName);

		if (conditions.PaymentMethodId != null)
			queryStrings.Add("method", conditions.PaymentMethodId.ToString());

		if (!string.IsNullOrWhiteSpace(conditions.UserId))
			queryStrings.Add("user_id", conditions.UserId);

		if (!string.IsNullOrWhiteSpace(conditions.UserName))
			queryStrings.Add("user_name", conditions.UserName);

		if (!string.IsNullOrWhiteSpace(conditions.ThirdPartyOrderNo))
			queryStrings.Add("third_party_order_no", conditions.ThirdPartyOrderNo);

		if (!string.IsNullOrWhiteSpace(conditions.OrderNo))
			queryStrings.Add("order_no", conditions.OrderNo);

		if (conditions.FormId != null)
			queryStrings.Add("form_id", conditions.FormId.ToString());

		foreach (var state in conditions.Status)
			queryStrings.Add("status", state.ToString());

		if (conditions.CreateOnRange != null)
		{
			queryStrings.Add("bc", conditions.CreateOnRange.Value.Begin.ToString("o"));
			queryStrings.Add("ec", conditions.CreateOnRange.Value.End.ToString("o"));
		}

		if (conditions.UpdateOnRange != null)
		{
			queryStrings.Add("bu", conditions.UpdateOnRange.Value.Begin.ToString("o"));
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
			.GetAsync($"/deposit/{conditions.BrandId}?{queryStrings}", cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		return JsonSerializer.Deserialize<DepositForms>(contentJson, _SerializerOptions)!;
	}

	public async ValueTask<WithdrawForms> QueryWithdrawFormsAsync(
		WithdrawFormQueryConditions conditions,
		Paging paging,
		CancellationToken cancellationToken = default)
	{
		var queryStrings = HttpUtility.ParseQueryString(string.Empty);

		if (!string.IsNullOrWhiteSpace(conditions.ChannelName))
			queryStrings.Add("channel", conditions.ChannelName);

		if (conditions.PaymentMethodId != null)
			queryStrings.Add("method", conditions.PaymentMethodId.ToString());

		if (!string.IsNullOrWhiteSpace(conditions.UserName))
			queryStrings.Add("user_name", conditions.UserName);

		if (!string.IsNullOrWhiteSpace(conditions.UserId))
			queryStrings.Add("user_id", conditions.UserId);

		if (conditions.UserType != null)
			queryStrings.Add("user_type", conditions.UserType.ToString());

		if (!string.IsNullOrWhiteSpace(conditions.ThirdPartyOrderNo))
			queryStrings.Add("third_party_order_no", conditions.ThirdPartyOrderNo);

		if (!string.IsNullOrWhiteSpace(conditions.OrderNo))
			queryStrings.Add("order_no", conditions.OrderNo);

		if (conditions.FormId != null)
			queryStrings.Add("form_id", conditions.FormId.ToString());

		foreach (var state in conditions.Status)
			queryStrings.Add("status", state.ToString());

		if (conditions.CreateOnRange != null)
		{
			queryStrings.Add("bc", conditions.CreateOnRange.Value.Begin.ToString("o"));
			queryStrings.Add("ec", conditions.CreateOnRange.Value.End.ToString("o"));
		}

		if (conditions.UpdateOnRange != null)
		{
			queryStrings.Add("bu", conditions.UpdateOnRange.Value.Begin.ToString("o"));
			queryStrings.Add("eu", conditions.UpdateOnRange.Value.End.ToString("o"));
		}

		if (conditions.ReviewOnRange != null)
		{
			queryStrings.Add("br", conditions.ReviewOnRange.Value.Begin.ToString("o"));
			queryStrings.Add("er", conditions.ReviewOnRange.Value.End.ToString("o"));
		}

		if (conditions.PaidOnRange != null)
		{
			queryStrings.Add("bp", conditions.PaidOnRange.Value.Begin.ToString("o"));
			queryStrings.Add("ep", conditions.PaidOnRange.Value.End.ToString("o"));
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
			.GetAsync($"/withdraw/{conditions.BrandId}?{queryStrings}", cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();

		var contentJson = await response.Content
			.ReadAsStringAsync(cancellationToken)
			.ConfigureAwait(false);

		return JsonSerializer.Deserialize<WithdrawForms>(contentJson, _SerializerOptions)!;
	}

	public async ValueTask ReviewDepositFormAsync(
		Guid formId,
		ReviewState state,
		string? reviewUserId,
		string? reviewUserName,
		string? remark = null,
		CancellationToken cancellationToken = default)
	{
		var request = new SignOffForm
		{
			ReviewState = state,
			ReviewUserId = reviewUserId,
			ReviewUserName = reviewUserName,
			Remark = remark
		};

		var requestContent = new StringContent(
			JsonSerializer.Serialize(request, _SerializerOptions));

		requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);

		var response = await _httpClient
			.PostAsync($"/deposit/{formId}", requestContent, cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();
	}

	public async ValueTask ReviewWithdrawFormAsync(
		Guid formId,
		Guid? brandPaymentId,
		ReviewState state,
		string? reviewUserId,
		string? reviewUserName,
		string? remark = null,
		CancellationToken cancellationToken = default)
	{
		var request = new SignOffForm
		{
			ReviewState = state,
			Remark = remark,
			ReviewUserId = reviewUserId,
			ReviewUserName = reviewUserName,
			BrandPaymentId = brandPaymentId
		};

		var requestContent = new StringContent(
			JsonSerializer.Serialize(request, _SerializerOptions));

		requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);

		var response = await _httpClient
			.PostAsync($"/withdraw/{formId}", requestContent, cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();
	}

	public async ValueTask SetDefaultPlayerBankInfoAsync(
		string playerCode,
		Guid bankInfoId,
		string modifyUser,
		CancellationToken cancellationToken = default)
	{
		var requestMessage = new HttpRequestMessage(HttpMethod.Patch, $"/bankinfo/player/{playerCode}/{bankInfoId}/default");
		requestMessage.Headers.Add("ModifyUser", modifyUser);

		var response = await _httpClient
			.SendAsync(requestMessage, cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();
	}

	public async ValueTask SubmitDepositFormStatusAsync(
		Guid formId,
		PaymentFlowState? state,
		string? paymentReturn = null,
		decimal? actualAmount = null,
		string? thirdPartyOrderNo = null,
		int? promotionPlayerId = null,
		CancellationToken cancellationToken = default)
	{
		var request = new DepositFormStatus
		{
			Status = state,
			PaymentReturn = paymentReturn,
			ActualAmount = actualAmount,
			ThirdPartyOrderNo = thirdPartyOrderNo,
			PromotionPlayerId = promotionPlayerId
		};

		var requestContent = new StringContent(
			JsonSerializer.Serialize(request, _SerializerOptions));

		requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);

		var response = await _httpClient
			.PatchAsync($"/deposit/{formId}", requestContent, cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();
	}

	public async ValueTask SubmitWithdrawFormStatusAsync(
		Guid formId,
		PaymentFlowState? state,
		string? paymentReturn = null,
		decimal? actualAmount = null,
		string? thirdPartyOrderNo = null,
		string? paidUserId = null,
		string? paidUserName = null,
		CancellationToken cancellationToken = default)
	{
		var request = new WithdrawFormStatus
		{
			Status = state,
			PaymentReturn = paymentReturn,
			ActualAmount = actualAmount,
			ThirdPartyOrderNo = thirdPartyOrderNo,
			PaidUserId = paidUserId,
			PaidUserName = paidUserName
		};

		var requestContent = new StringContent(
			JsonSerializer.Serialize(request, _SerializerOptions));

		requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);

		var response = await _httpClient
			.PatchAsync($"/withdraw/{formId}", requestContent, cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();
	}

	public async ValueTask UnlockDepositPaymentAsync(Guid formId, string paymentCode, string playerCode, CancellationToken cancellationToken = default)
	{
		var request = new
		{
			FormId = formId,
			PaymentCode = paymentCode,
			PlayerCode = playerCode
		};

		var requestContent = new StringContent(JsonSerializer.Serialize(request, _SerializerOptions));

		requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);

		var response = await _httpClient
			.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/deposit/mutex")
			{
				Content = requestContent
			}, cancellationToken)
			.ConfigureAwait(false);

		_ = response.EnsureSuccessStatusCode();
	}
}
