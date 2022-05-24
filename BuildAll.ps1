#requires -runasadministrator
$baseDir = Resolve-Path "$PSScriptRoot/.."
$ErrorActionPreference = 'Stop'

Write-Warning "BE AWARE THAT THIS SCRIPT IS CURRENTLY VERY SPECIFIC TO THE VERSIONS IT'S TARGETTING."
Write-Warning "IF YOU ARE TARGETING NEWER CHOCOLATEY COMPONENTS IT LIKELY NEEDS TO BE MODIFIED"
Write-Warning "PLEASE CANCEL WITH CTRL+C IF YOU DO NOT KNOW WHAT THIS WARNING MEANS"
Pause

$reposMissing = @(
    'choco'
    'choco-licensed'
    'licensing-services'
    'chocolatey-pro-server-simple'
    'choco-licensed-management-ui'
    'choco-licensed-services'
    'ChocolateyGUI'
    'chocolateygui-licensed'
) | Where-Object { -not (Test-Path -Path "$baseDir/$_" -PathType Container) }

if ($reposMissing.Count -gt 0) {
    throw "Missing repositories: $reposMissing"
}

function check {
    if ($LASTEXITCODE -ne 0) {
        throw "Something didn't work building $args"
    }
}

function copyNupkg {
    param(
        $Package,
        [switch]
        $CakeBuild,
        $Base = $baseDir
    )
    $splat = @{
        Path = if ($CakeBuild) {
            "$Base/$Package/code_drop/Packages/Chocolatey/*.nupkg"
        } else {
            "$Base/$Package/code_drop/nuget/*.nupkg"
        }
        Destination = "C:\CodeDrops"
    }
    New-Item $splat.Destination -ErrorAction Ignore -ItemType Directory
    Copy-Item @splat
}

$RhinoLicensingSource = "$baseDir/rhino-licensing"
$RhinoLicensingFiles = "$RhinoLicensingSource/code_drop/temp/_PublishedLibs/Rhino.Licensing"
$RhinoLicensingPackage = "$RhinoLicensingSource/code_drop/Packages/NuGet"
$ChocolateyFiles = "$baseDir/choco/code_drop/chocolatey/lib"
$ChocolateyLicensedFiles = "$baseDir/choco-licensed/code_drop/chocolateypro"
$ChocolateyGUIFiles = "$baseDir/ChocolateyGUI/code_drop/temp/_PublishedLibs"



try {
    Push-Location $RhinoLicensingSource
    ./build.ps1
    check Rhino.Licensing

    Write-Host Chocolatey
    Set-Location $baseDir/choco
    Copy-Item "$RhinoLicensingFiles/net40/Rhino.Licensing.dll" "$baseDir/choco/lib/Rhino.Licensing.1.4.1/lib/net40/Rhino.Licensing.dll"
    ./build.bat
    check Chocolatey CLI
    copyNupkg -Package choco

    Write-Host Chocolatey Licensed Extension
    Set-Location $baseDir/choco-licensed
    nuget restore src/chocolateypro.sln
    Copy-Item "$RhinoLicensingFiles/net40/Rhino.Licensing.dll" "$baseDir/choco-licensed/lib/Rhino.Licensing.1.4.1/lib/net40/Rhino.Licensing.dll"
    Copy-Item $ChocolateyFiles/chocolatey.dll "$baseDir/choco-licensed/src/packages/chocolatey.lib.1.0.0/lib/chocolatey.dll"
    ./build.bat
    check Chocolatey Licensed Extension
    copyNupkg -Package choco-licensed

    Write-Host Licensing Services
    Set-Location $baseDir/licensing-services
    dotnet add source/LicensingServices.Core package Rhino.Licensing -v 1.6.0-cleanup -s $RhinoLicensingPackage
    ./build.ps1
    check Licensing Services
    copyNupkg -Package licensing-services -CakeBuild

    Write-Host Simple Server Pro
    Set-Location $baseDir/chocolatey-pro-server-simple
    nuget restore src
    Copy-Item $RhinoLicensingFiles "$baseDir/chocolatey-pro-server-simple/src/packages/Rhino.Licensing.1.5.0/lib" -Recurse -Force
    ./build.ps1
    check Simple Server Pro
    copyNupkg -Package chocolatey-pro-server-simple -CakeBuild

    Write-Host Chocolatey Central Management
    Set-Location $baseDir/choco-licensed-management-ui
    dotnet restore ChocolateySoftware.ChocolateyManagement.sln
    Copy-Item $RhinoLicensingFiles "$baseDir/choco-licensed-management-ui/src/packages/rhino.licensing/1.4.0/lib/" -Recurse -Force
    ./build.ps1
    check Chocolatey Central Management
    copyNupkg -Package .\choco-licensed-management-ui -CakeBuild

    Write-Host Chocolatey Agent
    Set-Location $baseDir/choco-licensed-services
    nuget restore src
    Copy-Item $ChocolateyFiles/chocolatey.dll "$baseDir/choco-licensed-services/src/packages/chocolatey.lib.1.0.0/lib/chocolatey.dll" -Recurse -Force
    Copy-Item $ChocolateyLicensedFiles/chocolatey.licensed.dll "$baseDir/choco-licensed-services/src/packages/chocolatey-licensed.lib.4.0.0/lib/chocolatey.licensed.dll" -Recurse -Force
    ./build.bat
    check Chocolatey Agent
    copyNupkg -Package choco-licensed-services

    Write-Host Chocolatey GUI
    Set-Location $baseDir/ChocolateyGUI
    nuget restore Source
    Copy-Item $ChocolateyFiles/chocolatey.dll "$baseDir/ChocolateyGUI/Source/packages/chocolatey.lib.1.0.0/lib/chocolatey.dll" -Recurse -Force
    ./build.ps1
    check Chocolatey GUI
    copyNupkg -Package ChocolateyGUI -CakeBuild

    Write-Host Chocolatey GUI - Licensed Extension
    Set-Location $baseDir/chocolateygui-licensed
    nuget restore Source
    Copy-Item $ChocolateyFiles/chocolatey.dll "$baseDir/chocolateygui-licensed/Source/packages/chocolatey.lib.1.0.0/lib/chocolatey.dll" -Recurse -Force
    Copy-Item $ChocolateyGUIFiles/ChocolateyGui.Common/ChocolateyGui.Common.dll "$baseDir/chocolateygui-licensed/Source/packages/ChocolateyGui.Common.1.0.0/lib/net48"
    Copy-Item $ChocolateyGUIFiles/ChocolateyGui.Common.Windows/ChocolateyGui.Common.Windows.dll "$baseDir/chocolateygui-licensed/Source/packages/ChocolateyGui.Common.Windows.1.0.0/lib/net48"
    ./build.ps1
    check Chocolatey GUI - Licensed Extension
    copyNupkg -Package chocolateygui-licensed -CakeBuild

}
finally {
    Pop-Location
}