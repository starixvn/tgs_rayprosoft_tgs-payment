Feature: deposit

Scenario: Create deposit form
	Given I will create deposit form
	When call payment api
	Then deposit form created