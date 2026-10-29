# Build AnoMech, upload latest.zip to GitHub Releases on the PUBLIC repo, push manifest-only branch.
# Source code stays in the PRIVATE repo (tools/push-source.ps1).
# Requires: gh auth login (push access to PublicReleaseRepo)
param(
    [string]$Version = (Select-String -Path "$PSScriptRoot\..\AnoMech\AnoMech.csproj" -Pattern '<Version>([^<]+)</Version>' | ForEach-Object { $_.Matches.Groups[1].Value }),
    [string]$ReleaseNotes = ''
)

. (Join-Path $PSScriptRoot 'repo-settings.ps1')

$ErrorActionPreference = 'Stop'
function Resolve-GhCli {
    $cmd = Get-Command gh -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    $default = Join-Path ${env:ProgramFiles} 'GitHub CLI\gh.exe'
    if (Test-Path $default) { return $default }
    throw 'GitHub CLI (gh) not found. Install with: winget install GitHub.cli'
}
$gh = Resolve-GhCli
$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$out = Join-Path $root 'AnoMech\bin\x64\Debug'
$zip = Join-Path $out 'latest.zip'
$pluginmaster = Join-Path $root $PluginmasterPath
$assetsImages = Join-Path $root $ReleaseAssetsDir 'images'

Write-Host "Building AnoMech $Version (Debug|x64) for public release repo $PublicReleaseRepo ..."
dotnet build (Join-Path $root 'AnoMech\AnoMech.csproj') -c Debug -p:Platform=x64 -p:Version=$Version | Write-Host

$asm = [System.Reflection.AssemblyName]::GetAssemblyName((Join-Path $out 'AnoMech.dll')).Version.ToString()
if ($asm -ne $Version) { throw "Assembly version $asm != expected $Version" }

$lastUpdate = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$utf8 = New-Object System.Text.UTF8Encoding $false
$manifestText = [System.IO.File]::ReadAllText($pluginmaster, $utf8) `
    -replace '"AssemblyVersion": "[^"]*"', "`"AssemblyVersion`": `"$Version`"" `
    -replace '"LastUpdate": "[^"]*"', "`"LastUpdate`": `"$lastUpdate`""
[System.IO.File]::WriteAllText($pluginmaster, $manifestText, $utf8)

if (-not $ReleaseNotes -and $manifestText -match '"Changelog"\s*:\s*"([^"]*)"') {
    $ReleaseNotes = [regex]::Unescape($Matches[1])
}
if (-not $ReleaseNotes) { $ReleaseNotes = "AnoMech $Version" }

$files = @('AnoMech.dll', 'AnoMech.json', 'AnoMech.Relay.dll', 'NAudio.Core.dll', 'NAudio.WinMM.dll', 'NVorbis.dll')
foreach ($f in $files) {
    if (-not (Test-Path (Join-Path $out $f))) { throw "Missing $f in $out" }
}
if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path ($files | ForEach-Object { Join-Path $out $_ }) -DestinationPath $zip -Force
Write-Host "Packed $zip"

& $gh auth setup-git | Out-Null
& $gh auth status | Out-Null

$tag = "v$Version"
$releaseExists = $false
try {
    & $gh release view $tag --repo $PublicReleaseRepo 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) { $releaseExists = $true }
} catch { }

if ($releaseExists) {
    Write-Host "Release $tag exists; uploading latest.zip asset..."
    & $gh release upload $tag $zip --repo $PublicReleaseRepo --clobber
} else {
    & $gh release create $tag $zip --repo $PublicReleaseRepo --title "AnoMech $Version" --notes $ReleaseNotes
}

$iconFile = Join-Path $assetsImages 'icon6464.png'
if (-not (Test-Path $iconFile)) {
    throw "Missing $iconFile. Run bootstrap once or copy icon from the public release branch."
}

$tmpdir = Join-Path $env:TEMP "asumech-manifest-$([Guid]::NewGuid().ToString('N'))"
git clone --depth 1 -b $ReleaseBranch "https://github.com/$PublicReleaseRepo.git" $tmpdir
Copy-Item $pluginmaster (Join-Path $tmpdir 'pluginmaster.json') -Force
New-Item -ItemType Directory -Force -Path (Join-Path $tmpdir 'images') | Out-Null
Copy-Item $iconFile (Join-Path $tmpdir 'images/icon6464.png') -Force
Push-Location $tmpdir
git add pluginmaster.json images/icon6464.png
$status = git status --porcelain
if ($status) {
    git commit -m "Release $Version"
    git push origin $ReleaseBranch
} else {
    Write-Host 'Manifest branch unchanged (already up to date).'
}
Pop-Location
Remove-Item $tmpdir -Recurse -Force

Write-Host 'Done. Custom repo URL (public):'
Write-Host $Script:CustomRepoUrl
Write-Host "Commit updated pluginmaster.json in the PRIVATE repo if needed, then: tools\push-source.ps1"
