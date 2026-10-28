using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace AnoMech.Core.Native.Implementations.Interop;

// The local player's displayed name during a practice stay. The account name is left alone
// and put back when the stay ends.
internal static unsafe class LocalPlayerName
{
    public const string Display = "其他角色01";

    public static bool Shown { get; private set; }

    private static string? original;
    private static string? originalCompanyTag;

    public static void Show()
    {
        var obj = Local();
        if (obj == null) return;
        var chara = (Character*)obj;
        if (!Shown)
        {
            original = obj->NameString;
            originalCompanyTag = chara->FreeCompanyTagString;
        }
        Shown = true;
        if (obj->NameString != Display)
            obj->NameString = Display;
        if (chara->FreeCompanyTagString.Length != 0)
            chara->FreeCompanyTagString = "";
    }

    public static void Hide()
    {
        if (!Shown) return;
        Shown = false;
        var obj = Local();
        if (obj != null)
        {
            if (original != null)
                obj->NameString = original;
            if (originalCompanyTag != null)
                ((Character*)obj)->FreeCompanyTagString = originalCompanyTag;
        }
        original = null;
        originalCompanyTag = null;
    }

    private static GameObject* Local()
    {
        if (Plugin.ObjectTable.LocalPlayer is not { Address: var addr } || addr == nint.Zero) return null;
        return (GameObject*)addr;
    }
}
