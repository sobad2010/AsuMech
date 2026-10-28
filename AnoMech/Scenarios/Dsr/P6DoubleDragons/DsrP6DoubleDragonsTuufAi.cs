using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AnoMech.Core.Game.Ai;
using AnoMech.Core.Native.Interfaces;
using AnoMech.Core.Game.Party;
using AnoMech.Core.SimObjects;
using static AnoMech.Core.Game.Party.PartyRole;

namespace AnoMech.Scenarios.Dsr.P6DoubleDragons;

public sealed class DsrP6DoubleDragonsTuufAi : IScenarioAi<DsrP6DoubleDragonsState>
{
    public string Name => "TUUF (5-1 Wroth)";

    private static readonly PartyRole[] GroupOne = [MainTank, RegenHealer, MeleeDpsA, PhysRangedDps];
    private static readonly PartyRole[] GroupTwo = [OffTank, ShieldHealer, MeleeDpsB, CasterDps];
    private static readonly Vector2 FrontPairSpot = new(0f, 7.7f);
    private static readonly Vector2 WestPairSpot = new(-5.5f, 18.3f);
    private static readonly Vector2 EastPairSpot = new(5f, 18.3f);
    private const float PairOffset = 0.4f;
    private static readonly Vector2 MainTankCornerSpot = new(-12.5f, -14.5f);
    private static readonly Vector2 OffTankCornerSpot = new(12.5f, -14.5f);
    private static readonly Vector2 SharedBusterSpot = new(0f, -9f);
    private const float VowSpreadRadius = 8f;
    private static readonly PartyRole[] VowSpreadClockwiseFromNorthNorthEast = [MainTank, RegenHealer, ShieldHealer, OffTank, MeleeDpsB, CasterDps, PhysRangedDps, MeleeDpsA];
    private const float NearStackX = 7.5f;
    private const float WallStackX = 17f;
    private const float HotTailLineZ = 13f;
    private const float HotWingStripZ = 2f;
    private static readonly Vector2 SecondWyrmsbreathMainTankSpot = new(-14.8f, 12f);
    private static readonly Vector2 SecondWyrmsbreathOffTankSpot = new(14.3f, 12.1f);
    private static readonly Vector2 WaitForTheDivesSpot = new(0f, -8f);
    private const float DiveLaneStandX = 6.5f;
    private const float DiveLaneStandZ = -8f;
    private const float DiveTankX = 8f;
    private const float DiveTankZ = -20f;
    private static readonly Vector2 NorthWallSpot = new(0f, -20f);

    private DsrP6DoubleDragonsState state = null!;
    private SimWorld world = null!;
    private DsrWrothMarkers markers = null!;

