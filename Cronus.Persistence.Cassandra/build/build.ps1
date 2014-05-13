properties {
	$base_directory = Resolve-Path ..
	$src_directory = "$base_directory\src"
 	
	$sln="Elders.Cronus.Persistence.Cassandra.sln"
	
	$config = "debug"; #debug or release or stage
	
	$company="Elders"
	$product="Cronus.Persistence.Cassandra"
 
	$assemblyFileVersion = "1.0.?";
	$assemblyVersion = "1.0.0.0";
	$assemblyRevision = "0";

	$nugetSourceDir = "Elders.Cronus.Persistence.Cassandra"
	$nugetSourceFiles = @("Elders.Cronus.Persistence.Cassandra.dll", "Elders.Cronus.Persistence.Cassandra.pdb")
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
        -licenseUrl https://github.com/Elders/Cronus/blob/master/LICENSE `
        -projectUrl http://Elders.github.io/Cronus `
        -product $product `
        -version  $version `
        -description "MSSQL persistence for Cronus" `
		-dependencies @(
						,@("Cronus.DomainModelling", "[1.0.6, 1.1.0)")
						,@("Cronus", "[1.1.25, 1.2.0)")
						,@("Protoreg", "[1.0, 1.1)")
						,@("log4net", "[2.0.3, 2.0.4)")
						,@("RabbitMQ.Client", "[3.2.1, 3.4)")
					   ) `
        -file "$nugetDeploy\Product.nuspec" `

    Nuget-BuildPackage $nugetSourceDir $nugetSourceFiles

    Nuget-PushPackage $version
}