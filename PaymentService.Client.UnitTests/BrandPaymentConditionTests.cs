using System;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Tgs.Payment.Clients;
using Tgs.Payment.Clients.BrandPayment.PaymentConditions;

namespace PaymentService.Client.UnitTests;

[TestFixture]
public class BrandPaymentConditionTests
{
	[Test]
	public void AllMatchCondition_Should_All_SubConditions_Pass()
	{
		var playerInfo = new PlayerInfo(
			BrandId: Guid.NewGuid(),
			DepositedAmount: 0M,
			IdentityAuthentication: false,
			PhoneAuthentication: false,
			PlayerCode: "ThisIsPlayerCode",
			VipLevelId: null);

		var brandPayment = new BrandPaymentInfo
		{
			Id = Guid.NewGuid(),
			AuthenticateIdentity = false,
			AuthenticatePhone = false,
			ChannelCode = "TestChannelCode",
			ChannelId = Guid.NewGuid(),
			ChannelName = "TestChannel",
			ConditionMatchType = ConditionMatchType.MatchAll,
			DepositedAmountLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			Enabled = true,
			IsAllPlayers = true,
			MethodId = Guid.NewGuid(),
			PaymentId = Guid.NewGuid(),
			PaymentMethodChannelId = Guid.NewGuid(),
			TransferLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			TransferType = TransferType.Deposit
		};

		var fakeCondition1 = new Mock<IBrandPaymentCondition>();

		_ = fakeCondition1
			.Setup(fake => fake.IsValid(
				It.IsAny<BrandPaymentInfo>(),
				It.IsAny<PlayerInfo>()))
			.Returns(true);

		var fakeCondition2 = new Mock<IBrandPaymentCondition>();

		_ = fakeCondition2
			.Setup(fake => fake.IsValid(
				It.IsAny<BrandPaymentInfo>(),
				It.IsAny<PlayerInfo>()))
			.Returns(true);

		var fakeCondition3 = new Mock<IBrandPaymentCondition>();

		_ = fakeCondition3
			.Setup(fake => fake.IsValid(
				It.IsAny<BrandPaymentInfo>(),
				It.IsAny<PlayerInfo>()))
			.Returns(true);

		var sut = new MatchAllCondition(
			fakeCondition1.Object,
			fakeCondition2.Object,
			fakeCondition3.Object);

		var actual = sut.IsValid(brandPayment, playerInfo);

		ClassicAssert.IsTrue(actual);
	}

	[Test]
	public void AllPlayers_Is_False_Will_Ignore_After_Conditions()
	{
		var playerInfo = new PlayerInfo(
			BrandId: Guid.NewGuid(),
			DepositedAmount: 0M,
			IdentityAuthentication: false,
			PhoneAuthentication: false,
			PlayerCode: "ThisIsPlayerCode",
			VipLevelId: null);

		var brandPayment = new BrandPaymentInfo
		{
			Id = Guid.NewGuid(),
			AuthenticateIdentity = false,
			AuthenticatePhone = false,
			ChannelCode = "TestChannelCode",
			ChannelId = Guid.NewGuid(),
			ChannelName = "TestChannel",
			ConditionMatchType = ConditionMatchType.MatchAll,
			DepositedAmountLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			Enabled = true,
			IsAllPlayers = false,
			MethodId = Guid.NewGuid(),
			PaymentId = Guid.NewGuid(),
			PaymentMethodChannelId = Guid.NewGuid(),
			TransferLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			TransferType = TransferType.Deposit
		};

		var fakeCondition = new Mock<IBrandPaymentCondition>();

		_ = fakeCondition
			.Setup(fake => fake.IsValid(
				It.IsAny<BrandPaymentInfo>(),
				It.IsAny<PlayerInfo>()))
			.Returns(true);

		var sut = new BrandPaymentFilterCondition(
			new BlockListCondition(Array.Empty<Guid>()),
			new WhiteListCondition(Array.Empty<Guid>()),
			new ShowOnAllPlayersCondition(),
			fakeCondition.Object);

		var actual = sut.IsValid(brandPayment, playerInfo);

		ClassicAssert.IsFalse(actual);
	}