    public void Run(DsrP6DoubleDragonsState stateParam, SimWorld worldParam)
    {
        state = stateParam;
        world = worldParam;
        markers = new DsrWrothMarkers(state.Spreads, state.Stacks);
        var ai = new AiManager(world);

        Move(ai, 0.3f, () => AiMove.Create(PrePositionForTethers()).NaturalOrder());
        Move(ai, DsrP6DoubleDragonsScenario.TethersOut + 0.4f, () => AiMove.Create(PairUpWithOppositeTethers()).NaturalOrder(), jitter: 0f, sprint: true);
        Move(ai, DsrP6DoubleDragonsScenario.BreathsLand + 0.5f, () => AiMove.Create(EveryoneSpreadsForMortalVow()).NaturalOrder(), jitter: 0f);
        Move(ai, DsrP6DoubleDragonsScenario.VowApplied + 0.4f, () => AiMove.Create(LightPartiesNorthAndSouth()).NaturalOrder(), jitter: 0f);
        Move(ai, DsrP6DoubleDragonsScenario.AkhAfahLands + 0.6f, () => AiMove.Create(Cluster(Vector2.Zero, Enum.GetValues<PartyRole>())).NaturalOrder(), jitter: 0f);
        Move(ai, DsrP6DoubleDragonsScenario.HallowedWingsCast + 0.4f, () => AiMove.Create(Cluster(new Vector2(0f, 6f * SafeWingSide), Enum.GetValues<PartyRole>())).NaturalOrder(), jitter: 0f);
        Move(ai, DsrP6DoubleDragonsScenario.NidhoggCauterizeCast + 0.3f, () => AiMove.Create(HallowedWingsSpots()).NaturalOrder(), jitter: 0f, sprint: true);
        Move(ai, DsrP6DoubleDragonsScenario.HallowedWingsLands + 0.8f, () => AiMove.Create(VowHolderMeetsTank(state.VowTarget, MainTank, new Vector2(0f, 0f), new Vector2(0f, 9f))).NaturalOrder(), jitter: 0f);
        Move(ai, DsrP6DoubleDragonsScenario.VowPasses[0] + 0.4f, () => AiMove.Create(Cluster(Vector2.Zero, Enum.GetValues<PartyRole>())).NaturalOrder(), jitter: 0f);
        Move(ai, DsrP6DoubleDragonsScenario.HraesvelgrCauterizeCast + 0.25f, () => AiMove.Create(Cluster(AkhMornPath()[0], Enum.GetValues<PartyRole>())).NaturalOrder(), jitter: 0f, sprint: true);
        for (var i = 0; i < DsrP6DoubleDragonsScenario.AkhMornHits.Length; i++)
        {
            var next = i + 1;
            Move(ai, DsrP6DoubleDragonsScenario.AkhMornHits[i] + 0.05f, () => AiMove.Create(Cluster(AkhMornPath()[next], Enum.GetValues<PartyRole>())).NaturalOrder(), jitter: 0f, sprint: true);
        }
        Move(ai, DsrP6DoubleDragonsScenario.FlameBlastCasts[2] + DsrP6DoubleDragonsScenario.FlameBlastDelay + 0.07f, () => AiMove.Create(LineUpForWrothFlames()).NaturalOrder(), jitter: 0f, sprint: true);
        Move(ai, DsrP6DoubleDragonsScenario.FlamesResolve + 0.15f, () => AiMove.Create(VowHolderMeetsTank(MainTank, OffTank, Vector2.Zero, new Vector2(0f, -StartCorner().Z * 9f))).NaturalOrder(), jitter: 0f, sprint: true);
        Move(ai, DsrP6DoubleDragonsScenario.VowPasses[1] + 0.4f, () => AiMove.Create(Cluster(Vector2.Zero, Enum.GetValues<PartyRole>())).NaturalOrder(), jitter: 0f);

        Move(ai, DsrP6DoubleDragonsScenario.SecondHalfStart + 0.3f, () => AiMove.Create(Cluster(Vector2.Zero, Enum.GetValues<PartyRole>())).NaturalOrder(), jitter: 0f);
        Move(ai, DsrP6DoubleDragonsScenario.SecondAkhAfahCast + 0.4f, () => AiMove.Create(LightPartiesNorthAndSouth()).NaturalOrder(), jitter: 0f);
        Move(ai, DsrP6DoubleDragonsScenario.SecondAkhAfahLands + 0.6f, () => AiMove.Create(Cluster(Vector2.Zero, Enum.GetValues<PartyRole>())).NaturalOrder(), jitter: 0f);
        Move(ai, DsrP6DoubleDragonsScenario.SecondHotWingCast + 0.35f, () => AiMove.Create(SecondHallowedWingsSpots()).NaturalOrder(), jitter: 0f, sprint: true);
        Move(ai, DsrP6DoubleDragonsScenario.SecondHotWingLands + 0.6f, () => AiMove.Create(VowHolderMeetsTank(OffTank, ThirdVowHolder, Vector2.Zero, new Vector2(0f, 9f))).NaturalOrder(), jitter: 0f, sprint: true);
        Move(ai, DsrP6DoubleDragonsScenario.VowPasses[2] + 0.4f, () => AiMove.Create(Cluster(new Vector2(0f, -3f), Enum.GetValues<PartyRole>())).NaturalOrder(), jitter: 0f);
        Move(ai, DsrP6DoubleDragonsScenario.SecondTethersOut + 0.4f, () => AiMove.Create(StaticSecondWyrmsbreathSpots()).NaturalOrder(), jitter: 0f, sprint: true);
        Move(ai, DsrP6DoubleDragonsScenario.SecondBreathsLand + 0.5f, () => AiMove.Create(WaitNorthForTheDives()).NaturalOrder(), jitter: 0f);
        Move(ai, DsrP6DoubleDragonsScenario.CauterizeCast + 0.1f, () => AiMove.Create(IntoTheOppositeElementsLane()).NaturalOrder(), jitter: 0f, sprint: true);
        Move(ai, DsrP6DoubleDragonsScenario.CauterizeLands + 0.4f, () => AiMove.Create(Cluster(NorthWallSpot, Enum.GetValues<PartyRole>())).NaturalOrder(), jitter: 0f, sprint: true);
        Move(ai, DsrP6DoubleDragonsScenario.RevengeOfTheHorde - 0.6f, () => AiMove.Create(VowHolderMeetsTank(ThirdVowHolder, PhysRangedDps, new Vector2(8f, -19f), new Vector2(-5f, -19f))).NaturalOrder(), jitter: 0f);
        Move(ai, DsrP6DoubleDragonsScenario.VowPasses[3] + 0.4f, () => AiMove.Create(Cluster(new Vector2(0f, -19f), Enum.GetValues<PartyRole>())).NaturalOrder(), jitter: 0f);
    }

