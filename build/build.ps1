properties {
	$base_directory = Resolve-Path ..
	$src_directory = "$base_directory\src"
 
	$sln=''
	$sln_file = "$src_directory\"
	$config = "debug"; #debug or release or stage
 
	$assemblyInformationalVersion = "dev";
	$assemblyFileVersion = "0.0.0.?";
	$assemblyVersion = "0.0.0.0";
	$assemblyRevision = "0";

	$nugetPackageName=''
	$nugetDeployDir = "$base_directory\.nuget\lib\"
	$nugetSource=""
}

task default -depends PublishNugetPackage;

task -name ValidateConfig -action {
    assert ( "debug", "release", "stage" -contains $config) ` "Invalid config: $config; valid values are debug or release or stage";
}

task -name Build -depends ValidateConfig -description "Builds outdated source files" -action {
	$sln_full_path = "$src_directory\$sln";
	write-host
    write-host Current solution is: `t --> `t $sln_full_path;
    write-host Current config is: `t --> `t $config;
	write-host
    Build $sln_full_path $config
}

task -name PublishNugetPackage -action {
	$sln_full_path = "$src_directory\$sln";
	write-host
    write-host Current solution is: `t --> `t $sln_full_path;
    write-host Current config is: `t --> `t $config;
	write-host
	BuildNugetPackage $sln_full_path
	PushNugetPackage
}

function BuildNugetPackage([string] $sln_full_path)
{
	Build $sln_full_path $config
	Delete-Directory $nugetDeployDir
	Create-Directory $nugetDeployDir
	$binariesToNuget = "$base_directory\bin\$config\$nugetSource\*"
	Copy-Item $binariesToNuget $nugetDeployDir
	
	cd $base_directory\.nuget\
	
	$assemblyFileVersion = $assemblyFileVersion.Replace("?",$assemblyRevision);
	$versionFilePattern = "<version>.*</version>";
	$versionAssemblyFile = "<version>" + $assemblyFileVersion + "</version>";

	Get-ChildItem -r -filter Product.nuspec | % {
		$filename = $_.fullname
        (get-content $filename) | % {$_ -replace $versionFilePattern, $versionAssemblyFile } > $filename
        write-host
        write-host Updating file Product.nuspec
    }

    & ".\NuGet.exe" pack Product.nuspec -IncludeReferencedProjects
	Delete-Directory $nugetDeployDir
}

function PushNugetPackage
{
	cd $base_directory\.nuget\
	$assemblyFileVersion = $assemblyFileVersion.Replace("?",$assemblyRevision);
	$package = "$nugetPackageName.$assemblyFileVersion.nupkg"
    & ".\NuGet.exe" push $package
}

function Create-Directory($dir){
	if (!(Test-Path -path $dir)) { new-item $dir -force -type directory}
	write-host Directory $dir was created.
}

function Delete-Directory($dir){
	if ((Test-Path -path $dir)) { Remove-Item $dir -Force -Recurse }
	write-host Directory $dir was deleted.
}

function Build([string] $sln_file, [string] $config){
    write-host The build is now running...
    UpdateAssemblyInfoFile $assemblyInformationalVersion $assemblyVersion $assemblyFileVersion $assemblyRevision
    exec { msbuild /nologo /verbosity:minimal $sln_file /t:Clean /p:Configuration=$config /m }
    exec { msbuild /nologo /verbosity:minimal $sln_file /p:Configuration=$config /m }
	write-host The build finished.
}

function UpdateAssemblyInfoFile([string]$assemblyInformationalVersion, [string] $assemblyVersion, [string]$assemblyFileVersion, [string]$assemblyRevision) {
    
	$versionInformationalPattern = 'AssemblyInformationalVersion\(".*"\)';
	$versionAssemblyInformational = 'AssemblyInformationalVersion("' + $assemblyInformationalVersion + '")';
    
	$assemblyFileVersion = $assemblyFileVersion.Replace("?",$assemblyRevision);
	$versionFilePattern = 'AssemblyFileVersion\(".*"\)';
	$versionAssemblyFile = 'AssemblyFileVersion("' + $assemblyFileVersion + '")';
    
    $versionPattern = 'AssemblyVersion\(".*"\)';
	$versionAssembly = 'AssemblyVersion("' + $assemblyVersion + '")';

	Get-ChildItem -r -filter AssemblyVersionInfo.cs | % {
		$filename = $_.fullname
        (get-content $filename) | % {$_ -replace $versionInformationalPattern, $versionAssemblyInformational } | % {$_ -replace $versionFilePattern, $versionAssemblyFile } | % {$_ -replace $versionPattern, $versionAssembly } > $filename
        write-host
        write-host Updating file AssemblyInfo and AssemblyFileInfo: 
		write-host `t --> $filename
		write-host `t --> $versionAssemblyInformational
		write-host `t --> $versionAssemblyFile 
		write-host `t --> $versionAssembly
		write-host
    }
    Copy-Item AssemblyVersionInfo.cs ..\src\
    Copy-Item AssemblyCompanyProductInfo.cs ..\src\
	write-host Assembly info file was updated.
}