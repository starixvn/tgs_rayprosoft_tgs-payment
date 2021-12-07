using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;
using Tgs.Payment.Repositories.Entities;
using TGS.Payment.Repositories;

namespace PaymentRepositories.UnitTests
{
	public class PaymentRepositoryTests
	{
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
				PlayerCode = "Tester",
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
				null,
				null);

			Assert.AreEqual("Test", testFormData.PaymentReturn);
		}
	}
}
