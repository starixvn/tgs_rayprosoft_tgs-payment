using Google.Protobuf;
using Tgs.Payment.MessageQueue;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Payment.UnitTests;

public class PaymentBalanceDispatchProcessorTests
{
	[Test]
	public async Task PaymentBalanceDispatchProcessor_查詢商戶餘額的要求傳遞到對應的服務取得結果()
	{
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new PaymentBalanceDispatchProcessor(
			["test"],
			fakeMessageSender);

		var request = new PaymentBalanceRequest
		{
			BrandPaymentSettings = "{}"
		};

		_ = await sut.HandleAsync("tgs.payment.dispatch-balance.test", request.ToByteArray(), null);

		_ = fakeMessageSender.Received(1)
			.RequestAsync<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>(
				Arg.Is("tgs.payment.test.balance"),
				Arg.Any<ReadOnlyMemory<byte>>());
	}

	[Test]
	public async Task PaymentBalanceDispatchProcessor_查詢商戶餘額_如果不在支援列表中回傳不支援的結果()
	{
		var fakeMessageSender = Substitute.For<IMessageSender>();

		var sut = new PaymentBalanceDispatchProcessor(
			[],
			fakeMessageSender);

		var request = new PaymentBalanceRequest
		{
			BrandPaymentSettings = "{}"
		};

		var actual = await sut.HandleAsync("tgs.payment.dispatch-balance.test", request.ToByteArray(), null);
		var response = PaymentBalanceResponse.Parser.ParseFrom(actual.Span);

		_ = fakeMessageSender.Received(0)
			.RequestAsync<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>(
				Arg.Is("tgs.payment.test.balance"),
				Arg.Any<ReadOnlyMemory<byte>>());
		Assert.That((decimal?)response.Balance, Is.Null);
	}
}
