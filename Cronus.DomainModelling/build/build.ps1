properties {
	$base_directory = Resolve-Path ..
	$src_directory = "$base_directory\src"
 	
	$sln="Elders.Cronus.DomainModelling.sln"
	
	$config = "debug"; #debug or release or stage
	
	$company="Elders"
	$product="Cronus.DomainModelling"
 
	$assemblyFileVersion = "1.0.?";
	$assemblyVersion = "1.0.0.0";
	$assemblyRevision = "8";

	$nugetSourceDir = "Elders.Cronus.DomainModelling"
	$nugetSourceFiles = @("Elders.Cronus.DomainModelling.dll", "Elders.Cronus.DomainModelling.pdb")
}

. ".\nyx.ps1"

task build -depends Init, AssemblyInfo, ValidateConfig, BuildCronus
task nuget -depends Init, AssemblyInfo, ValidateConfig, BuildCronus, PublishNugetPackage

task Init {
	Delete-Directory("$root\bin")
}

task ValidateConfig {
    assert ( "debug", "release" -contains $config) ` "Invalid config: $config; valid values are debug or release";
}

task AssemblyInfo {
    Create-AssemblyVersionInfo `
        -assemblyVersion $assemblyVersion `
        -assemblyFileVersion $assemblyFileVersion.Replace("?",$assemblyRevision) `
        -file $src_directory\AssemblyVersionInfo.cs

    Create-AssemblyCompanyProductInfo `
        -company $company `
        -product $product `
        -file $src_directory\AssemblyCompanyProductInfo.cs
}

task BuildCronus -depends ValidateConfig, AssemblyInfo -description "Builds outdated source files" {
    Build "$src_directory\$sln" $config
}

task PublishNugetPackage {
    $version = $assemblyFileVersion.Replace("?",$assemblyRevision)
    $version = "$version"
    Nuget-CreateNuspec `
        -authors "Nikolai Mynkow, Simeon Dimov" `
        -owners "Nikolai Mynkow, Simeon Dimov" `
        -copyright Elders `
        -requireLicenseAcceptance false `
        -licenseUrl https://github.com/Elders/Cronus.DomainModelling/blob/master/LICENSE `
        -projectUrl https://github.com/Elders/Cronus.DomainModelling `
        -product $product `
        -version  $version `
        -description "DDD modelling for Cronus" `
        -file "$nugetDeploy\Product.nuspec" `

    Nuget-BuildPackage $nugetSourceDir $nugetSourceFiles

    Nuget-PushPackage $version
}