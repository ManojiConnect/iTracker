#addin nuget:?package=Cake.Coverlet

var target = Argument("target", "Test");
var configuration = Argument("configuration", "Debug");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////


Task("Build")
    .Does(() =>
{
    DotNetBuild("WebApiBoilerPlate.sln", new DotNetBuildSettings
    {
        Configuration = configuration,
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
	var coverletSettings = new CoverletSettings {
        CollectCoverage = true,
        CoverletOutputFormat = CoverletOutputFormat.opencover | CoverletOutputFormat.json,
        MergeWithFile = MakeAbsolute(new DirectoryPath("./coverage.json")).FullPath,
        CoverletOutputDirectory = MakeAbsolute(new DirectoryPath(@"./coverage")).FullPath
    };
	
	Coverlet(
        "./tests/BoilerPlate.Api.IntegrationTests/bin/Debug/net7.0/BoilerPlate.Api.IntegrationTests.dll", 
        "./tests/BoilerPlate.Api.IntegrationTests/BoilerPlate.Api.IntegrationTests.csproj", 
        coverletSettings);
		
	Coverlet(
        "./tests/BoilerPlate.Api.UnitTests/bin/Debug/net7.0/BoilerPlate.Api.UnitTests.dll", 
        "./tests/BoilerPlate.Api.UnitTests/BoilerPlate.Api.UnitTests.csproj", 
        coverletSettings);
	
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);