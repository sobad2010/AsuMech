using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AnoMech.Core.SimObjects;
using AnoMech.Core.Game.Party;
using AnoMech.Scenarios;
using AnoMech.Scenarios.Dsr.P4Eyes;
using static AnoMech.Core.Game.Party.PartyRole;
using static AnoMech.Scenarios.Dsr.DsrConstants;
using static AnoMech.Tests.NegativeRun;

namespace AnoMech.Tests;

// Times are the scenario's absolute timeline; the player follows the strat until taken over.
public class DsrP4EyesScenarioTests
{
    // Every role starts on the colour the strat wants: melee and tanks red, ranged and healers blue.
    private static void RightColours(DsrP4EyesStateOverrides o)
    {
        o.SoulMarked = MainTank;
        o.FirstRed = [MainTank, OffTank, MeleeDpsA, MeleeDpsB];
        o.DivePicks = [(0, 1), (0, 1), (0, 1), (0, 1)];
    }

    [Test]
    public void MissingAlphinaudsBuffsIsLethal()
        => Negative<DsrP4EyesScenario>(PhysRangedDps)
            .Overrides<DsrP4EyesStateOverrides>(RightColours)
            .TeleportAt(15f, to: new Vector2(0f, -12f))
            .ShouldKill(ActionId.SoulOfFriendship, PhysRangedDps);

    [Test]
    public void TouchingAnOrbWithoutTheRedTetherIsLethal()
        => Negative<DsrP4EyesScenario>(RegenHealer)
            .Overrides<DsrP4EyesStateOverrides>(RightColours)
            .TeleportAt(38f, to: new Vector2(10f, -5.6f))
            .ShouldKill(ActionId.FlareStar, RegenHealer);

    [Test]
    public void WrongColoursWaitBeforeTradingOnEstinien()
        => Negative<DsrP4EyesScenario>(RegenHealer)
            .Overrides<DsrP4EyesStateOverrides>(o => { RightColours(o); o.FirstRed = [MainTank, RegenHealer, MeleeDpsA, MeleeDpsB]; })
            .At(35.4f, p => Assert.That(Vector2.Distance(new Vector2(p.Member(OffTank)!.Position.X, p.Member(OffTank)!.Position.Z), new Vector2(6.5f, -1.7f)), Is.LessThan(0.5f)))
            .At(38.5f, p => Assert.That(p.Member(OffTank)!.HasStatus(StatusId.Clawbound), Is.True))
            .FreezeAt(41f)
            .ShouldKill(ActionId.FlareStarUnpopped, AllBut());

    [Test]
    public void LeavingABlueOrbToBurstWipes()
        => Negative<DsrP4EyesScenario>(RegenHealer)
            .Overrides<DsrP4EyesStateOverrides>(RightColours)
            .FreezeAt(41f)
            .ShouldKill(ActionId.FlareStarUnpopped, AllBut());

    [Test]
    public void PoppingAnOrbBeforeItHasGrownKillsThePopper()
        => Negative<DsrP4EyesScenario>(MeleeDpsA)
            .Overrides<DsrP4EyesStateOverrides>(RightColours)
            .TeleportAt(DsrP4EyesScenario.OrbSpawnTime + 3.5f, to: new Vector2(17f, 0f))
            .ShouldKill(ActionId.FlareNova, MeleeDpsA);

    [Test]
    public void OrbsGrowWithEachStep()
        => Negative<DsrP4EyesScenario>(MeleeDpsA)
            .Overrides<DsrP4EyesStateOverrides>(RightColours)
            .At(DsrP4EyesScenario.OrbSpawnTime + 5.0f, p =>
            {
                Assert.That(OrbScales(p, BNpcBaseId.GildedPrice), Is.All.EqualTo(1.5f));
                Assert.That(OrbScales(p, BNpcBaseId.AzurePrice), Is.All.EqualTo(0.5f));
            })
            .At(DsrP4EyesScenario.OrbSpawnTime + 12.5f, p => Assert.That(OrbScales(p, BNpcBaseId.AzurePrice), Is.All.EqualTo(1.525f)))
            .TeleportAt(DsrP4EyesScenario.OrbSpawnTime + 12.6f, to: new Vector2(10f, 7f))
            .ShouldKill(ActionId.FlareStar, MeleeDpsA);