	[Test]
	public void Authenticate_Identity_Should_Always_Valid()
	{
		var playerInfo = new PlayerInfo(
			BrandId: Guid.NewGuid(),
			DepositedAmount: 0M,
			IdentityAuthentication: false,
			PhoneAuthentication: false,
			PlayerCode: "ThisIsPlayerCode",
			VipLevelId: null);

		var brandPayment = new BrandPaymentInfo
		{
			Id = Guid.NewGuid(),
			AuthenticateIdentity = false,
			AuthenticatePhone = false,
			ChannelCode = "TestChannelCode",
			ChannelId = Guid.NewGuid(),
			ChannelName = "TestChannel",
			ConditionMatchType = ConditionMatchType.MatchAll,
			DepositedAmountLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			Enabled = true,
			IsAllPlayers = true,
			MethodId = Guid.NewGuid(),
			PaymentId = Guid.NewGuid(),
			PaymentMethodChannelId = Guid.NewGuid(),
			TransferLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			TransferType = TransferType.Deposit
		};

		var sut = new IdentityCondition(brandPayment.AuthenticateIdentity);

		var actual = sut.IsValid(brandPayment, playerInfo);

		ClassicAssert.IsTrue(actual);
	}

	[Test]
	public void Authenticate_Identity_Should_Invalid()
	{
		var playerInfo = new PlayerInfo(
			BrandId: Guid.NewGuid(),
			DepositedAmount: 0M,
			IdentityAuthentication: false,
			PhoneAuthentication: false,
			PlayerCode: "ThisIsPlayerCode",
			VipLevelId: null);
		var brandPayment = new BrandPaymentInfo
		{
			Id = Guid.NewGuid(),
			AuthenticateIdentity = true,
			AuthenticatePhone = false,
			ChannelCode = "TestChannelCode",
			ChannelId = Guid.NewGuid(),
			ChannelName = "TestChannel",
			ConditionMatchType = ConditionMatchType.MatchAll,
			DepositedAmountLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			Enabled = true,
			IsAllPlayers = true,
			MethodId = Guid.NewGuid(),
			PaymentId = Guid.NewGuid(),
			PaymentMethodChannelId = Guid.NewGuid(),
			TransferLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			TransferType = TransferType.Deposit
		};

		var sut = new IdentityCondition(brandPayment.AuthenticateIdentity);

		var actual = sut.IsValid(brandPayment, playerInfo);

		ClassicAssert.IsFalse(actual);
	}

	[Test]
	public void Authenticate_Identity_Should_Success()
	{
		var playerInfo = new PlayerInfo(
			BrandId: Guid.NewGuid(),
			DepositedAmount: 0M,
			IdentityAuthentication: false,
			PhoneAuthentication: false,
			PlayerCode: "ThisIsPlayerCode",
			VipLevelId: null);

		var brandPayment = new BrandPaymentInfo
		{
			Id = Guid.NewGuid(),
			AuthenticateIdentity = false,
			AuthenticatePhone = false,
			ChannelCode = "TestChannelCode",
			ChannelId = Guid.NewGuid(),
			ChannelName = "TestChannel",
			ConditionMatchType = ConditionMatchType.MatchAll,
			DepositedAmountLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			Enabled = true,
			IsAllPlayers = true,
			MethodId = Guid.NewGuid(),
			PaymentId = Guid.NewGuid(),
			PaymentMethodChannelId = Guid.NewGuid(),
			TransferLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			TransferType = TransferType.Deposit
		};

		var sut = new IdentityCondition(brandPayment.AuthenticateIdentity);

		var actual = sut.IsValid(brandPayment, playerInfo);

		ClassicAssert.IsTrue(actual);
	}

