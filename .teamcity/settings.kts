import jetbrains.buildServer.configs.kotlin.v2019_2.*
import jetbrains.buildServer.configs.kotlin.v2019_2.buildSteps.powerShell
import jetbrains.buildServer.configs.kotlin.v2019_2.triggers.vcs
import jetbrains.buildServer.configs.kotlin.v2019_2.vcs.GitVcsRoot

version = "2021.2"

project {
    buildType(RhinoLicensingBuild)
}

object RhinoLicensingBuild : BuildType({
    name = "Rhino.Licensing Build"

    artifactRules = "code_drop/TestResults/issues-report.html"

    params {
        param("teamcity.git.fetchAllHeads", "true")
        param("env.Git_Branch", "%teamcity.build.vcs.branch.RhinoLicensingAnsible_RhinoAnsibleVcsRoot%")
        param("env.vcsroot.branch", "%vcsroot.branch%")
    }

    vcs {
        root(DslContext.settingsRoot)
    }

    steps {
        powerShell {
            name = "Install dependencies"
            formatStderrAsError = true
            scriptMode = script {
                content = """
                    choco install netfx-4.8-devpack dotnetcore-sdk --confirm --no-progress

                    ${'$'}result = ${'$'}LASTEXITCODE
                    if (${'$'}result -notin 0, 1641, 3010) {
                    	throw "One or more dependencies failed to install. Last exit code: ${'$'}result"
                    }
                """.trimIndent()
            }
        }
        powerShell {
            name = "Build & Test"
            scriptMode = script {
                content = """
                    ${'$'}env:PATH = @(
                    	[System.Environment]::GetEnvironmentVariable('PATH', 'User')
                        [System.Environment]::GetEnvironmentVariable('PATH', 'Machine')
                    ) -join ';'

                    & ./build.ps1 -Verbosity Diagnostic
                """.trimIndent()
            }
            noProfile = false
            param("jetbrains_powershell_script_file", "build.ps1")
        }
    }

    triggers {
        vcs {
        }
    }
})
