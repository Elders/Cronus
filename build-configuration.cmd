@echo off

%FAKE% %NYX% "target=clean" -st
%FAKE% %NYX% "target=RestoreNugetPackages" -st


IF NOT [%1]==[] (set RELEASE_NUGETKEY="%1")

SET SUMMARY="Cronus is a lightweight framework for dispatching and receiving messages between microservices with DDD/CQRS in mind"
SET DESCRIPTION="Cronus is a lightweight framework for dispatching and receiving messages between microservices with DDD/CQRS in mind"

REM FIX ME PLEASE
REM %FAKE% %NYX% "target=RunTests" appName=Elders.Cronus.Tests appSummary=%SUMMARY% appDescription=%DESCRIPTION% appType=tests
REM if errorlevel 1 (
REM   echo Tests faild with exit code %errorlevel%
REM   exit /b %errorlevel%
REM )

%FAKE% %NYX% appName=Elders.Cronus appSummary=%SUMMARY% appDescription=%DESCRIPTION% nugetkey=%RELEASE_NUGETKEY% nugetPackageName=Cronus