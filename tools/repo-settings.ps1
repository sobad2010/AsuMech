# Shared repo layout for this fork. Dot-source from tools/*.ps1:
#   . (Join-Path $PSScriptRoot 'repo-settings.ps1')
#
# Source (private): full git history, branches like v15 — never push plugin zips here for public users.
# Release (public):  branch `release` with pluginmaster.json + images/ only; GitHub Releases hold latest.zip.

$Script:SourceRepo = 'Asunnya/AsuMech'
$Script:SourceRemote = 'origin'
$Script:SourceDefaultBranch = 'v15'

$Script:PublicReleaseRepo = 'sobad2010/AsuMech'
$Script:ReleaseBranch = 'release'

$Script:ReleaseAssetsDir = 'release-assets'
$Script:PluginmasterPath = 'pluginmaster.json'

$Script:CustomRepoUrl = "https://raw.githubusercontent.com/$PublicReleaseRepo/$ReleaseBranch/pluginmaster.json"
