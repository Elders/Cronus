properties {
	$base_directory = Resolve-Path ..
	$src_directory = "$base_directory\src"
 	
	$sln="NMSD.Cronus.sln"
	
	$config = "debug"; #debug or release or stage
	
	$company="NMSD"
	$product="Cronus"
 
	$assemblyInformationalVersion = "dev";
	$assemblyFileVersion = "0.0.?.0";
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
    Create-AssemblyVersionInfo `
        -assemblyVersion $assemblyVersion `
        -assemblyFileVersion $assemblyFileVersion.Replace("?",$assemblyRevision) `
        -file $src_directory\AssemblyVersionInfo.cs `

    Create-AssemblyCompanyProductInfo `
        -company $company `
        -product $product `
        -file $src_directory\AssemblyCompanyProductInfo.cs `
}

task Build -depends ValidateConfig, AssemblyInfo -description "Builds outdated source files" {
    Build "$src_directory\$sln" $config
}

task PublishNugetPackage {
    $version = $assemblyFileVersion.Replace("?",$assemblyRevision)
    $version = "$version-Beta"
    Create-ProductNuspec `
        -authors "Nikolai Mynkow, Simeon Dimov" `
        -owners "Nikolai Mynkow, Simeon Dimov" `
        -copyright NMSD `
        -requireLicenseAcceptance false `
        -licenseUrl https://github.com/NMSD/Cronus/blob/master/LICENSE `
        -projectUrl http://nmsd.github.io/Cronus `
        -product $product `
        -version  $version `
        -description "Simple CQRS + EvetStore framework" `
        -file "$base_directory\.nuget\Product.nuspec" `

    BuildNugetPackage $nugetSourceDir $nugetDeployDir

    PushNugetPackage $version
}

######################################################################################################

function BuildNugetPackage([string] $nugetSourceDir, [string] $nugetDeployDir)
{
    Delete-Directory $nugetDeployDir
    Create-Directory $nugetDeployDir
    $binariesToNuget = "$base_directory\bin\$config\$nugetSourceDir\*"
    Copy-Item $binariesToNuget $nugetDeployDir
    
    cd $base_directory\.nuget\

    & ".\NuGet.exe" pack Product.nuspec -IncludeReferencedProjects
    Delete-Directory $nugetDeployDir
    Remove-Item "$base_directory\.nuget\*" -include *.nuspec
}

function PushNugetPackage([string] $version)
{
    cd $base_directory\.nuget\
    $package = "$product.$version.nupkg"
    & ".\NuGet.exe" push $package
    Remove-Item "$base_directory\.nuget\*" -include *.nupkg
}

function Build([string] $sln_file, [string] $config)
{
    write-host The build is now running...
    write-host
    write-host Current solution: `t --> $sln_file
    write-host Current config: `t --> $config
    write-host
    
    exec { msbuild /nologo /verbosity:minimal $sln_file /t:Clean /p:Configuration=$config /m }
    exec { msbuild /nologo /verbosity:minimal $sln_file /p:Configuration=$config /m }
    
    write-host The build finished successfully!
}

function global:Create-ProductNuspec
{
    param(
        [string]$product = $(throw "product is a required parameter."),
        [string]$version = $(throw "version is a required parameter."),
        [string]$authors,
        [string]$owners,
        [string]$licenseUrl,
        [string]$projectUrl,
        [string]$requireLicenseAcceptance,
        [string]$description = $(throw "version is a required parameter."),
        [string]$releaseNotes,
        [string]$copyright,
        [string]$tags,
        [string]$file = $(throw "file is a required parameter.")
    )

"<?xml version=""1.0""?>
<package >
  <metadata>
    <id>$product</id>
    <version>$version</version>
    <authors>$authors</authors>
    <owners>$owners</owners>
    <licenseUrl>$licenseUrl</licenseUrl>
    <projectUrl>$projectUrl</projectUrl>
    <requireLicenseAcceptance>$requireLicenseAcceptance</requireLicenseAcceptance>
    <description>$description</description>
    <releaseNotes>$releaseNotes</releaseNotes>
    <copyright>$copyright</copyright>
    <tags>$tags</tags>
  </metadata>
</package>" | out-file $file -encoding "ASCII" 
}

function global:Create-AssemblyVersionInfo
{
    param(
        [string]$assemblyVersion,
        [string]$assemblyFileVersion,
        [string]$file = $(throw "file is a required parameter.")
    )

    $commit = Get-Git-Commit
    $tag = Get-Version-From-Git-Tag
    $assemblyInformationalVersion = "$tag / $commit"

    write-host Updating file $file : 
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
//		GA      - 7400
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
[assembly: AssemblyVersion(""$assemblyVersion"")]"  | out-file $file -encoding "ASCII"    
}

function global:Create-AssemblyCompanyProductInfo
{
    param(
        [string]$company, 
        [string]$product, 
        [string]$copyright,
        [string]$file = $(throw "file is a required parameter.")
    )

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
[assembly: AssemblyCopyright(""$copyright"")]
[assembly: AssemblyTrademark("""")]"  | out-file $file -encoding "ASCII"    
}

function global:Get-Git-Commit
{
    if(Test-Path "$base_directory\.git\"){
        $gitLog = git log --oneline -1
        return $gitLog.Split(' ')[0]
    }
    else
    {
        write-host Cannot find .git directory. May be this is not a GIT repository.
        return ""
    }
}

function global:Get-Version-From-Git-Tag 
{
    if(Test-Path "$base_directory\.git\")
    {
        $gitTag = git describe --tags --abbrev=0
        
        if($gitTag)
        {
            return $gitTag.Replace("v", "") + ".0"
        }
        else
        {
            return ""
        }
    }
    else
    {
        write-host Cannot find .git directory. May be this is not a GIT repository.
        return ""
    }
}

function global:Copy-Files($source,$destination,$exclude=@()) 
{    
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