    private static void Move(AiManager ai, float time, Func<IAiMove> positions, float jitter = .3f, bool sprint = false) =>
        ai.Move(time, positions, jitter: jitter, sprint: sprint);

    private PartyRole ThirdVowHolder => state.VowTarget == MeleeDpsA ? MeleeDpsB : MeleeDpsA;

    private Vector2?[] SecondHallowedWingsSpots()
    {
        var sz = state.SecondWingsNorth ? 1f : -1f;
        var z = sz * (state.SecondHotWing ? HotWingStripZ : HotTailLineZ);
        var (wallTank, middleTank, party) = state.SecondPlumesNear ? (19.5f, 1.8f, -18f) : (-19.5f, 0.5f, 19f);
        var spots = Cluster(new Vector2(party, z), [RegenHealer, ShieldHealer, MeleeDpsA, MeleeDpsB, PhysRangedDps, CasterDps]);
        spots[(int)MainTank] = new Vector2(wallTank, z);
        spots[(int)OffTank] = new Vector2(middleTank, z);
        return spots;
    }

    private Vector2?[] StaticSecondWyrmsbreathSpots()
    {
        var spots = new Vector2?[8];
        var fire = state.SecondNidhoggTethers.Where(r => r != PhysRangedDps).ToList();
        var ice = state.HraesvelgrTethersAt(1).Where(r => r != PhysRangedDps).ToList();
        var larger = fire.Count == 3 ? fire : ice;
        var centre = new[] { RegenHealer, ShieldHealer, CasterDps }.First(larger.Contains);
        spots[(int)centre] = new Vector2(0f, -19.5f);
        larger.Remove(centre);
        foreach (var (roles, side) in new[] { (fire, 1f), (ice, -1f) })
        {
            var ordered = roles.OrderBy(r => r is MeleeDpsA or MeleeDpsB ? 0 : 1).ToArray();
            spots[(int)ordered[0]] = new Vector2(side * 14f, -12f);
            spots[(int)ordered[1]] = new Vector2(side * 8f, -17f);
        }
        var southX = state.SecondGlow switch { BreathGlow.Nidhogg => -5f, BreathGlow.Hraesvelgr => 5f, _ => 0f };
        spots[(int)PhysRangedDps] = new Vector2(southX, 19f);
        var sharedBuster = state.SecondGlow == BreathGlow.Both;
        spots[(int)MainTank] = sharedBuster ? new Vector2(-0.3f, 0f) : SecondWyrmsbreathMainTankSpot;
        spots[(int)OffTank] = sharedBuster ? new Vector2(0.3f, 0f) : SecondWyrmsbreathOffTankSpot;
        return spots;
    }

