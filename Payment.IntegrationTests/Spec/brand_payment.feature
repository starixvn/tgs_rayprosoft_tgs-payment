Feature: BrandPayment

Scenario: Get one brand payment
	Given I has a brand payment id {AEFAB3EE-777C-425A-88CA-D61033DAC142}
	When call get brand payment api
	Then return brand payment data

Scenario: Get brand payments by brand id
	Given I will list brand payments
	And brand id is {234E5422-30F9-4A1E-A0A1-27C9FE030F9F}
	When call get list brand payments api
	Then return brand paymnets data

Scenario: Get brand payment acls
	Given I will get brand payment acls
	And playerCode is "4ucmqplxb029"
	And brand payment id is {BD0CC240-AD31-475B-81FB-C3CC06A9F824}、{50C7A6E2-E983-4510-924A-547BEB402D05}
	When call get brand payment acl api
	Then return acl data

Scenario: Get brand payment vips
	Given I will get brand payment vip settings
	And brand payment id is {BD0CC240-AD31-475B-81FB-C3CC06A9F824}、{50C7A6E2-E983-4510-924A-547BEB402D05}
	When call get brand payment vip api
	Then return vip data