# Push source code to the private repo only (never updates the public release repo).
param(
    [string]$Branch = '',
    [switch]$SetUpstream
)

. (Join-Path $PSScriptRoot 'repo-settings.ps1')

$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..')
Push-Location $root
try {
    if (-not $Branch) {
        $Branch = (git rev-parse --abbrev-ref HEAD).Trim()
    }
    if ($Branch -eq 'HEAD' -or -not $Branch) { throw 'Could not determine branch; pass -Branch.' }

    $remotes = git remote
    if ($SourceRemote -notin $remotes) {
        throw "Remote '$SourceRemote' missing. Add the private repo, e.g.: git remote add $SourceRemote https://github.com/$SourceRepo.git"
    }

    Write-Host "Pushing $Branch -> $SourceRemote ($SourceRepo) ..."
    if ($SetUpstream) {
        git push -u $SourceRemote $Branch
    } else {
        git push $SourceRemote $Branch
    }
    Write-Host "Source push done. Publish binaries with: tools\publish-plugin-release.ps1"
} finally {
    Pop-Location
}