    private static Vector2?[] WaitNorthForTheDives()
    {
        var spots = Cluster(WaitForTheDivesSpot, [RegenHealer, ShieldHealer, MeleeDpsA, MeleeDpsB, PhysRangedDps, CasterDps]);
        spots[(int)MainTank] = new Vector2(-4f, -14f);
        spots[(int)OffTank] = new Vector2(4f, -14f);
        return spots;
    }

    private Vector2?[] IntoTheOppositeElementsLane()
    {
        var nidhoggSide = state.NidhoggCauterizesWest ? -1f : 1f;
        var frozen = state.HraesvelgrTethersAt(1).ToArray();
        var burning = state.NidhoggTethersAt(1).ToArray();
        var spots = Cluster(new Vector2(nidhoggSide * DiveLaneStandX, DiveLaneStandZ), frozen);
        var hraesvelgrLane = Cluster(new Vector2(-nidhoggSide * DiveLaneStandX, DiveLaneStandZ), burning);
        foreach (var role in burning) spots[(int)role] = hraesvelgrLane[(int)role];
        spots[(int)MainTank] = new Vector2(-DiveTankX, DiveTankZ);
        spots[(int)OffTank] = new Vector2(DiveTankX, DiveTankZ);
        return spots;
    }

    private static Vector2?[] PrePositionForTethers()
    {
        var spots = new Vector2?[8];
        spots[(int)MainTank] = new Vector2(-2f, -3f);
        spots[(int)OffTank] = new Vector2(2f, -3f);
        Place(spots, new Vector2(0f, 5f), PhysRangedDps, CasterDps);
        Place(spots, new Vector2(-5f, 14f), RegenHealer, MeleeDpsA);
        Place(spots, new Vector2(5f, 14f), ShieldHealer, MeleeDpsB);
        return spots;
    }

    private Vector2?[] PairUpWithOppositeTethers()
    {
        var anchors = new[] { CasterDps, RegenHealer, ShieldHealer };
        var flexes = new[] { PhysRangedDps, MeleeDpsA, MeleeDpsB };
        var pairSpots = new[] { FrontPairSpot, WestPairSpot, EastPairSpot };
        SwapFlexesSoEachPairHoldsBothElements(anchors, flexes);
        var spots = new Vector2?[8];
        for (var i = 0; i < anchors.Length; i++)
        {
            spots[(int)anchors[i]] = pairSpots[i] + new Vector2(StandAwayFromYourDragon(anchors[i]), 0f);
            spots[(int)flexes[i]] = pairSpots[i] + new Vector2(StandAwayFromYourDragon(flexes[i]), 0f);
        }
        var sharedBuster = state.Glow == BreathGlow.Both;
        spots[(int)MainTank] = sharedBuster ? SharedBusterSpot + new Vector2(-0.3f, 0f) : MainTankCornerSpot;
        spots[(int)OffTank] = sharedBuster ? SharedBusterSpot + new Vector2(0.3f, 0f) : OffTankCornerSpot;
        return spots;
    }

    private void SwapFlexesSoEachPairHoldsBothElements(PartyRole[] anchors, PartyRole[] flexes)
    {
        bool Fire(PartyRole role) => state.NidhoggTethers.Contains(role);
        for (var i = 0; i < anchors.Length; i++)
        {
            if (Fire(anchors[i]) != Fire(flexes[i])) continue;
            for (var j = i + 1; j < anchors.Length; j++)
            {
                if (Fire(anchors[j]) != Fire(flexes[j]) || Fire(flexes[j]) == Fire(flexes[i])) continue;
                (flexes[i], flexes[j]) = (flexes[j], flexes[i]);
                break;
            }
        }
    }

    private float StandAwayFromYourDragon(PartyRole role) => state.NidhoggTethers.Contains(role) ? PairOffset : -PairOffset;

    private static Vector2?[] EveryoneSpreadsForMortalVow()
    {
        var spots = new Vector2?[8];
        for (var i = 0; i < VowSpreadClockwiseFromNorthNorthEast.Length; i++)
        {
            var bearing = MathF.PI / 8f + i * MathF.PI / 4f;
            spots[(int)VowSpreadClockwiseFromNorthNorthEast[i]] = VowSpreadRadius * new Vector2(MathF.Sin(bearing), -MathF.Cos(bearing));
        }
        return spots;
    }