	[Test]
	public void Authenticate_Phone_Should_Always_Valid()
	{
		var playerInfo = new PlayerInfo(
			BrandId: Guid.NewGuid(),
			DepositedAmount: 0M,
			IdentityAuthentication: false,
			PhoneAuthentication: false,
			PlayerCode: "ThisIsPlayerCode",
			VipLevelId: null);

		var brandPayment = new BrandPaymentInfo
		{
			Id = Guid.NewGuid(),
			AuthenticateIdentity = false,
			AuthenticatePhone = false,
			ChannelCode = "TestChannelCode",
			ChannelId = Guid.NewGuid(),
			ChannelName = "TestChannel",
			ConditionMatchType = ConditionMatchType.MatchAll,
			DepositedAmountLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			Enabled = true,
			IsAllPlayers = true,
			MethodId = Guid.NewGuid(),
			PaymentId = Guid.NewGuid(),
			PaymentMethodChannelId = Guid.NewGuid(),
			TransferLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			TransferType = TransferType.Deposit
		};

		var sut = new PhoneCondition(brandPayment.AuthenticatePhone);

		var actual = sut.IsValid(brandPayment, playerInfo);

		ClassicAssert.IsTrue(actual);
	}

	[Test]
	public void Authenticate_Phone_Should_Success()
	{
		var playerInfo = new PlayerInfo(
			BrandId: Guid.NewGuid(),
			DepositedAmount: 0M,
			IdentityAuthentication: false,
			PhoneAuthentication: false,
			PlayerCode: "ThisIsPlayerCode",
			VipLevelId: null);

		var brandPayment = new BrandPaymentInfo
		{
			Id = Guid.NewGuid(),
			AuthenticateIdentity = false,
			AuthenticatePhone = false,
			ChannelCode = "TestChannelCode",
			ChannelId = Guid.NewGuid(),
			ChannelName = "TestChannel",
			ConditionMatchType = ConditionMatchType.MatchAll,
			DepositedAmountLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			Enabled = true,
			IsAllPlayers = true,
			MethodId = Guid.NewGuid(),
			PaymentId = Guid.NewGuid(),
			PaymentMethodChannelId = Guid.NewGuid(),
			TransferLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			TransferType = TransferType.Deposit
		};

		var sut = new PhoneCondition(brandPayment.AuthenticatePhone);

		var actual = sut.IsValid(brandPayment, playerInfo);

		ClassicAssert.IsTrue(actual);
	}

	[Test]
	public void BlockList_Is_First_Check()
	{
		var playerInfo = new PlayerInfo(
			BrandId: Guid.NewGuid(),
			DepositedAmount: 0M,
			IdentityAuthentication: false,
			PhoneAuthentication: false,
			PlayerCode: "ThisIsPlayerCode",
			VipLevelId: null);

		var brandPayment = new BrandPaymentInfo
		{
			Id = Guid.NewGuid(),
			AuthenticateIdentity = false,
			AuthenticatePhone = false,
			ChannelCode = "TestChannelCode",
			ChannelId = Guid.NewGuid(),
			ChannelName = "TestChannel",
			ConditionMatchType = ConditionMatchType.MatchAll,
			DepositedAmountLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			Enabled = true,
			IsAllPlayers = true,
			MethodId = Guid.NewGuid(),
			PaymentId = Guid.NewGuid(),
			PaymentMethodChannelId = Guid.NewGuid(),
			TransferLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			TransferType = TransferType.Deposit
		};

		var fakeCondition = new Mock<IBrandPaymentCondition>();

		var sut = new BrandPaymentFilterCondition(
			new BlockListCondition(new[] { brandPayment.Id }),
			new WhiteListCondition(new[] { brandPayment.Id }),
			new ShowOnAllPlayersCondition(),
			fakeCondition.Object);

		var actual = sut.IsValid(brandPayment, playerInfo);

		ClassicAssert.IsFalse(actual);
	}

	[Test]
	public void BrandPayment_Set_AllPlayers_False_Will_Invalid()
	{
		var playerInfo = new PlayerInfo(
			BrandId: Guid.NewGuid(),
			DepositedAmount: 0M,
			IdentityAuthentication: false,
			PhoneAuthentication: false,
			PlayerCode: "ThisIsPlayerCode",
			VipLevelId: null);

		var brandPayment = new BrandPaymentInfo
		{
			Id = Guid.NewGuid(),
			AuthenticateIdentity = false,
			AuthenticatePhone = false,
			ChannelCode = "TestChannelCode",
			ChannelId = Guid.NewGuid(),
			ChannelName = "TestChannel",
			ConditionMatchType = ConditionMatchType.MatchAll,
			DepositedAmountLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			Enabled = true,
			IsAllPlayers = false,
			MethodId = Guid.NewGuid(),
			PaymentId = Guid.NewGuid(),
			PaymentMethodChannelId = Guid.NewGuid(),
			TransferLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			TransferType = TransferType.Deposit
		};

		var sut = new ShowOnAllPlayersCondition();

		var actual = sut.IsValid(brandPayment, playerInfo);

		ClassicAssert.IsFalse(actual);
	}

