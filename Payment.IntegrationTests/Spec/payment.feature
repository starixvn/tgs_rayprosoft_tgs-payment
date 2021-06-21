Feature: payment

Scenario: Get all payments
	Given I will list all payments
	When call get all payments api
	Then return all paymnets data

Scenario: Get one payment
	Given I has a payment id {A93D94EA-8067-4006-A1B3-0A44FDFE9225}
	When call get payment api
	Then return payment data