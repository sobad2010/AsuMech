using System;
using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace FFXIVClientStructs.FFXIV.Client.Game.Network;

// The API 13 ClientStructs build does not declare these. The TC client's handlers match the
// current signatures, and the layouts below are that handler's packet.
[StructLayout(LayoutKind.Explicit, Size = 0x290)]
public unsafe struct SpawnNpcPacket
{
    [FieldOffset(0x08)] public byte CharacterDataFlags;
    [FieldOffset(0x0D)] public byte LinkRange;
    [FieldOffset(0x10)] public CommonSpawnData Common;
}

[StructLayout(LayoutKind.Explicit, Size = 0x298)]
public unsafe struct SpawnPlayerPacket
{
    [FieldOffset(0x14)] public ushort CurrentWorldId;
    [FieldOffset(0x16)] public ushort HomeWorldId;
    [FieldOffset(0x20)] public CommonSpawnData Common;
}

[StructLayout(LayoutKind.Explicit, Size = 0x278)]
public unsafe struct CommonSpawnData
{
    [FieldOffset(0x00)] public uint TargetId;
    [FieldOffset(0x10)] public WeaponModelId MainhandWeaponModel;
    [FieldOffset(0x18)] public WeaponModelId OffhandWeaponModel;
    [FieldOffset(0x30)] public uint BaseId;
    [FieldOffset(0x34)] public uint NameId;
    [FieldOffset(0x38)] public uint LayoutId;
    [FieldOffset(0x3C)] public uint ObjectType;
    [FieldOffset(0x40)] public EventId EventId;
    [FieldOffset(0x44)] public uint OwnerId;
    [FieldOffset(0x48)] public uint TetherTargetId;
    [FieldOffset(0x4C)] public uint MaxHealthPoints;
    [FieldOffset(0x50)] public uint HealthPoints;
    [FieldOffset(0x54)] public uint DisplayFlags;
    [FieldOffset(0x5A)] public ushort MaxResourcePoints;
    [FieldOffset(0x5C)] public ushort ResourcePoints;
    [FieldOffset(0x60)] public ushort ModelChara;
    [FieldOffset(0x62)] public ushort Rotation;
    [FieldOffset(0x6E)] public byte SpawnIndex;
    [FieldOffset(0x6F)] public CharacterModes CharacterMode;
    [FieldOffset(0x71)] public ObjectKind ObjectKind;
    [FieldOffset(0x72)] public byte SubKind;
    [FieldOffset(0x75)] public byte Battalion;
    [FieldOffset(0x76)] public byte Level;
    [FieldOffset(0x77)] public byte ClassJobId;
    [FieldOffset(0x82)] public byte ModelScaleId;
    [FieldOffset(0x83)] public byte ModelState;
    [FieldOffset(0x84)] public byte ModelAttributeFlags;
    [FieldOffset(0x1F0)] public Vector3 Position;
    [FieldOffset(0x252)] public CustomizeData CustomizeData;

    [StructLayout(LayoutKind.Explicit, Size = 0xC)]
    public struct StatusEffect
    {
        [FieldOffset(0x0)] public ushort StatusId;
        [FieldOffset(0x2)] public ushort Param;
        [FieldOffset(0x4)] public uint SourceObjectId;
    }
}

[StructLayout(LayoutKind.Explicit, Size = 0x40)]
public struct SpawnObjectPacket
{
    [FieldOffset(0x00)] public byte ObjectIndex;
    [FieldOffset(0x01)] public byte ObjectKind;
    [FieldOffset(0x02), Obsolete("Retail sends 4 or 5, not the 0/1 IsTargetable assumes.")]
    public byte TargetableStatus;
    [FieldOffset(0x03)] public bool IsHidden;
    [FieldOffset(0x04)] public uint BaseId;
    [FieldOffset(0x08)] public uint EntityId;
    [FieldOffset(0x0C)] public uint LayoutId;
    [FieldOffset(0x10)] public EventId EventId;
    [FieldOffset(0x14)] public uint OwnerId;
    [FieldOffset(0x18)] public uint GimmickId;
    [FieldOffset(0x1C)] public float Radius;
    [FieldOffset(0x22)] public ushort Rotation;
    [FieldOffset(0x24)] public ushort FateId;
    [FieldOffset(0x26)] public byte EventState;
    [FieldOffset(0x30)] public uint Arg2;
    [FieldOffset(0x34)] public float PositionX;
    [FieldOffset(0x38)] public float PositionY;
    [FieldOffset(0x3C)] public float PositionZ;
}
