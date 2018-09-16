#addin "nuget:?package=CodeFileSanity&version=0.0.21"
#addin "nuget:?package=JetBrains.ReSharper.CommandLineTools&version=2018.2.2"
#tool "nuget:?package=NVika.MSBuild&version=1.0.1"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Build");
var framework = Argument("framework", "netcoreapp2.1");
var configuration = Argument("configuration", "Release");

var osuDesktop = new FilePath("./osu.Desktop/osu.Desktop.csproj");
var osuSolution = new FilePath("./osu.sln");

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Compile")
.Does(() => {
    DotNetCoreBuild(osuDesktop.FullPath, new DotNetCoreBuildSettings {
        Framework = framework,
        Configuration = configuration
    });
});

Task("Test")
.Does(() => {
    var testAssemblies = GetFiles("**/*.Tests/bin/**/*.Tests.dll");

    DotNetCoreVSTest(testAssemblies, new DotNetCoreVSTestSettings {
        Logger = AppVeyor.IsRunningOnAppVeyor ? "Appveyor" : $"trx",
        Parallel = true,
        ToolTimeout = TimeSpan.FromMinutes(10),
    });
});

// windows only because both inspectcore and nvika depend on net45
Task("InspectCode")
.WithCriteria(IsRunningOnWindows())
.IsDependentOn("Compile")
.Does(() => {
    var nVikaToolPath = GetFiles("./tools/NVika.MSBuild.*/tools/NVika.exe").First();
  
    DotNetCoreRestore(osuSolution.FullPath);

    InspectCode(osuSolution, new InspectCodeSettings {
        CachesHome = "inspectcode",
        OutputFile = "inspectcodereport.xml",
    });

    StartProcess(nVikaToolPath, @"parsereport ""inspectcodereport.xml"" --treatwarningsaserrors");
});

Task("CodeFileSanity")
.Does(() => {
    ValidateCodeSanity(new ValidateCodeSanitySettings {
        RootDirectory = ".",
        IsAppveyorBuild = AppVeyor.IsRunningOnAppVeyor
    });
});

Task("Build")
.IsDependentOn("CodeFileSanity")
.IsDependentOn("InspectCode")
.IsDependentOn("Test");

RunTarget(target);