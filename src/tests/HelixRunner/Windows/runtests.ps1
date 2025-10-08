param(
    [switch]$Verbose,
    [switch]$StopOnFirstFailure
)

Write-Host "Running Unit Tests..." -ForegroundColor Green

# Search recursively for *.UnitTests.dll files starting from 3 levels up
$searchPath = Join-Path (Get-Location) "../../.."
Write-Host "Search Directory: $(Resolve-Path $searchPath)" -ForegroundColor Yellow

$testDlls = Get-ChildItem -Path $searchPath -Filter "*.UnitTests.dll" -Recurse

if ($testDlls.Count -eq 0) {
    Write-Host "No *.UnitTests.dll files found in $searchPath" -ForegroundColor Red
    exit 1
}

Write-Host "Found $($testDlls.Count) test assemblies:" -ForegroundColor Cyan
$testDlls | ForEach-Object { Write-Host "  - $($_.FullName)" -ForegroundColor Gray }

$totalTests = 0
$passedTests = 0
$failedTests = 0
$failedAssemblies = @()

foreach ($testDll in $testDlls) {
    # Extract test name (remove .UnitTests.dll suffix)
    $testName = $testDll.BaseName -replace '\.UnitTests$', ''
    $testDir = $testDll.Directory.FullName
    
    # Construct paths
    $runtimeConfigPath = Join-Path $testDir "$($testName).UnitTests.runtimeconfig.json" 
    $xunitConsolePath = Join-Path $testDir "xunit.console.dll"
    
    Write-Host ""
    Write-Host ("=" * 80) -ForegroundColor Magenta
    Write-Host "Running: $($testDll.Name)" -ForegroundColor Yellow
    Write-Host "Test Name: $testName" -ForegroundColor Gray
    Write-Host "Directory: $testDir" -ForegroundColor Gray
    Write-Host "Runtime Config: $runtimeConfigPath" -ForegroundColor Gray
    Write-Host ("=" * 80) -ForegroundColor Magenta
    
    # Check if required files exist
    if (-not (Test-Path $runtimeConfigPath)) {
        Write-Host "ERROR: Runtime config not found: $runtimeConfigPath" -ForegroundColor Red
        $failedAssemblies += $testDll.Name
        $failedTests++
        $totalTests++
        continue
    }
    
    if (-not (Test-Path $xunitConsolePath)) {
        Write-Host "ERROR: xunit.console.dll not found: $xunitConsolePath" -ForegroundColor Red
        $failedAssemblies += $testDll.Name
        $failedTests++
        $totalTests++
        continue
    }
    
    # Build command exactly as specified
    $command = "dotnet exec --runtimeconfig `"$runtimeConfigPath`" `"$xunitConsolePath`" `"$($testDll.FullName)`""
    
    if ($Verbose) {
        Write-Host "Command: $command" -ForegroundColor Cyan
    }
    
    # Set working directory to test directory for execution
    Push-Location $testDir
    
    try {
        # Execute the test
        Write-Host "Executing test..." -ForegroundColor White
        $startTime = Get-Date
        
        # Invoke-Expression "$command"
        $exitCode = $LASTEXITCODE
        
        $endTime = Get-Date
        $duration = $endTime - $startTime
        $durationStr = $duration.ToString("mm\:ss")
        
        # Check results based on exit code
        if ($exitCode -eq 0) {
            Write-Host "O $($testDll.Name) - PASSED (Duration: $durationStr)" -ForegroundColor Green
            $passedTests++
        } else {
            Write-Host "X $($testDll.Name) - FAILED (Exit Code: $exitCode, Duration: $durationStr)" -ForegroundColor Red
            $failedTests++
            $failedAssemblies += $testDll.Name
            
            if ($StopOnFirstFailure) {
                Write-Host "Stopping execution due to test failure." -ForegroundColor Red
                Pop-Location
                break
            }
        }
    } catch {
        Write-Host "✗ $($testDll.Name) - ERROR: $($_.Exception.Message)" -ForegroundColor Red
        $failedTests++
        $failedAssemblies += $testDll.Name
        
        if ($StopOnFirstFailure) {
            Write-Host "Stopping execution due to error." -ForegroundColor Red
            Pop-Location
            break
        }
    }
    finally {
        Pop-Location
    }
    
    $totalTests++
}

# Final Summary
Write-Host ""
Write-Host ("=" * 80) -ForegroundColor Magenta
Write-Host "TEST EXECUTION SUMMARY" -ForegroundColor White
Write-Host ("=" * 80) -ForegroundColor Magenta
Write-Host "Total Assemblies: $totalTests" -ForegroundColor White
Write-Host "Passed: $passedTests" -ForegroundColor Green
Write-Host "Failed: $failedTests" -ForegroundColor Red

if ($failedAssemblies.Count -gt 0) {
    Write-Host ""
    Write-Host 'Failed Assemblies:' -ForegroundColor Red
    foreach ($assembly in $failedAssemblies) {
        Write-Host "  - $assembly" -ForegroundColor Red
    }
    exit 1
} else {
    Write-Host ""
    $successMsg = 'All tests completed successfully!'
    Write-Host $successMsg -ForegroundColor Green
    exit 0
}