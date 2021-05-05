module.exports = {
    verifyConditions: [
        //     // Verifies a NUGET_TOKEN environment variable has been provided
        //     () => {
        //         if (!process.env.NUGET_TOKEN) {
        //             throw new SemanticReleaseErrorc(
        //                 'The environment variable NUGET_TOKEN is required.',
        //                 'ENOAPMTOKEN',
        //             )
        //         }
        //     },
        //     // Verifies the conditions for the plugins used below
        //     // For example verifying a GITHUB_TOKEN environment variable has been provided
        '@semantic-release/changelog',
        '@semantic-release/git',
        //     '@semantic-release/github',
    ],
    prepare: [

        '@semantic-release/changelog',
        '@semantic-release/git',
    ],
    publish: [

        // https://github.com/semantic-release/git
        // Git plugin is need so the changelog file will be committed to the Git repository and available on subsequent builds in order to be updated.
        //'@semantic-release/git',

        // https://github.com/semantic-release/git
        // Exec plugin uses to call dotnet nuget push to push the packages from
        // the artifacts folder to NuGet

        // https://github.com/semantic-release/github
        // Set of Semantic-release plugins for publishing a GitHub release.
        // Includes the packages from the artifacts folder as assets
        // {
        //     path: '@semantic-release/github',
        //     assets: 'artifacts/**/*.nupkg',
        // },
    ],
    verifyRelease: [
        {
          "path": "@semantic-release/exec",
          "cmd": "echo ${nextRelease.version} > semver.txt",
        }
     ],
    plugins: [
        ["@semantic-release/commit-analyzer", {
            "releaseRules": [
                { "type": "major", "release": "major" },
                { "type": "release", "release": "major" },
            ],
            "parserOpts": {
                "noteKeywords": ["BREAKING CHANGE", "BREAKING CHANGES"]
            }
        }],
        "@semantic-release/release-notes-generator",
        "@semantic-release/changelog", {
            "changelogFile": "CHANGELOG.md",
        },
        "@semantic-release/exec"
    ],
    branches: [
      {name: 'master'},
      {name: 'beta', channel: 'beta', prerelease: true},
      {name: 'preview', channel: 'beta', prerelease: true}
    ]
}
