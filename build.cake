//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var packagesDir = Directory("packages");
var buildDir = Directory("./Dcc/bin") + Directory(configuration);
var solution = "./Dcc.sln";

var libuvPackageDir = "Libuv.1.9.1";
var libuvFilePathInsideNugetPackage = "runtimes/win7-x86/native/libuv.dll";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(solution);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      MSBuild(solution, settings =>
        settings.SetConfiguration(configuration));
    }
    else
    {
      XBuild(solution, settings =>
        settings.SetConfiguration(configuration));
    }
});

Task("Copy-Libuv")
    .IsDependentOn("Build")
    .Does(() =>
{
    var libuvSourceFile = Directory("packages") + Directory(libuvPackageDir) + File(libuvFilePathInsideNugetPackage);
    CopyFileToDirectory(libuvSourceFile, buildDir);
});

Task("Run")
    .IsDependentOn("Copy-Libuv")
    .Does(() =>
{
    StartProcess(buildDir + File("Dcc.exe"));
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
