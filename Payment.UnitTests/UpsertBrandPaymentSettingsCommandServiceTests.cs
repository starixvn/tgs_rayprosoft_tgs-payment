using NSubstitute;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices.Settings;

namespace Payment.UnitTests;

public class UpsertBrandPaymentSettingsCommandServiceTests
{
	[Test]
	public async Task 更新Brand的金流設定值()
	{
		var fakePaymentRepository = Substitute.For<IPaymentRepository>();

		var sut = new UpsertBrandPaymentSettingsCommandService(fakePaymentRepository);

		var command = new UpsertBrandPaymentSettingsCommand(
			Guid.NewGuid(),
			new BrandPaymentSettings(10));

		await sut.ExecuteAsync(command);

		_ = fakePaymentRepository.Received(1)
			.UpsertBrandPaymentSettingsAsync(
				Arg.Is(command.BrandId),
				Arg.Is(command.ModifyData));
	}
}
