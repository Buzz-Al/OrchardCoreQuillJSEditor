# CI/CD Integration Guide

This guide shows how to integrate tests into your Azure DevOps CI/CD pipeline.

## Current Pipeline Structure

Your existing pipeline (`.azuredevops/pipelines/pipeline.yml`):
```yaml
jobs:
- template: jobs/test.yml      # Currently just builds
- template: jobs/publish.yml
```

## Updated Pipeline Structure

```yaml
trigger:
  batch: true
  branches:
    include:
      - develop
      - main
  tags:
    include:
      - '*'

variables:
  BuildConfiguration: Release

stages:
- stage: Build
  jobs:
  - template: jobs/build.yml

- stage: UnitTests
  dependsOn: Build
  jobs:
  - template: jobs/unit-tests.yml

- stage: E2ETests
  dependsOn: Build
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
  jobs:
  - template: jobs/e2e-tests.yml

- stage: Publish
  dependsOn:
  - UnitTests
  - E2ETests
  condition: and(succeeded(), startsWith(variables['Build.SourceBranch'], 'refs/tags/'))
  jobs:
  - template: jobs/publish.yml
```

## Job Templates

### Build Job

**File:** `.azuredevops/pipelines/jobs/build.yml`

```yaml
jobs:
- job: build
  displayName: 'Build'
  pool:
    vmImage: windows-latest
  steps:
  - checkout: self
    fetchDepth: 0

  - task: gitversion/setup@0
    displayName: Install GitVersion
    inputs:
      versionSpec: 5.x

  - task: gitversion/execute@0
    displayName: Determine Version

  - task: DotNetCoreCLI@2
    displayName: Restore
    inputs:
      command: restore
      projects: '**/*.csproj'
      selectOrConfig: config
      nugetConfigPath: 'NuGet.config'

  - task: DotNetCoreCLI@2
    displayName: Build
    inputs:
      projects: '**/*.csproj'
      arguments: '--configuration $(BuildConfiguration) --no-restore'

  - task: DotNetCoreCLI@2
    displayName: Pack
    inputs:
      command: pack
      searchPatternPack: src/**/*.csproj
      configuration: $(BuildConfiguration)
      versioningScheme: byEnvVar
      versionEnvVar: GitVersion.NuGetVersionV2

  - task: PublishBuildArtifacts@1
    displayName: Publish Build Artifacts
    inputs:
      PathtoPublish: '$(Build.ArtifactStagingDirectory)'
      ArtifactName: 'packages'
```

### Unit Tests Job

**File:** `.azuredevops/pipelines/jobs/unit-tests.yml`

```yaml
jobs:
- job: unit_tests
  displayName: 'Unit & Integration Tests'
  pool:
    vmImage: windows-latest
  steps:
  - checkout: self

  - task: DotNetCoreCLI@2
    displayName: Restore
    inputs:
      command: restore
      projects: '**/*.csproj'

  - task: DotNetCoreCLI@2
    displayName: Build
    inputs:
      projects: '**/*.csproj'
      arguments: '--configuration $(BuildConfiguration) --no-restore'

  - task: DotNetCoreCLI@2
    displayName: Run Tests
    inputs:
      command: test
      projects: 'tests/**/*.Tests.csproj'
      arguments: '--configuration $(BuildConfiguration) --no-build --collect:"XPlat Code Coverage" --logger trx'
      publishTestResults: false

  - task: PublishTestResults@2
    displayName: Publish Test Results
    condition: always()
    inputs:
      testResultsFormat: 'VSTest'
      testResultsFiles: '**/*.trx'
      mergeTestResults: true
      testRunTitle: 'Unit & Integration Tests'

  - task: PublishCodeCoverageResults@1
    displayName: Publish Code Coverage
    condition: always()
    inputs:
      codeCoverageTool: 'Cobertura'
      summaryFileLocation: '$(Agent.TempDirectory)/**/*coverage.cobertura.xml'
      failIfCoverageEmpty: false

  - task: BuildQualityChecks@8
    displayName: 'Check Code Coverage >= 70%'
    inputs:
      checkCoverage: true
      coverageFailOption: 'fixed'
      coverageType: 'lines'
      coverageThreshold: '70'
```

