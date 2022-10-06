using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices.Settings;

namespace Payment.UnitTests;

public class SettingsTests
{
	[Test]
	public async Task Get_BrandPlayerBankInfoDuplicateCheck_Flag()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new GetPlayerBankInfoDuplicateCheckIsEnabledQueryService(fakePaymentRepository);

		var query = new GetPlayerBankInfoDuplicateCheckIsEnabledQuery(Guid.NewGuid());

		_ = await sut.GetAsync(query);

		_ = fakePaymentRepository.Received(1)
			.GetBrandCheckPlayerBankInfoDuplicateIsEnabledAsync(Arg.Is(query.brandId));
	}

	[Test]
	public async Task Set_BrandPlayerBankInfoDuplicateCheck_Enabled()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new SetPlayerBankInfoDuplicateCheckIsEnabledCommandService(fakePaymentRepository);

		var command = new SetPlayerBankInfoDuplicateCheckIsEnabledCommand(Guid.NewGuid(), true);

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.SetBrandCheckPlayerBankInfoDuplicateEnabledAsync(
				Arg.Is(command.BrandId),
				Arg.Is(command.IsEnabled));
	}
}
