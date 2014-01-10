powershell -ExecutionPolicy Unrestricted -Command "& {Import-Module .\psake.psm1; Invoke-psake .\build.ps1 -properties @{ sln='NMSD.Cronus.sln' }%*} "
PAUSE