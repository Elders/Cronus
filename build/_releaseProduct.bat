powershell -ExecutionPolicy Unrestricted -Command "& {Import-Module .\psake.psm1; Invoke-psake .\release.ps1 -properties @{ sln='NMSD.Cronus.sln' }%*} "
PAUSE