    private static Vector2?[] LightPartiesNorthAndSouth()
    {
        var spots = Cluster(new Vector2(0f, -5.5f), GroupOne);
        var south = Cluster(new Vector2(0f, 6f), GroupTwo);
        foreach (var role in GroupTwo) spots[(int)role] = south[(int)role];
        return spots;
    }

    private float SafeWingSide => state.WingsNorth ? 1f : -1f;

    private float SafeDiveSide => state.NidhoggDivesEast ? -1f : 1f;

    private Vector2?[] HallowedWingsSpots()
    {
        var sz = SafeWingSide;
        var east = SafeDiveSide > 0f;
        var (mainTank, offTank, party) = (state.PlumesNear, east) switch
        {
            (true, false) => (new Vector2(-2.5f, 2.5f), new Vector2(-2.5f, 19.5f), new Vector2(-17f, 11f)),
            (true, true) => (new Vector2(19.5f, 2.5f), new Vector2(19.5f, 15f), new Vector2(3f, 4.5f)),
            (false, false) => (new Vector2(-19.5f, 2.5f), new Vector2(-19.5f, 19f), new Vector2(-2.5f, 7f)),
            _ => (new Vector2(2.5f, 2.5f), new Vector2(2.5f, 19f), new Vector2(19f, 7f)),
        };
        var spots = Cluster(party with { Y = party.Y * sz }, [RegenHealer, ShieldHealer, MeleeDpsA, MeleeDpsB, PhysRangedDps, CasterDps]);
        spots[(int)MainTank] = offTank with { Y = offTank.Y * sz };
        spots[(int)OffTank] = mainTank with { Y = mainTank.Y * sz };
        return spots;
    }

    private static Vector2?[] VowHolderMeetsTank(PartyRole holder, PartyRole taker, Vector2 passSpot, Vector2 restSpot)
    {
        var rest = Enum.GetValues<PartyRole>().Where(r => r != holder && r != taker).ToArray();
        var spots = Cluster(restSpot, rest);
        spots[(int)holder] = passSpot + new Vector2(-0.3f, 0f);
        spots[(int)taker] = passSpot + new Vector2(0.3f, 0f);
        return spots;
    }

    private (int X, int Z) StartCorner() => (HraesvelgrDivesThroughNidhogg ? 1 : -1, -state.SecondSetCorner.Z);

    private bool HraesvelgrDivesThroughNidhogg => state.HraesvelgrLane < 0;

    private List<Vector2> AkhMornPath()
    {
        var (cx, cz) = StartCorner();
        Vector2 At(float x, float z) => new(cx * x, cz * z);
        return [At(21f, 18.5f), At(14f, 18.5f), At(10.5f, 18.5f), At(3f, 12.5f), Vector2.Zero];
    }

    private Vector2?[] LineUpForWrothFlames()
    {
        var z = state.HotWing ? 0f : -StartCorner().Z * HotTailLineZ;
        var spots = new Vector2?[8];
        foreach (var (role, sign) in markers.Assignments)
        {
            var x = sign switch
            {
                Sign.Attack1 => -20f,
                Sign.Attack2 => -13.5f,
                Sign.Attack3 => -7f,
                Sign.Attack4 => -0.5f,
                Sign.Bind1 or Sign.Bind2 => NearStackX,
                Sign.Ignore1 or Sign.Ignore2 => WallStackX,
                _ => throw new InvalidOperationException("Unexpected Wroth marker"),
            };
            spots[(int)role] = new Vector2(x, z);
        }
        return spots;
    }

    private static void Place(Vector2?[] spots, Vector2 at, params PartyRole[] roles)
    {
        foreach (var role in roles) spots[(int)role] = at;
    }

    private static Vector2?[] Cluster(Vector2 centre, IEnumerable<PartyRole> roles)
    {
        var spots = new Vector2?[8];
        var i = 0;
        foreach (var role in roles)
        {
            spots[(int)role] = centre + new Vector2(0.5f * (i % 3 - 1), 0.5f * (i / 3 - 1));
            i++;
        }
        return spots;
    }
}
