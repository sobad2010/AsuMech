using System;
using System.Collections.Generic;
using System.Linq;
using AnoMech.Core.Game.Party;
using AnoMech.Core.Native.Interfaces;

namespace AnoMech.Scenarios.Dsr.P6DoubleDragons;

public sealed class DsrWrothMarkers
{
    public IReadOnlyDictionary<PartyRole, Sign> Assignments { get; }

    public DsrWrothMarkers(IReadOnlyCollection<PartyRole> spreads, IReadOnlyCollection<PartyRole> stacks)
    {
        var priority = Enum.GetValues<PartyRole>();
        if (spreads.Count != 4 || stacks.Count != 2 || spreads.Distinct().Count() != 4 ||
            stacks.Distinct().Count() != 2 || spreads.Intersect(stacks).Any() ||
            spreads.Concat(stacks).Any(r => !priority.Contains(r)))
            throw new ArgumentException("Wroth requires four distinct spreads and two distinct stacks.");
        var assignments = new Dictionary<PartyRole, Sign>();
        var purple = priority.Where(spreads.Contains).ToArray();
        var white = priority.Where(stacks.Contains).ToArray();
        var partners = priority.Where(r => !spreads.Contains(r) && !stacks.Contains(r)).ToArray();
        Sign[] attacks = [Sign.Attack4, Sign.Attack3, Sign.Attack2, Sign.Attack1];
        for (var i = 0; i < purple.Length; i++) assignments.Add(purple[i], attacks[i]);
        assignments.Add(white[0], Sign.Bind1);
        assignments.Add(partners[0], Sign.Bind2);
        assignments.Add(white[1], Sign.Ignore1);
        assignments.Add(partners[1], Sign.Ignore2);
        Assignments = new System.Collections.ObjectModel.ReadOnlyDictionary<PartyRole, Sign>(assignments);
    }
}
