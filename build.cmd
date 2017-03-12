@echo off

SETLOCAL

SET NUGET=%LocalAppData%\NuGet\NuGet.exe
SET FAKE=%LocalAppData%\FAKE\tools\Fake.exe
SET NYX=%LocalAppData%\Nyx\tools\build_next.fsx
SET GITVERSION=%LocalAppData%\GitVersion.CommandLine\tools\GitVersion.exe
SET MSPEC=%LocalAppData%\Machine.Specifications.Runner.Console\tools\mspec-clr4.exe

echo Downloading NuGet.exe...
IF NOT EXIST %LocalAppData%\NuGet md %LocalAppData%\NuGet
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe' -OutFile '%NUGET%'"

echo Downloading FAKE...
IF NOT EXIST %LocalAppData%\FAKE %NUGET% "install" "FAKE" "-OutputDirectory" "%LocalAppData%" "-ExcludeVersion" "-Version" "4.50.0"

echo Downloading GitVersion.CommandLine...
IF NOT EXIST %LocalAppData%\GitVersion.CommandLine %NUGET% "install" "GitVersion.CommandLine" "-OutputDirectory" "%LocalAppData%" "-ExcludeVersion" "-Version" "3.6.1"

echo Downloading Machine.Specifications.Runner.Console...
IF NOT EXIST %LocalAppData%\Machine.Specifications.Runner.Console %NUGET% "install" "Machine.Specifications.Runner.Console" "-OutputDirectory" "%LocalAppData%" "-ExcludeVersion"

echo Downloading Nyx...
%NUGET% "install" "Nyx" "-OutputDirectory" "%LocalAppData%" "-ExcludeVersion" "-PreRelease"

%FAKE% %NYX% "target=clean" -st
%FAKE% %NYX% "target=RestoreNugetPackages" -st

IF NOT [%1]==[] (set RELEASE_NUGETKEY="%1")

SET RELEASE_NOTES=src/Elders.Cronus/RELEASE_NOTES.md
SET SUMMARY="Cronus is a lightweight framework for dispatching and receiving messages between microservices with DDD/CQRS in mind"
SET DESCRIPTION="Cronus is a lightweight framework for dispatching and receiving messages between microservices with DDD/CQRS in mind"

%FAKE% %NYX% "target=RunTests" appName=Elders.Cronus.Tests appSummary=%SUMMARY% appDescription=%DESCRIPTION% appReleaseNotes=%RELEASE_NOTES% nugetPackageName=Cronus.Tests appType=tests
if errorlevel 1 (
   echo Tests faild with exit code %errorlevel%
   exit /b %errorlevel%
)

%FAKE% %NYX% appName=Elders.Cronus appSummary=%SUMMARY% appDescription=%DESCRIPTION% nugetkey=%RELEASE_NUGETKEY% nugetPackageName=Cronus