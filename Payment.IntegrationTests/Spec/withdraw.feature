Feature: withdraw

Scenario: Create withdraw form
	Given I will create withdraw form
	When call create withdraw form api
	Then withdraw form created

Scenario: Change a withdraw form status
	Given I have a withdraw form
	When I change status to "Approved"
	Then form status change to "Approved"