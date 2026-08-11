SonarProvider:
SonarQube Cloud

SonarProjectKey:
rodri-oliveira-dev_Dapper.TypedParameters

SonarOrganizationKey:
rodri-oliveira-dev

GitHubRepository:
rodri-oliveira-dev/Dapper.TypedParameters

RepositorySecret:
SONAR_TOKEN

RepositorySecretValue:
NOT STORED

ExpectedNewCodeCoverage:
80%

QualityGate:
Sonar way

QualityGateVerification:
Public SonarCloud API confirmed built-in Sonar way with new code coverage,
new reliability rating, new security rating, new maintainability rating,
new security hotspots reviewed, and new duplicated lines density conditions.

GitHubRequiredCheck:
SonarQube Cloud

SonarDecorationCheck:
VERIFY AFTER FIRST PR ANALYSIS

BranchProtection:
PENDING HUMAN VERIFICATION

HumanVerification:
PENDING

ManualActionsAfterMerge:
1. Push the branch and open a pull request.
2. Observe the first SonarQube Cloud analysis.
3. Confirm that the project still uses Sonar way or an equivalent Quality Gate
   with New Code Coverage >= 80%.
4. Configure the main branch ruleset or branch protection to require the
   `SonarQube Cloud` status check.
5. Consider whether the SonarQube Cloud decoration check should also be
   required.
6. Confirm that a red Quality Gate blocks merge.
