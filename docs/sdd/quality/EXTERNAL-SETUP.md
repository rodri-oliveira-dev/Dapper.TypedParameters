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

NewCodeDefinition:
previous_version

NewCodeDefinitionSource:
Public SonarCloud settings API for `sonar.leak.period`.

NewCodeBaseline:
No previous project version baseline was available in the first recorded public
analysis. The failed analysis recorded project version `not provided` and issue
search with `inNewCodePeriod=true` returned all 13 unresolved issues.

NewCodeDefinitionChangeRequired:
No for Q02. The repository-side remediation fixes the concrete issues exposed by
the first analysis without weakening the Clean as You Code policy. Do not change
the New Code definition remotely unless a later CI run proves the remaining
baseline is still inconsistent with Sonar adoption policy.

SecurityHotspotReview:
NOT REQUIRED FOR Q02

SecurityHotspotReviewReason:
Public SonarCloud hotspot search returned 0 hotspots for `main`.

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
