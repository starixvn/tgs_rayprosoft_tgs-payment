Feature: Player BankInfo

Scenario: List player bank infos
	Given I has a playerCode "4ucmqplxb029"
	When call get bank infos api
	Then return bank info data by player

Scenario: Create player bank info
	Given I will create bank info
	And playerCode is "4ucmqplxb029"
	And brandId is {07FF2BF7-49DC-4FEC-A776-A5F63B0FE992}
	When call create bank info api
	Then the bank info created