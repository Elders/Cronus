properties {
  $sln=''
  $base_directory = Resolve-Path ..
  $nugetDeployDir = "$base_directory\.nuget\lib"
  $nugetSource = "$base_directory\bin\release\NMSD.Cronus"
}

task default -depends Release;

task -name Release -action {
	Invoke-psake .\build.ps1 Build -properties @{ config='release'; sln=$sln }
	#exec { msbuild /nologo /verbosity:minimal $msiProject /t:Clean /p:Configuration=$config /m }
    #exec { msbuild /nologo /verbosity:minimal $msiProject /p:Configuration=$config /m }
    Create-Directory $nugetDeployDir
    Copy-Item $nugetSource $nugetDeployDir
	write-host $nugetDeployDir
}

function Create-Directory($dir){
	if (!(Test-Path -path $dir)) { new-item $dir -force -type directory}
}