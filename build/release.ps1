
task default -depends Release;

task -name Pack -action {
	Invoke-psake .\build.ps1 Build -properties @{ config='release'; }
	exec { msbuild /nologo /verbosity:minimal $msiProject /t:Clean /p:Configuration=$config /m }
    exec { msbuild /nologo /verbosity:minimal $msiProject /p:Configuration=$config /m }
    Create-Directory $msiDeployDir
    Copy-Item $msiSource $msiDeploy
	write-host $msiDeploy
}

function Create-Directory($dir){
	if (!(Test-Path -path $dir)) { new-item $dir -force -type directory}
}