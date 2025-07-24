import jetbrains.buildServer.configs.kotlin.v2019_2.*
import jetbrains.buildServer.configs.kotlin.v2019_2.buildSteps.script
import jetbrains.buildServer.configs.kotlin.v2019_2.buildSteps.powerShell
import jetbrains.buildServer.configs.kotlin.v2019_2.buildFeatures.pullRequests
import jetbrains.buildServer.configs.kotlin.v2019_2.buildSteps.nuGetPublish
import jetbrains.buildServer.configs.kotlin.v2019_2.triggers.vcs
import jetbrains.buildServer.configs.kotlin.v2019_2.triggers.ScheduleTrigger
import jetbrains.buildServer.configs.kotlin.v2019_2.triggers.schedule
import jetbrains.buildServer.configs.kotlin.v2019_2.vcs.GitVcsRoot

project {
    buildType(RhinoLicensing)
    buildType(RhinoLicensingSchd)
    buildType(RhinoLicensingQA)
}

object RhinoLicensing : BuildType({
    id("RhinoLicensing")
    name = "Build (Unit Tests)"

    templates(AbsoluteId("SlackNotificationTemplate"))

    artifactRules = """
    """.trimIndent()

    params {
        param("env.vcsroot.branch", "%vcsroot.branch%")
        param("env.Git_Branch", "%teamcity.build.vcs.branch.RhinoLicensing_RhinoLicensingVcsRoot%")
        param("teamcity.git.fetchAllHeads", "true")
        password("env.GITHUB_PAT", "%system.GitHubPAT%", display = ParameterDisplay.HIDDEN, readOnly = true)
    }

    vcs {
        root(DslContext.settingsRoot)

        branchFilter = """
            +:*
        """.trimIndent()
    }

    steps {
        powerShell {
            name = "Install dependencies"
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

        script {
            name = "Call Cake"
            scriptContent = """
                build.bat --verbosity=diagnostic --target=CI --testExecutionType=unit --shouldRunOpenCover=false
            """.trimIndent()
        }
    }

    triggers {
        vcs {
            branchFilter = """
                +:*
                -:master
                -:support/*
            """.trimIndent()
        }
    }

    features {
        pullRequests {
            provider = github {
                authType = token {
                    token = "%system.GitHubPAT%"
                }
            }
        }
    }
})

object RhinoLicensingSchd : BuildType({
    id = AbsoluteId("RhinoLicensingSchd")
    name = "Build (Scheduled Integration Testing)"

    templates(AbsoluteId("SlackNotificationTemplate"))

    artifactRules = """
    """.trimIndent()

    params {
        param("env.vcsroot.branch", "%vcsroot.branch%")
        param("env.Git_Branch", "%teamcity.build.vcs.branch.RhinoLicensing_RhinoLicensingVcsRoot%")
        param("teamcity.git.fetchAllHeads", "true")
        password("env.GITHUB_PAT", "%system.GitHubPAT%", display = ParameterDisplay.HIDDEN, readOnly = true)
    }

    vcs {
        root(DslContext.settingsRoot)

        branchFilter = """
            +:*
        """.trimIndent()
    }

    steps {
        powerShell {
            name = "Install dependencies"
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

        script {
            name = "Call Cake"
            scriptContent = """
                build.bat --verbosity=diagnostic --target=CI --testExecutionType=all --shouldRunOpenCover=false --shouldRunAnalyze=false --shouldRunIlMerge=false --shouldObfuscateOutputAssemblies=false --shouldRunChocolatey=false --shouldRunNuGet=false --shouldAuthenticodeSignOutputAssemblies=false --shouldAuthenticodeSignPowerShellScripts=false
            """.trimIndent()
        }
    }

    triggers {
        schedule {
            schedulingPolicy = daily {
                hour = 2
                minute = 0
            }
            branchFilter = """
                +:<default>
            """.trimIndent()
            triggerBuild = always()
            withPendingChangesOnly = false
        }
    }
})

object RhinoLicensingQA : BuildType({
    id = AbsoluteId("RhinoLicensingQA")
    name = "Build (SonarQube)"

    templates(AbsoluteId("SlackNotificationTemplate"))

    artifactRules = """
    """.trimIndent()

    params {
        param("env.vcsroot.branch", "%vcsroot.branch%")
        param("env.Git_Branch", "%teamcity.build.vcs.branch.RhinoLicensing_RhinoLicensingVcsRoot%")
        param("env.SONARQUBE_ID", "rhino-licensing")
        param("teamcity.git.fetchAllHeads", "true")
        password("env.GITHUB_PAT", "%system.GitHubPAT%", display = ParameterDisplay.HIDDEN, readOnly = true)
    }

    vcs {
        root(DslContext.settingsRoot)

        branchFilter = """
            +:*
        """.trimIndent()
    }

    steps {
        powerShell {
            name = "Install dependencies"
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

        script {
            name = "Call Cake"
            scriptContent = """
                build.bat --verbosity=diagnostic --target=CI --testExecutionType=none --shouldRunSonarQube=true --shouldRunDependencyCheck=true --shouldRunOpenCover=false --shouldRunAnalyze=false --shouldRunIlMerge=false --shouldObfuscateOutputAssemblies=false --shouldRunChocolatey=false --shouldRunNuGet=false --shouldAuthenticodeSignOutputAssemblies=false --shouldAuthenticodeSignPowerShellScripts=false
            """.trimIndent()
        }
    }

    triggers {
        schedule {
            schedulingPolicy = weekly {
                dayOfWeek = ScheduleTrigger.DAY.Saturday
                hour = 2
                minute = 45
            }
            branchFilter = """
                +:<default>
            """.trimIndent()
            triggerBuild = always()
            withPendingChangesOnly = false
        }
    }
})
