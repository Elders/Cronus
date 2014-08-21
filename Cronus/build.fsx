#I @"./bin/tools/FAKE/tools/"
#r @"./bin/tools/FAKE/tools/FakeLib.dll"
#load @"./bin/tools/SourceLink.Fake/tools/SourceLink.fsx"

open Fake
open Fake.Git
open Fake.FSharpFormatting
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open System
open System.IO

let release = LoadReleaseNotes "RELEASE_NOTES.md"

let projectName = "Cronus"
let projectSummary = "CQRS + EvetStore framework"
let projectDescription = "CQRS + EvetStore framework"
let projectAuthors = ["Nikolai Mynkow"; "Simeon Dimov";]

let packages = ["Cronus", projectDescription]

let buildDir  = @"./bin/Release"
let nugetDir = "./bin/nuget"
let nugetDependencies = getDependencies "./src/Elders.Cronus/packages.config"
let excludeLog4net (path : string) = path.Contains "log4net" |> not

Target "Clean" (fun _ -> CleanDirs [buildDir])

Target "AssemblyInfo" (fun _ ->
    CreateCSharpAssemblyInfo @"./src/Elders.Cronus/Properties/AssemblyInfo.cs"
           [Attribute.Title "Elders.Cronus"
            Attribute.Description "Elders.Cronus"
            Attribute.Guid "b422fc04-22de-40ec-978f-bf89fc017411"
            Attribute.Product "Elders.Cronus"
            Attribute.Version release.AssemblyVersion
            Attribute.InformationalVersion release.AssemblyVersion
            Attribute.FileVersion release.AssemblyVersion]
)

Target "Build" (fun _ ->
    !! @"./src/*.sln" 
        |> MSBuildRelease null "Build"
        |> Log "Build-Output: "
)

Target "RestorePackages" (fun _ ->
     "./src/Elders.Cronus/packages.config"
     |> RestorePackage (fun p -> { p with OutputPath = "./src/packages" })
)

Target "CreateNuGet" (fun _ ->
    for package,description in packages do
    
        let nugetToolsDir = nugetDir @@ "lib" @@ "net40-full"
        CleanDir nugetToolsDir

        match package with
        | p when p = projectName ->
            CopyDir nugetToolsDir (buildDir @@ ("Elders." + package)) excludeLog4net
        !! (nugetToolsDir @@ "*.srcsv") |> DeleteFiles

        NuGet (fun p ->
            {p with
                Authors = projectAuthors
                Project = package
                Description = description
                Version = release.NugetVersion
                Summary = projectSummary
                ReleaseNotes = release.Notes |> toLines
                Dependencies = nugetDependencies
                AccessKey = getBuildParamOrDefault "nugetkey" ""
                Publish = hasBuildParam "nugetkey"
                ToolPath = "./tools/NuGet/nuget.exe"
                OutputPath = nugetDir
                WorkingDir = nugetDir }) "Multithreading.Scheduler.nuspec"
)

Target "Release" (fun _ ->
    StageAll ""
    Commit "" (sprintf "Bump version to %s" release.NugetVersion)
    Branches.push ""

    Branches.tag "" release.NugetVersion
    Branches.pushTag "" "origin" release.NugetVersion
)

// Dependencies
"Clean"
    ==> "RestorePackages"
    ==> "AssemblyInfo"
    ==> "Build"
    ==> "CreateNuGet"
    ==> "Release"
 
// start build
RunParameterTargetOrDefault "target" "Build"