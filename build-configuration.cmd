@echo off

set nugetserver=https://www.myget.org/F/eldersoss/api/v2/package

@powershell -File .nyx\build.ps1 '--appname=Elders.Cronus' '--nugetPackageName=Cronus'