	[Test]
	public void DepositedAmountCondition_Should_Success()
	{
		var playerInfo = new PlayerInfo(
			BrandId: Guid.NewGuid(),
			DepositedAmount: 50M,
			IdentityAuthentication: false,
			PhoneAuthentication: false,
			PlayerCode: "ThisIsPlayerCode",
			VipLevelId: null);

		var brandPayment = new BrandPaymentInfo
		{
			Id = Guid.NewGuid(),
			AuthenticateIdentity = false,
			AuthenticatePhone = false,
			ChannelCode = "TestChannelCode",
			ChannelId = Guid.NewGuid(),
			ChannelName = "TestChannel",
			ConditionMatchType = ConditionMatchType.MatchAll,
			DepositedAmountLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			Enabled = true,
			IsAllPlayers = true,
			MethodId = Guid.NewGuid(),
			PaymentId = Guid.NewGuid(),
			PaymentMethodChannelId = Guid.NewGuid(),
			TransferLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			TransferType = TransferType.Deposit
		};

		var sut = new DepositedAmountCondition(brandPayment.DepositedAmountLimit);

		var actual = sut.IsValid(brandPayment, playerInfo);

		ClassicAssert.IsTrue(actual);
	}

	[Test]
	public void MatchAll_And_AllPlayers_Should_Pass()
	{
		var playerInfo = new PlayerInfo(
			BrandId: Guid.NewGuid(),
			DepositedAmount: 0M,
			IdentityAuthentication: false,
			PhoneAuthentication: false,
			PlayerCode: "ThisIsPlayerCode",
			VipLevelId: null);

		var brandPayment = new BrandPaymentInfo
		{
			Id = Guid.NewGuid(),
			AuthenticateIdentity = false,
			AuthenticatePhone = false,
			ChannelCode = "TestChannelCode",
			ChannelId = Guid.NewGuid(),
			ChannelName = "TestChannel",
			ConditionMatchType = ConditionMatchType.MatchAll,
			DepositedAmountLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			Enabled = true,
			IsAllPlayers = true,
			MethodId = Guid.NewGuid(),
			PaymentId = Guid.NewGuid(),
			PaymentMethodChannelId = Guid.NewGuid(),
			TransferLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			TransferType = TransferType.Deposit
		};

		var fakeCondition1 = new Mock<IBrandPaymentCondition>();

		_ = fakeCondition1
			.Setup(fake => fake.IsValid(
				It.IsAny<BrandPaymentInfo>(),
				It.IsAny<PlayerInfo>()))
			.Returns(true);

		var fakeCondition2 = new Mock<IBrandPaymentCondition>();

		_ = fakeCondition2
			.Setup(fake => fake.IsValid(
				It.IsAny<BrandPaymentInfo>(),
				It.IsAny<PlayerInfo>()))
			.Returns(true);

		var fakeCondition3 = new Mock<IBrandPaymentCondition>();

		_ = fakeCondition3
			.Setup(fake => fake.IsValid(
				It.IsAny<BrandPaymentInfo>(),
				It.IsAny<PlayerInfo>()))
			.Returns(true);

		var sut = new BrandPaymentFilterCondition(
			new BlockListCondition(Array.Empty<Guid>()),
			new WhiteListCondition(Array.Empty<Guid>()),
			new ShowOnAllPlayersCondition(),
			new MatchAllCondition(
				fakeCondition1.Object,
				fakeCondition2.Object,
				fakeCondition3.Object));

		var actual = sut.IsValid(brandPayment, playerInfo);

		ClassicAssert.IsTrue(actual);
	}

