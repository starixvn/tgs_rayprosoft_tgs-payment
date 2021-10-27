using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Tgs.Payment.Repositories;
using Tgs.Payment.Repositories.Entities;
using TGS.Payment.Repositories;

namespace PaymentRepositories.UnitTests
{
	public class PaymentRepositoryTests
	{
		[Test]
		public async Task Can_Get_DepositForm_OrderNo()
		{
			var options = Options.Create(new RedisOptions());
			var redisConnectionFactory = new RedisConnectionFactory(options);
			var sut = new RedisTakeNumberStore(redisConnectionFactory);

			var brandCode = "GPI";
			var dateTimeRule = DateTime.Now.ToString("yyMMddHH");
			var formType = Tgs.Payment.FormType.Deposit;
			var order = await sut.GetOrderNoAsync(brandCode, formType);
			var formTypeRule = formType == Tgs.Payment.FormType.Deposit ? "T" : "W";

			var actual = order.Substring(0, 12);

			var expected = $"{brandCode}{formTypeRule}{dateTimeRule}";

			Assert.AreEqual(16, order.Length);
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public async Task Can_Get_WithdrawForm_OrderNo()
		{
			var options = Options.Create(new RedisOptions());
			var redisConnectionFactory = new RedisConnectionFactory(options);
			var sut = new RedisTakeNumberStore(redisConnectionFactory);

			var brandCode = "GPI";
			var dateTimeRule = DateTime.Now.ToString("yyMMddHH");
			var formType = Tgs.Payment.FormType.Withdraw;
			var order = await sut.GetOrderNoAsync(brandCode, formType);
			var formTypeRule = formType == Tgs.Payment.FormType.Deposit ? "T" : "W";

			var actual = order.Substring(0, 12);
			var expected = $"{brandCode}{formTypeRule}{dateTimeRule}";

			Assert.AreEqual(16, order.Length);
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public async Task Submit_DepositForm_Status_Without_PaymentReturn_The_Data_Will_Keep_Original()
		{
			var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
			dbContextOptionsBuilder
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.ConfigureWarnings(x => x.Ignore(
					InMemoryEventId.TransactionIgnoredWarning));

			var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

			var testFormData = new DepositForm
			{
				Id = Guid.NewGuid(),
				Status = 0,
				PaymentReturn = "Test"
			};

			await dbContext.DepositForms.AddAsync(testFormData);

			await dbContext.SaveChangesAsync();

			var sut = new PaymentRepository(dbContext);

			await sut.SubmitDepositFormStatusAsync(
				testFormData.Id,
				Tgs.Payment.PaymentFlowState.Paid,
				null,
				null,
				null);

			Assert.AreEqual("Test", testFormData.PaymentReturn);
		}
	}
}
