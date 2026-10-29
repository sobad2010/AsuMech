# One-time: replace the public repo's release branch with manifest-only content (no source history on that branch).
# Requires: gh auth, push access to PublicReleaseRepo. Does NOT touch the private source repo.
param(
    [switch]$Force
)

. (Join-Path $PSScriptRoot 'repo-settings.ps1')

$ErrorActionPreference = 'Stop'
function Resolve-GhCli {
    $cmd = Get-Command gh -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    $default = Join-Path ${env:ProgramFiles} 'GitHub CLI\gh.exe'
    if (Test-Path $default) { return $default }
    throw 'GitHub CLI (gh) not found.'
}
$gh = Resolve-GhCli

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$assets = Join-Path $root $ReleaseAssetsDir
$manifest = Join-Path $root $PluginmasterPath
if (-not (Test-Path $manifest)) { throw "Missing $PluginmasterPath at repo root." }
$icon = Join-Path $assets 'images/icon6464.png'
if (-not (Test-Path $icon)) { throw "Missing $icon — copy the icon into $ReleaseAssetsDir/images/." }

if (-not $Force) {
    Write-Host "This will FORCE-PUSH branch '$ReleaseBranch' on $PublicReleaseRepo to a manifest-only tree."
    Write-Host 'Only pluginmaster.json and release-assets/images/ will remain on that branch.'
    $confirm = Read-Host 'Type YES to continue'
    if ($confirm -ne 'YES') { throw 'Aborted.' }
}

$work = Join-Path $env:TEMP "asumech-bootstrap-$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Force -Path $work | Out-Null
try {
    Copy-Item $manifest (Join-Path $work 'pluginmaster.json') -Force
    New-Item -ItemType Directory -Force -Path (Join-Path $work 'images') | Out-Null
    Copy-Item $icon (Join-Path $work 'images/icon6464.png') -Force

    Push-Location $work
    git init | Out-Null
    git checkout -b $ReleaseBranch | Out-Null
    git add pluginmaster.json images/icon6464.png
    git commit -m "Manifest-only release branch (no source)" | Out-Null

    & $gh auth setup-git | Out-Null
    git remote add origin "https://github.com/$PublicReleaseRepo.git"
    git push --force origin "${ReleaseBranch}:${ReleaseBranch}"
    Write-Host "Public release branch updated: https://github.com/$PublicReleaseRepo/tree/$ReleaseBranch"
    Write-Host $Script:CustomRepoUrl
} finally {
    Pop-Location
    Remove-Item $work -Recurse -Force -ErrorAction SilentlyContinue
}
