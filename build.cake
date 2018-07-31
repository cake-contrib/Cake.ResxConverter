// Arguments
var target = Argument("target", "Default");
var configuration = "Release";

// Enviroment
var isRunningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;

// Define directories
var solutionFile = File("Cake.ResxConverter.sln");
var artifactsDirectory = Directory("artifacts");

// Versioning
var version = "1.0.2";
var versionSuffix = "local" + DateTime.Now.Ticks;

// Misc
Func<DotNetCoreMSBuildSettings> msBuildHideLogoSettings = () => new DotNetCoreMSBuildSettings().HideLogo();

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
    MSBuildSettings = msBuildHideLogoSettings()
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
    AppVeyor.UpdateBuildVersion(string.Format("{0}-{1}-build{2}", version, AppVeyor.Environment.Repository.Branch, AppVeyor.Environment.Build.Number));

    versionSuffix = AppVeyor.Environment.Repository.Branch == "master"
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
      MSBuildSettings = msBuildHideLogoSettings().SetVersion(version)
    };

    DotNetCoreBuild(solutionFile, setings);
  });

Task ("NuGet")
	.IsDependentOn("Build")
	.Does (() =>
  {
    var settings = new DotNetCorePackSettings
    {
      VersionSuffix = versionSuffix,
      Configuration = configuration,
      OutputDirectory = artifactsDirectory,
      NoBuild = true,
      NoRestore = true,
      Verbosity = DotNetCoreVerbosity.Minimal,
      MSBuildSettings = msBuildHideLogoSettings().SetVersionPrefix(version) // Use this method to avoid overriding the suffix
    };

    DotNetCorePack("src/Cake.ResxConverter/Cake.ResxConverter.csproj", settings);
  });

Task("Default")
	.IsDependentOn("NuGet");

RunTarget(target);