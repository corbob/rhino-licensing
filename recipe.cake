#load nuget:https://hermes.chocolatey.org:8443/repository/choco-internal-testing/?package=cake-chocolatey-recipe&prerelease&version=0.1.0-unstable0082


///////////////////////////////////////////////////////////////////////////////
// ADDINS
///////////////////////////////////////////////////////////////////////////////

// (None)

///////////////////////////////////////////////////////////////////////////////
// RECIPE SETUP
///////////////////////////////////////////////////////////////////////////////


Environment.SetVariableNames();
var packageSources = new List<PackageSourceData>();
var nugetDevUrl = EnvironmentVariable("NUGET_DEV_SOURCE");
var nugetProdUrl = EnvironmentVariable("NUGET_PROD_SOURCE");

packageSources.Add(new PackageSourceData(Context, "NugetDev", nugetDevUrl, FeedType.NuGet, isRelease: false));
packageSources.Add(new PackageSourceData(Context, "NugetProd", nugetProdUrl, FeedType.NuGet, isRelease: true));

Func<FilePathCollection> getProjectsToPack = ()=> GetFiles(BuildParameters.SourceDirectoryPath + "/**/*.csproj")
        - GetFiles(BuildParameters.RootDirectoryPath + "/tools/**/*.csproj")
        - GetFiles(BuildParameters.SourceDirectoryPath + "/**/*.Tests.csproj")
        - GetFiles(BuildParameters.SourceDirectoryPath + "/packages/**/*.csproj")
        - GetFiles(BuildParameters.SourceDirectoryPath + "/**/*.AdminTool.csproj");

BuildParameters.SetParameters(
    context: Context,
    buildSystem: BuildSystem,
    sourceDirectoryPath: "./src",
    solutionFilePath: "./Rhino.Licensing.sln",
    title: "Rhino.Licensing",
    repositoryOwner: "chocolatey",
    repositoryName: "rhino-licensing",
    productName: "Rhino Licensing",
    productDescription: "Licensing Framework for .NET.",
    productCopyright: "Copyright (c) 2005 - 2009 Ayende Rahien (ayende@ayende.com).",
    shouldStrongNameOutputAssemblies: false,
    shouldObfuscateOutputAssemblies: false,
    shouldAuthenticodeSignOutputAssemblies: false,
    shouldStrongNameSignDependentAssemblies: false,
    shouldRunInspectCode: false,
    treatWarningsAsErrors: false,
    testDirectoryPath: "./test",
    packageSourceDatas: packageSources,
    shouldRunDotNetCorePack: true,
    shouldRunDupFinder: false,
    getProjectsToPack: getProjectsToPack);

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(context: Context);

///////////////////////////////////////////////////////////////////////////////
// PROJECT SPECIFIC TASKS
///////////////////////////////////////////////////////////////////////////////

// (None)

///////////////////////////////////////////////////////////////////////////////
// RUN IT!
///////////////////////////////////////////////////////////////////////////////

Build.RunDotNetCore();
