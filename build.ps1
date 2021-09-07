[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [Parameter()]
    [string]$Path = "$PSScriptRoot/build",

    [Parameter()]
    [string]$Solution = $PSScriptRoot,

    [Parameter()]
    [switch]$Pack
)

$verbosityArgs = if ($VerbosePreference -ne 'SilentlyContinue' -or $DebugPreference -ne 'SilentlyContinue') {
    if ($DebugPreference -ne 'SilentlyContinue') {
        '--verbosity', 'diagnostic'
    }
    else {
        '--verbosity', 'detailed'
    }
}

# Build & test library
$buildSucceeded = $true

$arguments = @(
    '--configuration', $Configuration
    $verbosityArgs
)

dotnet build @arguments

if (-not $? -or $LASTEXITCODE -ne 0) {
    $buildSucceeded = $false
    Write-Error "An error occurred during the build for framework '$framework'."
}

$testFrameworks = @( 'net48', 'netcoreapp3.1' )
foreach ($framework in $testFrameworks) {

    $arguments = @(
        '--no-build'
        '--configuration', $Configuration
        '--framework', $framework
        $verbosityArgs
    )
    dotnet test @arguments

    if (-not $? -or $LASTEXITCODE -ne 0) {
        $buildSucceeded = $false
        Write-Error "Test run failed for framework '$framework'."
    }
}

if (-not $buildSucceeded) {
    throw "Build and/or tests did not complete successfully."
}

if ($Pack) {
    # Pack library
    $arguments = @(
        '--output', $Path
        '--configuration', $Configuration
        $verbosityArgs
    )

    dotnet pack @arguments
}
