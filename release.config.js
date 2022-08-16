module.exports={
  plugins: [
    ["@semantic-release/commit-analyzer", {
        releaseRules: [
            {"type": "major"  , "release": "major"},
            {"type": "release", "release": "major"},
        ],
        parserOpts: {
            "noteKeywords": ["BREAKING CHANGE", "BREAKING CHANGES", "BREAKING"]
        }
    }],

    ["@semantic-release/exec",{
        prepareCmd: `
            set -e
            VER=\${nextRelease.version}
            ##vso[build.updatebuildnumber]\${nextRelease.version}
            dotnet pack "src/$PROJECT_DIR/"*.csproj -o "$STAGING_PATH" -p:Configuration=Release -p:PackageVersion=$VER --verbosity Detailed
        `,
        successCmd: `
            set -e
            echo "##vso[task.setvariable variable=newVer;]yes"
        `,
    }],

    "@semantic-release/release-notes-generator",
    "@semantic-release/changelog",
    "@semantic-release/git"
  ],

  branches: [
    'master',
    {name: 'beta', channel: 'beta', prerelease: true},
    {name: 'preview', channel: 'beta', prerelease: true}
  ],
}