	[Test]
	public void Player_In_BlockList_Should_Invalid()
	{
		var playerInfo = new PlayerInfo(
			BrandId: Guid.NewGuid(),
			DepositedAmount: 0M,
			IdentityAuthentication: false,
			PhoneAuthentication: false,
			PlayerCode: "ThisIsPlayerCode",
			VipLevelId: null);

		var brandPayment = new BrandPaymentInfo
		{
			Id = Guid.NewGuid(),
			AuthenticateIdentity = false,
			AuthenticatePhone = false,
			ChannelCode = "TestChannelCode",
			ChannelId = Guid.NewGuid(),
			ChannelName = "TestChannel",
			ConditionMatchType = ConditionMatchType.MatchAll,
			DepositedAmountLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			Enabled = true,
			IsAllPlayers = true,
			MethodId = Guid.NewGuid(),
			PaymentId = Guid.NewGuid(),
			PaymentMethodChannelId = Guid.NewGuid(),
			TransferLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			TransferType = TransferType.Deposit
		};

		var sut = new BlockListCondition(new[] { brandPayment.Id });

		var actual = sut.IsValid(brandPayment, playerInfo);

		ClassicAssert.IsFalse(actual);
	}

	[Test]
	public void Player_In_WhiteList_Should_Pass()
	{
		var playerInfo = new PlayerInfo(
			BrandId: Guid.NewGuid(),
			DepositedAmount: 0M,
			IdentityAuthentication: false,
			PhoneAuthentication: false,
			PlayerCode: "ThisIsPlayerCode",
			VipLevelId: null);

		var brandPayment = new BrandPaymentInfo
		{
			Id = Guid.NewGuid(),
			AuthenticateIdentity = false,
			AuthenticatePhone = false,
			ChannelCode = "TestChannelCode",
			ChannelId = Guid.NewGuid(),
			ChannelName = "TestChannel",
			ConditionMatchType = ConditionMatchType.MatchAll,
			DepositedAmountLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			Enabled = true,
			IsAllPlayers = true,
			MethodId = Guid.NewGuid(),
			PaymentId = Guid.NewGuid(),
			PaymentMethodChannelId = Guid.NewGuid(),
			TransferLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			TransferType = TransferType.Deposit
		};

		var sut = new WhiteListCondition(new[] { brandPayment.Id });

		var actual = sut.IsValid(brandPayment, playerInfo);

		ClassicAssert.IsTrue(actual);
	}

	[Test]
	public void Player_In_WhiteList_Will_Ignore_After_Conditions()
	{
		var playerInfo = new PlayerInfo(
			BrandId: Guid.NewGuid(),
			DepositedAmount: 0M,
			IdentityAuthentication: false,
			PhoneAuthentication: false,
			PlayerCode: "ThisIsPlayerCode",
			VipLevelId: null);

		var brandPayment = new BrandPaymentInfo
		{
			Id = Guid.NewGuid(),
			AuthenticateIdentity = false,
			AuthenticatePhone = false,
			ChannelCode = "TestChannelCode",
			ChannelId = Guid.NewGuid(),
			ChannelName = "TestChannel",
			ConditionMatchType = ConditionMatchType.MatchAll,
			DepositedAmountLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			Enabled = true,
			IsAllPlayers = true,
			MethodId = Guid.NewGuid(),
			PaymentId = Guid.NewGuid(),
			PaymentMethodChannelId = Guid.NewGuid(),
			TransferLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			TransferType = TransferType.Deposit
		};

		var fakeCondition = new Mock<IBrandPaymentCondition>();

		var sut = new BrandPaymentFilterCondition(
			new BlockListCondition(Array.Empty<Guid>()),
			new WhiteListCondition(new[] { brandPayment.Id }),
			new ShowOnAllPlayersCondition(),
			fakeCondition.Object);

		var actual = sut.IsValid(brandPayment, playerInfo);

		ClassicAssert.IsTrue(actual);
	}

