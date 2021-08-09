Feature: payment

Scenario: Get all payments
	Given I will list all payments
	When call get all payments api
	Then return all paymnets data

Scenario: Get one payment
	Given I has a payment id {A93D94EA-8067-4006-A1B3-0A44FDFE9225}
	When call get payment api
	Then return payment data

Scenario: Get all payment methods
	Given I will list all payment methods
	When call get all payment methods api
	Then return all payment methods data

Scenario: Get all payment channels
	Given I will list all payment channels
	When call get all payment channels
	Then return all payment channels data

Scenario: Get all payment_method_channels
	Given I will list all payment_method_channels
	When call get all payment_method_channels api
	Then return all payment_method_channels data

Scenario: Create a payment
	Given I will create payment info
	When call create payment api
	Then the payment info created

Scenario: Create a payment channel
	Given I will create payment channel info
	When call create payment channel api
	Then the payment channel created

Scenario: Create a payment method
	Given I will create payment method info
	When call create payment method api
	Then the payment channel created