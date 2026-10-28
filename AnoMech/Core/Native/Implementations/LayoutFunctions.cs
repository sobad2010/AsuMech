using System.Numerics;
using AnoMech.Core.Native.Implementations.Interop;
using AnoMech.Core.Native.Interfaces;

namespace AnoMech.Core.Native.Implementations;

internal sealed unsafe class LayoutFunctions : ILayoutFunctions
{
    public int DisableSpawnAreaColliders(Vector3 center, float radius) => DirectorFunctions.DisableSpawnAreaColliders(center, radius);

    public void PlaySharedGroupTimeline(string sgbPath, uint index)
    {
        _ = sgbPath;
        _ = index;
    }

    public string DescribeActiveLayers() => LayoutQuery.DescribeActiveLayers();

    public string DescribeLiveEffects() => LayoutQuery.DescribeLiveEffects();
}