    private static float[] OrbScales(ScenarioProbe p, uint baseId) =>
        p.World.Children.OfType<SimEnemy>().Where(e => e.IsActive && e.SpawnConfig.BNpcBaseId == baseId).Select(e => e.Proxy!.ModelScale).ToArray();

    private static readonly int Tuuf = new DsrP4EyesScenario().AiStrats
        .Select((ai, index) => (ai, index))
        .Single(s => s.ai is DsrP4EyesTuufAi)
        .index;

    private static void TuufRightColours(DsrP4EyesStateOverrides o)
    {
        o.SoulMarked = MainTank;
        o.FirstRed = [MainTank, OffTank, RegenHealer, ShieldHealer];
        o.DivePicks = [(0, 1), (0, 1), (0, 1), (0, 1)];
    }

    [Test]
    public void TuufGivesSupportsRedThenDpsRedThenHealersTakeTheFirstDives()
    {
        var clawAtOrbs = new Dictionary<PartyRole, bool>();
        var clawAfterPass = new Dictionary<PartyRole, bool>();
        var clawAfterFirstDive = new Dictionary<PartyRole, bool>();
        var run = ScenarioRun.Execute(typeof(DsrP4EyesScenario), Tuuf, seed: 1, new ScenarioRunOptions
        {
            PlayerRole = MainTank,
            Overrides = o => TuufRightColours((DsrP4EyesStateOverrides)o),
            Probe = p =>
            {
                if (p.Crossed(40.8f)) CaptureClawbound(p, clawAtOrbs);
                if (p.Crossed(46.8f)) CaptureClawbound(p, clawAfterPass);
                if (p.Crossed(64.5f)) CaptureClawbound(p, clawAfterFirstDive);
            },
        });
        Assert.That(run.Passed, Is.True, run.ToString);
        Assert.That(clawAtOrbs[MainTank] && clawAtOrbs[OffTank] && clawAtOrbs[RegenHealer] && clawAtOrbs[ShieldHealer], Is.True);
        Assert.That(clawAtOrbs[MeleeDpsA] || clawAtOrbs[MeleeDpsB] || clawAtOrbs[PhysRangedDps] || clawAtOrbs[CasterDps], Is.False);
        Assert.That(clawAfterPass[MeleeDpsA] && clawAfterPass[MeleeDpsB] && clawAfterPass[PhysRangedDps] && clawAfterPass[CasterDps], Is.True);
        Assert.That(clawAfterPass[MainTank] || clawAfterPass[OffTank] || clawAfterPass[RegenHealer] || clawAfterPass[ShieldHealer], Is.False);
        Assert.That(clawAfterFirstDive[RegenHealer] && clawAfterFirstDive[ShieldHealer], Is.True);
        Assert.That(clawAfterFirstDive[MeleeDpsA] || clawAfterFirstDive[MeleeDpsB], Is.False);
    }

    private static void CaptureClawbound(ScenarioProbe probe, Dictionary<PartyRole, bool> into)
    {
        foreach (var role in PerRole.All)
            if (probe.Member(role) is { } member)
                into[role] = member.HasStatus(StatusId.Clawbound);
    }

    // The healers are dived first; the off tank never comes to take the regen healer's red.
    [Test]
    public void KeepingRedAfterADiveIsLethalOnTheNextOne()
        => Negative<DsrP4EyesScenario>(OffTank)
            .Overrides<DsrP4EyesStateOverrides>(o =>
            {
                RightColours(o);
                o.DivePicks = [(0, 1), (1, 0), (0, 1), (0, 1)];
            })
            .FreezeAt(59f)
            .ShouldKill(ActionId.MirageDive, RegenHealer);
}
