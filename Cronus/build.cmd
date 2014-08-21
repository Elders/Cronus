@echo off

".\tools\NuGet\NuGet.exe" "install" "FAKE" "-OutputDirectory" ".\bin\tools" "-ExcludeVersion" "-Prerelease"
".\tools\Nuget\NuGet.exe" "install" "FSharp.Formatting.CommandTool" "-OutputDirectory" ".\bin\tools" "-ExcludeVersion" "-Prerelease"
".\tools\Nuget\NuGet.exe" "install" "SourceLink.Fake" "-OutputDirectory" ".\bin\tools" "-ExcludeVersion"

SET TARGET="Build"

IF NOT [%1]==[] (set TARGET="%1")

".\bin\tools\FAKE\tools\Fake.exe" "build.fsx" "target=%TARGET%"