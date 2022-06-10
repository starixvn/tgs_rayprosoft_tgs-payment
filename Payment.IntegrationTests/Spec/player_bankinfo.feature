Feature: Player BankInfo

Scenario: List player bank infos
	Given I has a playerCode "4ucmqplxb029"
	When call get bank infos api
	Then return bank info data by player

@CreateBankInfo
Scenario: Create player bank info
	Given I will create bank info
	And playerCode is "4ucmqplxb029"
	And brandId is {07FF2BF7-49DC-4FEC-A776-A5F63B0FE992}
	When call create bank info api
	Then the bank info created

@CreateBankInfo
Scenario: Create player bank info with modify user
	Given I will create bank info
	And playerCode is "4ucmqplxb029"
	And modifyUser is "bo_admin"
	And brandId is {07FF2BF7-49DC-4FEC-A776-A5F63B0FE992}
	When call create bank info api with modify user
	Then the bank info created

@DeleteBankInfo
Scenario: Delete player bank info
	Given I will delete bank info
	And playerCode is "4ucmqplxb029"
	And bank info id is {4F4458F5-126A-4B2F-91C6-B905C6E0C1CB}
	When call delete bank info api
	Then the bank info deleted