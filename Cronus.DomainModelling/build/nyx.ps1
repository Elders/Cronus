#	Parameters you do not want to change
$root = Resolve-Path ..
$src_directory = "$root\src"
$verbosity = "minimal"
$nuget = "$root\build\nuget.exe"
$deploy = "$root\bin\Deploy"
$nugetDeploy = "$deploy\nuget"
$nugetDeployLib = "$deploy\nuget\lib"

function global:Create-WebDeploymentPackage
{
	param(
		[string]$webProject = $(throw "webProject is a required parameter."),
		[string]$publishProfile = $(throw "publishProfile is a required parameter.")
	)
	
	$webProjectDir = [io.path]::GetDirectoryName($webProject)
	$publishProfileFile = "$webProjectDir\Properties\PublishProfiles\$publishProfile"
		
	DrawParamsTable `
		-header "Create a deployment package from publish profile" `
		-params @{
					"Web project" = $webProject; 
					"Publish profile" = $publishProfileFile
				}
	
	exec { msbuild /nologo $webProject /verbosity:$verbosity /t:WebPublish /p:PublishProfile=$publishProfileFile }
	
	write-host The deployment package was created successfully!
	write-host
}

function global:DrawParamsTable
{
	param(
		[string]$header = $(throw "header is a required parameter."),
		[HashTable]$params = $(throw "$params is a required parameter. ex: @{""name1"" = ""value1""; ""name2"" = ""value2""}")
	)

	write-host
	WriteSeparator
	write-host $header
	WriteSeparator

	$a = @{Expression={$_.Name};Label="Name";width=30}, `
	@{Expression={$_.Value};Label="Value";width=300}

	$params | Format-Table $a
}

function global:WriteSeparator
{
	write-host ----------------------------------------------------------------------
}

function global:DeployToServer
{
	param(
		[string]$webProject = $(throw "webProject is a required parameter."),
		[string]$server = $(throw "server is a required parameter."),
		[string]$username = $(throw "username is a required parameter."),
		[string]$password = $(throw "password is a required parameter.")
	)
	
	$webProjectName = [io.path]::GetFileNameWithoutExtension($webProject)
	write-host $webProjectName
	DrawParamsTable `
		-header "Deploy package to server" `
		-params @{
					"Web project" = $webProject; 
					"Server" = $server;
					"Username" = $username
				}
	
	exec {& "$deploy\$webProjectName.deploy.cmd" /Y /M:$server /U:$username /P:$password }
	
	write-host The package was deployed successfully!
	write-host
}

function Nuget-BuildPackage
{
	param(
        [string]$nugetSourceDir = $(throw "nugetSourceDir is a required parameter."),
        [array]$nugetSourceFiles = $(throw "nugetSourceFiles is a required parameter.")
    )
	
    Delete-Directory $nugetDeployLib
    Create-Directory $nugetDeployLib
    $binariesToPack = "$root\bin\$config\$nugetSourceDir\*"
	
	Copy-Files $binariesToPack $nugetDeployLib $nugetSourceFiles
    
    exec {& "$nuget" pack "$nugetDeploy\Product.nuspec" -OutputDirectory "$nugetDeploy" -IncludeReferencedProjects -NonInteractive}
    #Delete-Directory $nugetDeployDir
    #Remove-Item "$root\.nuget\*" -include *.nuspec
}

function Nuget-PushPackage([string] $version)
{
    $package = "$nugetDeploy\$product.$version.nupkg"
    exec {& "$nuget" push "$package" }
    #Remove-Item "$root\.nuget\*" -include *.nupkg
}

function Build
{
	param(
        [string]$sln_file = $(throw "sln_file is a required parameter."),
        [string]$config = $(throw "config is a required parameter.")
    )

	write-host
    write-host Restoring nuget packages...
	WriteSeparator
	exec {& "$nuget" restore "$sln_file" -NonInteractive}
	
	DrawParamsTable	`
		-header:"Build configuration" `
		-params @{
					"Solution" = $sln_file;
					"Configuration" = $config
				}
	
	write-host
	write-host The build is now running...
	WriteSeparator
	msbuild /version
	write-host
	WriteSeparator
    exec {msbuild /nologo /verbosity:$verbosity $sln_file /t:Clean /p:Configuration=$config /m}
    exec {msbuild /nologo /verbosity:$verbosity $sln_file /p:Configuration=$config /m}
	
    write-host The build finished successfully!
}

function global:Nuget-CreateNuspec
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
        [string]$file = $(throw "file is a required parameter."),
		[array]$dependencies
    )
	
	if($dependencies)
	{
		$dependenciesTags = "<dependencies>"
		$generated = $dependencies | % { "<dependency id=""" + $_[0] + """ version=""" + $_[1] + """ />" }
		$dependenciesTags = $dependenciesTags + $generated + "</dependencies>"
	}
	
	$nugetDeployDir = [io.path]::GetDirectoryName($file)
	Create-Directory $nugetDeployDir
	
	DrawParamsTable `
		-header $file `
		-params @{
					"Product" = $product;
					"Description" = $description;
					"Version" = $version;
					"Authors" = $authors;
					"Owners" = $owners;
					"LicenseUrl" = $licenseUrl;
					"Require License Acceptance" = $requireLicenseAcceptance;
					"ProjectUrl" = $projectUrl;
					"Release Notes" = $releaseNotes;
					"Copyright" = $copyright;
					"Tags" = $tags;
				}

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
	$dependenciesTags
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

	DrawParamsTable `
		-header $file `
		-params @{
					"AssemblyInformationalVersion" = $assemblyInformationalVersion; 
					"AssemblyFileVersion" = $assemblyFileVersion;
					"AssemblyVersion" = $assemblyVersion
				}

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
	
	DrawParamsTable `
		-header $file `
		-params @{
					"Company" = $company; 
					"Product" = $product
				}

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
    if(Test-Path "$root\.git\"){
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
    if(Test-Path "$root\.git\")
    {
        return git describe --tags --abbrev=0 --always
    }
    else
    {
        write-host Cannot find .git directory. May be this is not a GIT repository.
        return ""
    }
}

function global:Copy-Files($source,$destination,$include=@()) 
{    
    Create-Directory $destination
    Get-ChildItem $source -Recurse -Include $include | Copy-Item -Destination { Join-Path $destination $_.FullName.Substring($source.length - 1)} 
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