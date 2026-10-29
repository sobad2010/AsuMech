# Two-repo release (private source + public plugin)

## Layout

| | Repo | Visibility | Git remote (local) |
|---|------|------------|-------------------|
| **Source** | `Asunnya/AsuMech` | **Private** | `origin` |
| **Release** | `sobad2010/AsuMech` | **Public** | `release` (fetch only; publish via script) |

Public repo branch `release` contains only:

- `pluginmaster.json`
- `images/icon6464.png`

Binaries: GitHub Releases `latest.zip` on the public repo.

Dalamud custom repo URL (unchanged):

```
https://raw.githubusercontent.com/sobad2010/AsuMech/release/pluginmaster.json
```

Edit repo names in `tools/repo-settings.ps1` if yours differ.

## One-time setup

```powershell
tools\setup-remotes.ps1
```

Make the **source** repo private on GitHub (Settings → Danger zone).  
If the public repo still has full source history on `v15`, reset the public `release` branch to manifest-only:

```powershell
tools\bootstrap-public-release.ps1   # type YES to confirm force-push
```

Stop pushing code branches to the public remote (`git push origin-sobad v15` etc.).

On GitHub, open the **public** repo and delete branches that contain source (e.g. `v15`, `main`) so only `release` remains public-facing. The private repo keeps all branches.

## Daily workflow

1. Develop on `v15` (or your branch).
2. Bump version / `pluginmaster.json` in the **private** repo.
3. Push source only:

   ```powershell
   tools\push-source.ps1
   ```

4. Publish plugin (build + Release + public `release` branch):

   ```powershell
   tools\publish-plugin-release.ps1
   ```

5. Commit any `pluginmaster.json` / `LastUpdate` changes from step 4, then `tools\push-source.ps1` again.

Never `git push release v15` — the public repo should not receive source branches.
