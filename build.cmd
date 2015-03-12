@echo off

".\tools\NuGet\NuGet.exe" "install" "FAKE" "-OutputDirectory" ".\bin\tools" "-ExcludeVersion" "-Prerelease"

SET TARGET="Build"

IF NOT [%1]==[] (set TARGET="%1")

".\bin\tools\FAKE\tools\Fake.exe" "build.fsx" "target=%TARGET%"