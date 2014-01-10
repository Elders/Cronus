properties {
 $base_directory = Resolve-Path ..
 $src_directory = "$base_directory\src"
 $sln=''
 $sln_file = "$src_directory\"
 $config = 'debug'; #debug or release or stage
 
 $assemblyInformationalVersion = "dev";
 $assemblyFileVersion = "0.0.0.?";
 $assemblyVersion = "0.0.0.0";
 $assemblyRevision = "0";
}

task default -depends Build;

task -name ValidateConfig -action {
    assert ( 'debug', 'release', 'stage' -contains $config) ` "Invalid config: $config; valid values are debug or release or stage";
}

task -name Build -depends ValidateConfig -description "Builds outdated source files" -action {
	$sln_file = "$src_directory\$sln";
    write-host 'Current solution is '$sln_file'';
    write-host 'Current configuration is '$config'';
    Build $sln_file $config
}

function Build([string] $sln_file, [string] $config){
    write-host 'The build task is now running...';
    UpdateAssemblyInfoFile
    exec { msbuild /nologo /verbosity:minimal $sln_file /t:Clean /p:Configuration=$config /m }
    exec { msbuild /nologo /verbosity:minimal $sln_file /p:Configuration=$config /m }
}

function UpdateAssemblyInfoFile() {
    
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
            
        write-host Updating file AssemblyInfo and AssemblyFileInfo: $filename --> $versionAssemblyInformational / $versionAssemblyFile / $versionAssembly
    }
    Copy-Item AssemblyVersionInfo.cs ..\src\
    Copy-Item AssemblyCompanyProductInfo.cs ..\src\
}