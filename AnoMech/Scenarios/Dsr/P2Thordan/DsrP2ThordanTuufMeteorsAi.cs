using System.Collections.Generic;
using AnoMech.Core.Game.Ai;
using AnoMech.Core.Game.Party;
using AnoMech.Core.SimObjects;

namespace AnoMech.Scenarios.Dsr.P2Thordan;

public sealed class DsrP2ThordanTuufMeteorsAi : DsrP2ThordanAi
{
    public override string Name => "TUUF (Meteors)";

    public override void Run(DsrP2ThordanState stateParam, SimWorld worldParam)
    {
        stateParam.TuufMeteorPairs = true;
        base.Run(stateParam, worldParam);
    }

    protected override IReadOnlyDictionary<PartyRole, float> MeteorCardinalsForAi() => state.TuufMeteorCardinals();
}
