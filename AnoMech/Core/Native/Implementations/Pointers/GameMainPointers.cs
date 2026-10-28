using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AnoMech.Core.Native.Implementations.Pointers;

internal unsafe class GameMainPointers
{
    [Signature("40 55 41 54 41 55 41 56 41 57 48 83 EC 60 4C 8B F1 45 0F B6 E9", UseFlags = SignatureUseFlags.Pointer, ScanType = ScanType.Text)]
    public static LoadZoneDelegate LoadZone = null!;

    public delegate void LoadZoneDelegate(GameMain* thisPtr, uint territoryTypeId, uint transitionTerritoryFilterKey, byte a4, byte a5);

    public static void Initialize()
    {
        Plugin.GameInterop.InitializeFromAttributes(new GameMainPointers());
    }
}
