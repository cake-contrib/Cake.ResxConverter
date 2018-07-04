// Arguments
var target = Argument("target", "Default");
var configuration = "Release";

// Enviroment
var isRunningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;

// Define directories
var solutionFile = File("Cake.ResxConverter.sln");
var artifactsDirectory = Directory("artifacts");

// Versioning
var nugetVersionSuffix = "local" + DateTime.Now.Ticks;

// Misc
var msBuildHideLogoSettings = new DotNetCoreMSBuildSettings().HideLogo();

Setup(context =>
{
	Information("AppVeyor: {0}", isRunningOnAppVeyor);
	Information("Configuration: {0}", configuration);
});

Task("Clean")
	.Does(() =>
{	
  CleanDirectory(artifactsDirectory);

  DotNetCoreClean(solutionFile, new DotNetCoreCleanSettings {
    Configuration = configuration,
    Verbosity = DotNetCoreVerbosity.Minimal,
    MSBuildSettings = msBuildHideLogoSettings
  });
});

Task("Restore")
	.Does(() => 
{
  DotNetCoreRestore(solutionFile);
});

Task("Update-Version")
  .WithCriteria(isRunningOnAppVeyor)
  .Does(() => 
  {
    // TODO decide if we really want/need to have this. If so, it means that we'd need to either:
    //      1) Read the version from the .csproj
    //      2) Manage assembly version / file version from this script (as MSBuild props)
    //
    // AppVeyor.UpdateBuildVersion(string.Format("{0}-{1}-build{2}", version.ToString(), AppVeyor.Environment.Repository.Branch, AppVeyor.Environment.Build.Number));

    nugetVersionSuffix = AppVeyor.Environment.Repository.Branch == "master"
      ? string.Empty
      : "pre" + AppVeyor.Environment.Build.Number;
  });

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("Restore")
  .IsDependentOn("Update-Version")
	.Does(() =>  
  {
    var setings = new DotNetCoreBuildSettings
    {
      Configuration = configuration,
      NoRestore = true,
      Verbosity = DotNetCoreVerbosity.Minimal,
      MSBuildSettings = msBuildHideLogoSettings
    };

    DotNetCoreBuild(solutionFile, setings);
  });

Task ("NuGet")
	.IsDependentOn("Build")
	.Does (() =>
  {
    var settings = new DotNetCorePackSettings
    {
        VersionSuffix = nugetVersionSuffix, // Package and assembly versions are set on .csproj
        Configuration = configuration,
        OutputDirectory = artifactsDirectory,
        NoBuild = true,
        NoRestore = true,
        Verbosity = DotNetCoreVerbosity.Minimal,
        MSBuildSettings = msBuildHideLogoSettings
    };

    DotNetCorePack("src/Cake.ResxConverter/Cake.ResxConverter.csproj", settings);
  });

Task("Default")
	.IsDependentOn("NuGet");

RunTarget(target);