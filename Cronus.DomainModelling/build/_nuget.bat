powershell -ExecutionPolicy Unrestricted -Command "& {Import-Module .\psake.psm1; Invoke-psake .\build.ps1 nuget -properties @{ config='Release' }; exit !($psake.build_success)} "
PAUSE