Feature: withdraw

Scenario: Create withdraw form
	Given I will create withdraw form
	When call create withdraw form api
	Then withdraw form created

Scenario: Change a withdraw form status
	Given I have a withdraw form
	When I change status to "Approved"
	Then form status change to "Approved"

Scenario: Review a withdraw form
	Given I review a withdraw form
	When approve form
	Then form status change to "Approved"

Scenario: Get withdraw detail
	Given I have a brand id {7C27560A-9B64-437F-93B5-C6F16E8EF14E}
	And a form id {EF74B68B-EB8F-4031-BFBA-3AEC7CD7C0F3}
	When call get withdraw form detail api
	Then return detail data

Scenario: Query withdraw forms
	Given I Query withdraw forms
	When call get withdraw forms api
	Then return withdraw forms list