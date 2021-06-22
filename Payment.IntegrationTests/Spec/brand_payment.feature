Feature: BrandPayment

Scenario: Get one brand payment
	Given I has a brand payment id {AEFAB3EE-777C-425A-88CA-D61033DAC142}
	When call get brand payment api
	Then return brand payment data