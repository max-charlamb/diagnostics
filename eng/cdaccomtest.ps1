$env:SOS_TEST_CDAC="true"

$env:DOTNET_ROOT="C:\Users\maxcharlamb\source\reposA\diagnostics\.dotnet-test"
$env:DOTNET_MULTILEVEL_LOOKUP=0
$env:DOTNET_ENABLE_CDAC=1
$env:DOTNET_EnableWriteXorExecute=0
$env:COMPlus_EnableWriteXorExecute=0

$LogFile = "$PSScriptRoot\cdaccomtest.log"
$env:DOTNET_ENABLED_SOS_LOGGING="$LogFile"

# $Command = $PSScriptRoot + "\..\.dotnet\dotnet.exe"
# $Params = @("test", "--no-build", "--blame-crash", "--logger", "console;verbosity=detailed", "$PSScriptRoot\..\src\SOS\SOS.UnitTests\SOS.UnitTests.csproj", "--filter", "FullyQualifiedName~StackAndOtherTests")

$Command = "C:\Users\maxcharlamb\.nuget\packages\cdb-sos\10.0.26100.1\runtimes\win-x64\native\cdb.exe"
$Params = @("-cf", "$PSScriptRoot\cdbscript.ini",
            "-y", "C:\Users\maxcharlamb\source\reposA\diagnostics\artifacts\Debuggees\portable\SymbolTestApp\SymbolTestApp\bin\Debug\net10.0\publish",
            "-c", ".load C:\Users\maxcharlamb\source\reposA\diagnostics\artifacts\bin\Windows_NT.x64.Debug\runcommand.dll",
            "-Gsins", "C:\Users\maxcharlamb\source\reposA\diagnostics\.dotnet-test\dotnet.exe",
            "--fx-version", "10.0.0-preview.7.25325.106",
            "C:\Users\maxcharlamb\source\reposA\diagnostics\artifacts\Debuggees\portable\SymbolTestApp\SymbolTestApp\bin\Debug\net10.0\publish\SymbolTestApp.dll",
            "C:\Users\maxcharlamb\source\reposA\diagnostics\artifacts\Debuggees\portable\SymbolTestApp\SymbolTestApp\bin\Debug\net10.0\publish")

# Set maximum number of test runs
$MaxRuns = 1000
$RunCount = 0

Write-Host "Starting test loop (max runs: $MaxRuns)"

do {
    $RunCount++
    Write-Host "Test run #$RunCount"

    # Delete the log file if it exists before starting
    if (Test-Path $LogFile) {
        Remove-Item $LogFile -Force
        Write-Host "Deleted existing log file: $LogFile"
    }


    $result = & $Command $Params *>&1

    if ($LASTEXITCODE -eq "0")
    {
        Write-Host "Test run #$RunCount completed successfully."
        $StatusLine = $result | Select-Object -Last 2
        Write-Host $StatusLine
    }
    else
    {
        $ErrorLines = $result | Select-Object -Last 100
        Write-Error "Test run #$RunCount failed with exit code 0x$([Convert]::ToString($LASTEXITCODE, 16).ToUpper()). Last lines of output:"
        Write-Error ($ErrorLines -join [System.Environment]::NewLine)
        break
    }

} while ($RunCount -lt $MaxRuns)

if ($RunCount -eq $MaxRuns -and $LASTEXITCODE -eq "0")
{
    Write-Host "All $MaxRuns test runs completed successfully without failure."
}
