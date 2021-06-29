Feature: agent_bankinfo

Scenario: List agent bank infos
	Given I has a agentId {EC0F5010-F77A-4C46-9D40-6014F936677D}
	When call get bank infos api
	Then return bank info data by angent

Scenario: Create agent bank info
	Given I will create bank info
	And agentId is {EC0F5010-F77A-4C46-9D40-6014F936677D}
	And brandId is {D8470AB8-D8FB-453A-979F-3CE373E6E2A1}
	And agentName is "TestAgent"
	When call create bank info api
	Then the bank info created

Scenario: Delete agent bank info
	Given I will delete bank info
	And agentId is {EC0F5010-F77A-4C46-9D40-6014F936677D}
	And bank info id is {4419D5D2-1142-41B5-B0ED-37C05C52D48F}
	When call delete bank info api
	Then the bank info deleted