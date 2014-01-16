properties {
	$base_directory = Resolve-Path ..
	$src_directory = "$base_directory\src"
 	
	$sln="NMSD.Cronus"
	$sln_file = "$src_directory\"
	
	$config = "debug"; #debug or release or stage
	
	$company="NMSD"
	$product="Cronus"
 
	$assemblyInformationalVersion = "dev";
	$assemblyFileVersion = "0.0.0.?";
	$assemblyVersion = "0.0.0.0";
	$assemblyRevision = "0";

	$nugetDeployDir = "$base_directory\.nuget\lib\"
	$nugetSourceDir="NMSD.Cronus"
}

task default -depends AssemblyInfo, ValidateConfig, Build
task nuget -depends AssemblyInfo, ValidateConfig, Build, PublishNugetPackage

task ValidateConfig {
    assert ( "debug", "release", "stage" -contains $config) ` "Invalid config: $config; valid values are debug or release or stage";
}

task -name AssemblyInfo -action {
	Create-AssemblyVersionInfo $assemblyInformationalVersion $assemblyVersion $assemblyFileVersion.Replace("?",$assemblyRevision) $src_directory\AssemblyVersionInfo.cs
	Create-AssemblyCompanyProductInfo $company $product $src_directory\AssemblyCompanyProductInfo.cs
}

task Build -depends ValidateConfig, AssemblyInfo -description "Builds outdated source files" {
    Build "$src_directory\$sln" $config
}

task PublishNugetPackage {
	$sln_full_path = "$src_directory\$sln";
	BuildNugetPackage $sln_full_path
	PushNugetPackage
}

######################################################################################################

function BuildNugetPackage([string] $sln_full_path)
{
	Delete-Directory $nugetDeployDir
	Create-Directory $nugetDeployDir
	$binariesToNuget = "$base_directory\bin\$config\$nugetSourceDir\*"
	Copy-Item $binariesToNuget $nugetDeployDir
	
	cd $base_directory\.nuget\
	
	Create-ProductNuspec $product $assemblyFileVersion.Replace("?",$assemblyRevision) "$base_directory\.nuget\Product.nuspec"

    & ".\NuGet.exe" pack Product.nuspec -IncludeReferencedProjects
	Delete-Directory $nugetDeployDir
	Remove-Item "$base_directory\.nuget\*" -include *.nuspec
}

function PushNugetPackage
{
	cd $base_directory\.nuget\
	$assemblyFileVersion = $assemblyFileVersion.Replace("?",$assemblyRevision);
	$package = $product.$assemblyFileVersion.nupkg
    & ".\NuGet.exe" push $package
	Remove-Item "$base_directory\.nuget\*" -include *.nupkg
}

function Build([string] $sln_file, [string] $config){
    write-host The build is now running...
	write-host
    write-host Current solution: `t --> $sln_file
    write-host Current config: `t --> $config
	write-host
	
    exec { msbuild /nologo /verbosity:minimal $sln_file /t:Clean /p:Configuration=$config /m }
    exec { msbuild /nologo /verbosity:minimal $sln_file /p:Configuration=$config /m }
	
	write-host The build finished successfully!
}

function global:Create-ProductNuspec($product, $assemblyFileVersion, $filename)
{
"<?xml version=""1.0""?>
<package >
  <metadata>
    <id>$product</id>
    <version>$assemblyFileVersion</version>
    <authors>Nikolay Mynkow, Simeon Dimov</authors>
    <owners>Nikolay Mynkow, Simeon Dimov</owners>
    <licenseUrl>https://github.com/NMSD/Cronus/blob/master/LICENSE</licenseUrl>
    <projectUrl>http://nmsd.github.io/Cronus/</projectUrl>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <description>Simple CQRS + EventStore framework</description>
    <releaseNotes>https://github.com/NMSD/Cronus</releaseNotes>
    <copyright>Copyright NMSD 2014</copyright>
    <tags>CQRS, EventSource</tags>
  </metadata>
</package>" | out-file $filename -encoding "ASCII" 
}

function global:Create-AssemblyVersionInfo($assemblyInformationalVersion, $assemblyVersion, $assemblyFileVersion, $filename)
{
write-host Updating file $filename : 
write-host `t --> $assemblyInformationalVersion
write-host `t --> $assemblyFileVersion 
write-host `t --> $assemblyVersion
write-host

"using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Stage Number
//      Revision

//	Stage Numbers
//		Alpha	- 7100
//		Beta	- 7200
//		RC		- 7300
//		GA	- 7400
//		HF		- 7500
//		SP		- 7600

//	Revision
//		This is the first charachters of GIT revision.

//	The version of the product which is released
[assembly: AssemblyInformationalVersion(""$assemblyInformationalVersion"")]

//	Describes file version of the assembly. If you increment Major Version 
//	you probably want to change the AssemblyVersion as well
[assembly: AssemblyFileVersion(""$assemblyFileVersion"")]

//	Careful! The CLR uses this version when loading assemblies. 
//	Change this only when you introduce breaking changes.
[assembly: AssemblyVersion(""$assemblyVersion"")]"  | out-file $filename -encoding "ASCII"    
}

function global:Create-AssemblyCompanyProductInfo($company, $product, $filename)
{
write-host
write-host Updating file $filename : 
write-host `t --> $company
write-host `t --> $product 
write-host

"using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyConfiguration("""")]
[assembly: AssemblyCompany(""$company"")]
[assembly: AssemblyProduct(""$product"")]
[assembly: AssemblyCopyright(""Copyright $company"")]
[assembly: AssemblyTrademark("""")]"  | out-file $filename -encoding "ASCII"    
}

function global:Copy-Files($source,$destination,$exclude=@()){    
    Create-Directory $destination
    Get-ChildItem $source -Recurse -Exclude $exclude | Copy-Item -Destination {Join-Path $destination $_.FullName.Substring($source.length)} 
}

function global:Create-Directory($directory_name)
{
	mkdir $directory_name  -ErrorAction SilentlyContinue  | out-null
}

function global:Delete-Directory($directory_name)
{
	rd $directory_name -recurse -force  -ErrorAction SilentlyContinue | out-null
}

function global:Delete-Files-In-Dir($dir, $pattern)
{
	get-childitem $dir -recurse | foreach ($_) {remove-item $_.fullname -include $pattern}
}