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
        successCmd: `
          set -e
          echo \${nextRelease.version} > semver.txt
        `,
    }],

    "@semantic-release/release-notes-generator",
    "@semantic-release/changelog",
    "@semantic-release/git"
  ],

  branches: [
    {name: 'master'},
    {name: 'beta', channel: 'beta', prerelease: true},
    {name: 'preview', channel: 'beta', prerelease: true}
  ],
}