	[Test]
	public void SomeOneCondition_Should_Any_One_SubCondition_Pass()
	{
		var playerInfo = new PlayerInfo(
			BrandId: Guid.NewGuid(),
			DepositedAmount: 0M,
			IdentityAuthentication: false,
			PhoneAuthentication: false,
			PlayerCode: "ThisIsPlayerCode",
			VipLevelId: null);

		var brandPayment = new BrandPaymentInfo
		{
			Id = Guid.NewGuid(),
			AuthenticateIdentity = false,
			AuthenticatePhone = false,
			ChannelCode = "TestChannelCode",
			ChannelId = Guid.NewGuid(),
			ChannelName = "TestChannel",
			ConditionMatchType = ConditionMatchType.MatchAll,
			DepositedAmountLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			Enabled = true,
			IsAllPlayers = true,
			MethodId = Guid.NewGuid(),
			PaymentId = Guid.NewGuid(),
			PaymentMethodChannelId = Guid.NewGuid(),
			TransferLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			TransferType = TransferType.Deposit
		};

		var fakeCondition1 = new Mock<IBrandPaymentCondition>();

		_ = fakeCondition1
			.Setup(fake => fake.IsValid(
				It.IsAny<BrandPaymentInfo>(),
				It.IsAny<PlayerInfo>()))
			.Returns(false);

		var fakeCondition2 = new Mock<IBrandPaymentCondition>();

		_ = fakeCondition2
			.Setup(fake => fake.IsValid(
				It.IsAny<BrandPaymentInfo>(),
				It.IsAny<PlayerInfo>()))
			.Returns(true);

		var fakeCondition3 = new Mock<IBrandPaymentCondition>();

		_ = fakeCondition3
			.Setup(fake => fake.IsValid(
				It.IsAny<BrandPaymentInfo>(),
				It.IsAny<PlayerInfo>()))
			.Returns(false);

		var sut = new MatchOneCondition(
			fakeCondition1.Object,
			fakeCondition2.Object,
			fakeCondition3.Object);

		var actual = sut.IsValid(brandPayment, playerInfo);

		ClassicAssert.IsTrue(actual);
	}

	[Test]
	public void VipCondition_Is_Empty_Should_Invalid()
	{
		var playerInfo = new PlayerInfo(
			BrandId: Guid.NewGuid(),
			DepositedAmount: 0M,
			IdentityAuthentication: false,
			PhoneAuthentication: false,
			PlayerCode: "ThisIsPlayerCode",
			VipLevelId: null);

		var brandPayment = new BrandPaymentInfo
		{
			Id = Guid.NewGuid(),
			AuthenticateIdentity = false,
			AuthenticatePhone = false,
			ChannelCode = "TestChannelCode",
			ChannelId = Guid.NewGuid(),
			ChannelName = "TestChannel",
			ConditionMatchType = ConditionMatchType.MatchAll,
			DepositedAmountLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			Enabled = true,
			IsAllPlayers = true,
			MethodId = Guid.NewGuid(),
			PaymentId = Guid.NewGuid(),
			PaymentMethodChannelId = Guid.NewGuid(),
			TransferLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			TransferType = TransferType.Deposit
		};

		var sut = new VipCondition(Array.Empty<Guid>());

		var actual = sut.IsValid(brandPayment, playerInfo);

		ClassicAssert.IsFalse(actual);
	}

	[Test]
	public void VipCondition_Should_Success()
	{
		var playerInfo = new PlayerInfo(
			BrandId: Guid.NewGuid(),
			DepositedAmount: 0M,
			IdentityAuthentication: false,
			PhoneAuthentication: false,
			PlayerCode: "ThisIsPlayerCode",
			VipLevelId: Guid.NewGuid());

		var brandPayment = new BrandPaymentInfo
		{
			Id = Guid.NewGuid(),
			AuthenticateIdentity = false,
			AuthenticatePhone = false,
			ChannelCode = "TestChannelCode",
			ChannelId = Guid.NewGuid(),
			ChannelName = "TestChannel",
			ConditionMatchType = ConditionMatchType.MatchAll,
			DepositedAmountLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			Enabled = true,
			IsAllPlayers = true,
			MethodId = Guid.NewGuid(),
			PaymentId = Guid.NewGuid(),
			PaymentMethodChannelId = Guid.NewGuid(),
			TransferLimit = new ValueRange<decimal>
			{
				From = 10M,
				To = 100M
			},
			TransferType = TransferType.Deposit
		};

		var sut = new VipCondition(new[] { playerInfo.VipLevelId!.Value });

		var actual = sut.IsValid(brandPayment, playerInfo);

		ClassicAssert.IsTrue(actual);
	}
}
