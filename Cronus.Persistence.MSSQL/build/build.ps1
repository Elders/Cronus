properties {
	$base_directory = Resolve-Path ..
	$src_directory = "$base_directory\src"
 	
	$sln="Elders.Cronus.Persistence.MSSQL.sln"
	
	$config = "debug"; #debug or release or stage
	
	$company="Elders"
	$product="Cronus.Persistence.MSSQL"

	$assemblyFileVersion = "1.0.?";
	$assemblyVersion = "1.0.0.0";
	$assemblyRevision = "18";

	$nugetSourceDir = "Elders.Cronus.Persistence.MSSQL"
	$nugetSourceFiles = @("Elders.Cronus.Persistence.MSSQL.dll", "Elders.Cronus.Persistence.MSSQL.pdb")
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
        -licenseUrl https://github.com/Elders/Cronus.Persitence.MSSQL/blob/master/LICENSE `
        -projectUrl https://github.com/Elders/Cronus.Persitence.MSSQL `
        -product $product `
        -version  $version `
        -description "MSSQL persistence for Cronus" `
		-dependencies @(
						,@("Cronus.DomainModelling", "[1.0.7, 1.1.0)")
						,@("Cronus", "[1.1.30, 1.2.0)")
						,@("log4net", "[2.0.3, 2.0.4)")
						,@("RabbitMQ.Client", "[3.2.1, 3.4)")
					   ) `
        -file "$nugetDeploy\Product.nuspec" `

    Nuget-BuildPackage $nugetSourceDir $nugetSourceFiles

    Nuget-PushPackage $version
}