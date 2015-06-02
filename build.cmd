@echo off

SETLOCAL

SET TOOLS_PATH=.\bin\tools
SET NUGET=%TOOLS_PATH%\NuGet\NuGet.exe
SET FAKE=%TOOLS_PATH%\FAKE\tools\Fake.exe
SET NYX=%TOOLS_PATH%\Nyx\tools\build.fsx
SET BUILD_TOOLS_PATH="%ProgramFiles(x86)%\MSBuild\12.0\bin\MSBuild.exe"

IF NOT EXIST %BUILD_TOOLS_PATH% (
  echo In order to build or run this tool you need either Visual Studio 2015 or
  echo Microsoft Build Tools 2013 tools installed.
  echo.
  echo Visit this page to download either:
  echo.
  echo http://www.microsoft.com/en-us/download/details.aspx?id=40760
  echo.
  goto :eof
)

echo Downloading latest version of NuGet.exe...
IF NOT EXIST %TOOLS_PATH%\NuGet md %TOOLS_PATH%\NuGet
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://www.nuget.org/nuget.exe' -OutFile '%NUGET%'"

echo Downloading latest version of Fake.exe...
%NUGET% "install" "FAKE" "-OutputDirectory" "%TOOLS_PATH%" "-ExcludeVersion" "-Prerelease"

echo Downloading latest version of Nuget.Core...
%NUGET% "install" "Nuget.Core" "-OutputDirectory" "%TOOLS_PATH%" "-ExcludeVersion" "-Prerelease"

echo Downloading latest version of Nyx...
%NUGET% "install" "Nyx" "-OutputDirectory" "%TOOLS_PATH%" "-ExcludeVersion" "-Prerelease"

SET TARGET="Build"

IF NOT [%1]==[] (set TARGET="%1")

SET SUMMARY="Elders.Cronus"
SET DESCRIPTION="Elders.Cronus"

%FAKE% %NYX% "target=%TARGET%" appName=Elders.Cronus appSummary=%SUMMARY% appDescription=%DESCRIPTION% nugetPackageName=Cronus
