# Quality decisions

## Accepted

1. Sonar provider: SonarQube Cloud.
2. Project key: `rodri-oliveira-dev_Dapper.TypedParameters`.
3. Organization key: `rodri-oliveira-dev`.
4. Authentication: GitHub repository Actions secret `SONAR_TOKEN`.
5. Secret value: never stored in repository.
6. Scanner: SonarScanner for .NET.
7. Scanner version: `11.2.1`.
8. Official documentation used:
   - https://docs.sonarsource.com/sonarqube-cloud/analyzing-source-code/scanners/sonarscanner-for-dotnet/installing
   - https://docs.sonarsource.com/sonarqube-cloud/analyzing-source-code/scanners/sonarscanner-for-dotnet/using
   - https://docs.sonarsource.com/sonarqube-cloud/analyzing-source-code/test-coverage/dotnet-test-coverage
   - https://docs.sonarsource.com/sonarqube-cloud/analyzing-source-code/test-coverage/test-coverage-parameters
   - https://docs.sonarsource.com/sonarqube-cloud/analyzing-source-code/ci-based-analysis/github-actions-for-sonarcloud
   - https://docs.sonarsource.com/sonarqube-cloud/analyzing-source-code/scanners/scanner-environment/verifying-code-checkout-step
   - https://docs.sonarsource.com/sonarqube-cloud/analyzing-source-code/analysis-parameters/parameters-not-settable-in-ui
   - https://docs.sonarsource.com/sonarqube-cloud/analyzing-source-code/scanners/scanner-environment/general-requirements
9. Scanner runtime: GitHub Actions configures Java 21 explicitly for the Sonar
   job.
10. Coverage producer: Coverlet through `coverlet.collector`.
11. Coverage format for Sonar: OpenCover.
12. Coverage import property: `sonar.cs.opencover.reportsPaths`.
13. Existing Cobertura artifacts in the validation matrix are preserved.
14. Quality strategy: Clean as You Code.
15. Minimum New Code Coverage: 80%.
16. Enforcement authority: SonarQube Cloud Quality Gate.
17. Quality Gate enforcement: required.
18. Quality Gate wait: enabled.
19. Quality Gate timeout: 300 seconds.
20. Pull Request gate failure: fails GitHub Actions job.
21. Required status check: `SonarQube Cloud`.
22. Canonical TFM for Sonar coverage: `net8.0`, because production code has no
    TFM-specific compilation and the existing CI matrix continues validating
    both `net8.0` and `net10.0`.
23. Fork PR policy: never expose `SONAR_TOKEN` to untrusted fork code. Fork PRs
    cannot run authenticated Sonar analysis through `pull_request` secrets; the
    job fails early when the secret is unavailable.
24. Q02 Quality Gate root cause: the first Sonar analysis treated historical
    repository content as New Code under the inherited `previous_version` New
    Code definition, exposing a real GitHub Actions dependency pinning
    vulnerability and duplicated validation-script lines to the Clean as You
    Code gate.
25. Q02 Quality Gate policy: unchanged.
26. Q02 New Code Coverage threshold: unchanged at 80%.
27. Q02 Quality Gate wait: enabled.
28. Q02 `TableValuedSqlParameter.SqlDbType` S2325 decision: valid but
    intentionally accepted with targeted source suppression because the member
    is frozen public instance metadata in the 1.0 candidate API.
29. Q02 `TableValuedSqlParameter.Direction` S2325 decision: valid but
    intentionally accepted with targeted source suppression because the member
    is frozen public instance metadata in the 1.0 candidate API.
30. Q02 public API breaking change: no.
31. Q02 RC compatibility: preserved.
32. Q02 external Sonar configuration required: no repository-side remediation
    requires changing the Quality Gate, coverage threshold, Quality Gate wait,
    Security Hotspots, or New Code definition remotely.
