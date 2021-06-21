using NSubstitute;
using NUnit.Framework;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices;

namespace Payment.UnitTests
{
	[TestFixture]
	public class BankInfoTests
	{
		[Test]
		public void Can_List_BankInfos_By_PlayerCode()
		{
			var fakePaymentRepository = Substitute.For<IPaymentRepository>();

			var sut = new GetPlayerBankInfosQueryService(fakePaymentRepository);

			var query = new GetPlayerBankInfosQuery("ThisIsPlayerCode");

			sut.QueryAsync(query);

			fakePaymentRepository.Received(1)
				.GetBankInfosByPlayerCodeAsync(Arg.Is(query.PlayerCode));
		}
	}
}
