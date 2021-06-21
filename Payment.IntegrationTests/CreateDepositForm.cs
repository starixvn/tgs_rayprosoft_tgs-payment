using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Tgs.Payment.Clients;
using Tgs.Payment.Repositories.Entities;

namespace Payment.IntegrationTests
{
	[Binding]
	public class CreateDepositForm
	{
		private readonly PaymentServiceClient _paymentServiceClient;
		private readonly IHost _testHost;
		private DepositFormCreateInfo _depositForm;
		private Guid _createdFormId;

		public CreateDepositForm()
		{
			_testHost = TestSiteHelper.GetPaymentServiceTestHost();

			_ = _testHost.StartAsync();

			_paymentServiceClient = new PaymentServiceClient(
				_testHost.GetTestClient(),
				new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				});
		}

		[Given(@"I will create deposit form")]
		public void GivenIWillCreateDepositForm()
		{
			_depositForm = new DepositFormCreateInfo
			{
				Amount = 1000M,
				ApplyInfo = "{}",
				BrandId = Guid.NewGuid(),
				BrandPaymentId = Guid.NewGuid(),
				PlayerCode = "PlayerCode",
				PromotionId = null
			};
		}

		[Then(@"deposit form created")]
		public void ThenDepositFormCreated()
		{
			Assert.AreNotEqual(Guid.Empty, _createdFormId);
		}

		[When(@"call payment api")]
		public async Task WhenCallPaymentApiAsync()
		{
			_createdFormId = await _paymentServiceClient.CreateDepositFormAsync(_depositForm);
		}

		[AfterScenario]
		public async Task CleanUpAsync()
		{
			await _testHost.StopAsync();
		}
	}
}
