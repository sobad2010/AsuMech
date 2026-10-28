# Build AnoMech (FFXIVSimpleLauncher / 繁中 Dalamud API 13), pack latest.zip, push pluginmaster, GitHub Release.
# Requires: gh auth login (push access to the target repo)
param(
    [string]$Version = (Select-String -Path "$PSScriptRoot\..\AnoMech\AnoMech.csproj" -Pattern '<Version>([^<]+)</Version>' | ForEach-Object { $_.Matches.Groups[1].Value }),
    [string]$Repo = "sobad2010/AsuMech",
    [string]$ManifestBranch = "release"
)

$ErrorActionPreference = "Stop"
$root = Resolve-Path "$PSScriptRoot\.."
$out = Join-Path $root "AnoMech\bin\x64\Debug"
$zip = Join-Path $out "latest.zip"
$pluginmaster = Join-Path $root "pluginmaster.json"

Write-Host "Building AnoMech $Version (Debug|x64, Dalamud from FFXIVSimpleLauncher)..."
dotnet build (Join-Path $root "AnoMech\AnoMech.csproj") -c Debug -p:Platform=x64 -p:Version=$Version | Write-Host

$asm = [System.Reflection.AssemblyName]::GetAssemblyName((Join-Path $out "AnoMech.dll")).Version.ToString()
if ($asm -ne $Version) { throw "Assembly version $asm != expected $Version" }

$lastUpdate = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$utf8 = New-Object System.Text.UTF8Encoding $false
$manifestText = [System.IO.File]::ReadAllText($pluginmaster, $utf8) `
    -replace '"AssemblyVersion": "[^"]*"', "`"AssemblyVersion`": `"$Version`"" `
    -replace '"LastUpdate": "[^"]*"', "`"LastUpdate`": `"$lastUpdate`""
[System.IO.File]::WriteAllText($pluginmaster, $manifestText, $utf8)

$files = @("AnoMech.dll", "AnoMech.json", "AnoMech.Relay.dll", "NAudio.Core.dll", "NAudio.WinMM.dll", "NVorbis.dll")
foreach ($f in $files) {
    if (-not (Test-Path (Join-Path $out $f))) { throw "Missing $f in $out" }
}
if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path ($files | ForEach-Object { Join-Path $out $_ }) -DestinationPath $zip -Force
Write-Host "Packed $zip"

gh auth status | Out-Null

$tag = "v$Version"
$releaseExists = $false
try {
    & gh release view $tag --repo $Repo 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) { $releaseExists = $true }
} catch { }

if ($releaseExists) {
    Write-Host "Release $tag exists; uploading latest.zip asset..."
    & gh release upload $tag $zip --repo $Repo --clobber
} else {
    & gh release create $tag $zip --repo $Repo --title "AnoMech $Version" --notes "Dalamud API 13 build for 繁中客户端."
}

$tmpdir = Join-Path $env:TEMP "asumech-manifest-$([Guid]::NewGuid().ToString('N'))"
git clone --depth 1 -b $ManifestBranch "https://github.com/$Repo.git" $tmpdir
Copy-Item $pluginmaster (Join-Path $tmpdir "pluginmaster.json") -Force
Push-Location $tmpdir
git add pluginmaster.json
git commit -m "Release $Version"
git push origin $ManifestBranch
Pop-Location
Remove-Item $tmpdir -Recurse -Force

Write-Host "Done. Custom repo URL:"
Write-Host "https://raw.githubusercontent.com/$Repo/$ManifestBranch/pluginmaster.json"