### E2E Tests Job

**File:** `.azuredevops/pipelines/jobs/e2e-tests.yml`

```yaml
jobs:
- job: e2e_tests
  displayName: 'E2E Tests (Playwright)'
  pool:
    vmImage: ubuntu-latest
  timeoutInMinutes: 20
  steps:
  - checkout: self

  # Build Sample.Web app
  - task: DotNetCoreCLI@2
    displayName: Restore Sample App
    inputs:
      command: restore
      projects: 'samples/**/*.csproj'

  - task: DotNetCoreCLI@2
    displayName: Build Sample App
    inputs:
      projects: 'samples/**/*.csproj'
      arguments: '--configuration Release --no-restore'

  # Start Sample.Web in background
  - bash: |
      cd samples/Buzz.OrchardCore.Quilljs.Sample.Web
      dotnet run --no-build --configuration Release &

      # Wait for app to be ready (max 60 seconds)
      timeout=60
      until curl -s http://localhost:5000 > /dev/null; do
        sleep 1
        timeout=$((timeout - 1))
        if [ $timeout -le 0 ]; then
          echo "ERROR: Timeout waiting for app to start"
          exit 1
        fi
        echo "Waiting for app to start... ($timeout seconds remaining)"
      done
      echo "✓ App is ready!"
    displayName: Start Sample.Web App

  # Install Node.js
  - task: NodeTool@0
    displayName: Install Node.js
    inputs:
      versionSpec: '20.x'

  # Install Playwright
  - bash: |
      cd tests/Buzz.OrchardCore.Quilljs.UITests
      npm ci
      npx playwright install --with-deps chromium
    displayName: Install Playwright Dependencies

  # Run Playwright tests
  - bash: |
      cd tests/Buzz.OrchardCore.Quilljs.UITests
      npx playwright test --reporter=html,junit
    displayName: Run Playwright Tests
    env:
      BASE_URL: http://localhost:5000

  # Publish test results
  - task: PublishTestResults@2
    condition: always()
    inputs:
      testResultsFormat: 'JUnit'
      testResultsFiles: 'tests/Buzz.OrchardCore.Quilljs.UITests/test-results/junit.xml'
      testRunTitle: 'Playwright E2E Tests'

  # Publish Playwright HTML report
  - task: PublishBuildArtifacts@1
    condition: always()
    inputs:
      pathToPublish: 'tests/Buzz.OrchardCore.Quilljs.UITests/playwright-report'
      artifactName: 'playwright-report'

  # Publish screenshots on failure
  - task: PublishBuildArtifacts@1
    condition: failed()
    inputs:
      pathToPublish: 'tests/Buzz.OrchardCore.Quilljs.UITests/test-results'
      artifactName: 'playwright-test-results'
```

## Test Result Dashboard

Azure DevOps will automatically show:

- **Test Summary** - Pass/fail counts, duration
- **Code Coverage** - Line/branch coverage percentages
- **Trends** - Test results over time
- **Failed Tests** - Stack traces and error messages

## PR Validation

Add PR trigger to run tests on pull requests:

```yaml
# .azuredevops/pipelines/pr-validation.yml
trigger: none

pr:
  branches:
    include:
    - main
    - develop

variables:
  BuildConfiguration: Release

stages:
- stage: PRValidation
  displayName: 'PR Validation'
  jobs:
  - template: jobs/build.yml
  - template: jobs/unit-tests.yml
  # Skip E2E tests on PR for speed (run on merge to main)
```

## Local Testing Before Push

```bash
# Run what CI will run
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build

# If all pass, push with confidence!
git push
```

## Troubleshooting

### Tests Pass Locally But Fail in CI

1. **Timing issues** - Add explicit waits in Playwright tests
2. **Environment differences** - Check baseURL, ports
3. **Parallel execution** - Tests might not be isolated

### Slow E2E Tests

1. Run E2E only on main branch, not every commit
2. Run E2E tests in parallel (update `workers` in playwright.config.ts)
3. Reduce timeout values

### Code Coverage Drops

1. Review what code was added without tests
2. Focus on critical paths first
3. Don't aim for 100% - 80% is good

## Next Steps

Review [Test Data & Fixtures](05-test-data-fixtures.md) for managing test data.
