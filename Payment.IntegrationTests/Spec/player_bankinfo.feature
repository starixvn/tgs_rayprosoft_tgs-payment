Feature: Player BankInfo

Scenario: List player bank infos
	Given I has a playerCode "4ucmqplxb029"
	When call get bank infos api
	Then return bank info data by player