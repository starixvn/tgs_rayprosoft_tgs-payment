Feature: deposit=

Scenario: Create deposit form
	Given I will create deposit form
	When call payment api
	Then deposit form created

Scenario: Get deposit detail
	Given I have a brand id {FDB0044E-2271-45DC-91B0-B6E1CAAAAE61}
	And a form id {D7A90C7A-910A-4C43-A047-8D02F2BB1567}
	When call get deposit form detail api
	Then return detail data

Scenario: Get deposit detail by form id
	Given I have a form id {D7A90C7A-910A-4C43-A047-8D02F2BB1567}
	When call get deposit form detail api
	Then return detail data

Scenario: Query deposit forms
	Given I Query deposit forms
	When call get deposit forms api
	Then return deposit forms list

Scenario: Review a deposit form
	Given I review a deposit form
	When approve form
	Then form status change to approved

Scenario: Change a deposit form status
	Given I have a deposit form
	When I change status to "WaitPayment"
	Then form status change to "WaitPayment"

Scenario: Save thirdparty order no to deposit form
	Given I get thirdparty order no "AH16322770975542"
	When I call save api
	Then form thirdparty_order_no should be "AH16322770975542"