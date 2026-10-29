# Add git remotes for the two-repo layout (safe to run more than once).
. (Join-Path $PSScriptRoot 'repo-settings.ps1')

$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..')
Push-Location $root
try {
    function Ensure-Remote($name, $url) {
        $list = git remote 2>&1
        if ($list -contains $name) {
            $existing = (git remote get-url $name).Trim()
            if ($existing -ne $url) {
                Write-Host "Updating remote $name -> $url"
                git remote set-url $name $url
            } else {
                Write-Host "Remote $name already set."
            }
        } else {
            Write-Host "Adding remote $name -> $url"
            git remote add $name $url
        }
    }

    $list = @(git remote)
    if ($list -contains 'origin-sobad' -and -not ($list -contains 'release')) {
        Write-Host 'Renaming remote origin-sobad -> release (public manifest repo)'
        git remote rename origin-sobad release
        $list = @(git remote)
    }

    Ensure-Remote $SourceRemote "https://github.com/$SourceRepo.git"
    Ensure-Remote 'release' "https://github.com/$PublicReleaseRepo.git"

    Write-Host ''
    Write-Host 'Remotes:'
    git remote -v
    Write-Host ''
    Write-Host "Private source : git push $SourceRemote <branch>   (or tools\push-source.ps1)"
    Write-Host "Public manifest: tools\publish-plugin-release.ps1   (never push full tree to release remote)"
} finally {
    Pop-Location
}
