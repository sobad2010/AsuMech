using AnoMech.Core.Game;
using AnoMech.Core.Native.Interfaces;
using System.Numerics;

namespace AnoMech.Core.Native.Implementations;

// This ClientStructs build has no VfxObject.Create. Omen spawn stays off until that entry point is bound.
internal sealed class VfxFunctions : IVfxFunctions
{
    private static bool logged;

    public IStaticVfxProxy? SpawnStatic(string path, Placement placement, Vector3 scale)
    {
        if (!logged)
        {
            Plugin.Log.Warning("Static VFX spawn is not available on this client.");
            logged = true;
        }
        return null;
    }
}
