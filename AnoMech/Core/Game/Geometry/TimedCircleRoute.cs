using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AnoMech.Core.Game.Geometry;

public readonly record struct TimedCircle(Vector2 Center, float Radius, float ActiveFrom, float ActiveUntil);

public static class TimedCircleRoute
{
    public const float Clearance = .35f;

    public static bool SegmentClear(Vector2 a, Vector2 b, float time, float speed, IReadOnlyList<TimedCircle> hazards)
    {
        var duration = Vector2.Distance(a, b) / speed;
        foreach (var h in hazards)
        {
            var begin = MathF.Max(0, h.ActiveFrom - time - .1f);
            var end = MathF.Min(duration + .1f, h.ActiveUntil - time);
            if (end < begin) continue;
            var from = duration > 0 ? Vector2.Lerp(a, b, Math.Clamp(begin / duration, 0, 1)) : a;
            var to = duration > 0 ? Vector2.Lerp(a, b, Math.Clamp(end / duration, 0, 1)) : b;
            var delta = to - from;
            var t = delta.LengthSquared() > 0 ? Math.Clamp(Vector2.Dot(h.Center - from, delta) / delta.LengthSquared(), 0, 1) : 0;
            if (Vector2.Distance(h.Center, from + t * delta) <= h.Radius + Clearance) return false;
        }
        return true;
    }

    public static IReadOnlyList<Vector2>? Plan(Vector2 start, Vector2 goal, float time, float speed,
        IReadOnlyList<TimedCircle> hazards, bool clockwise, float minRadius = 12f, float maxRadius = 20f)
    {
        var originAngle = MathF.Atan2(start.X, -start.Y);
        float Progress(Vector2 p) => MathF.Atan2(MathF.Sin(MathF.Atan2(p.X, -p.Y) - originAngle), MathF.Cos(MathF.Atan2(p.X, -p.Y) - originAngle));
        var finish = Progress(goal);
        bool InBounds(Vector2 p) => p.Length() >= minRadius && p.Length() <= maxRadius
            && (!clockwise || Progress(p) >= -.001f && Progress(p) <= MathF.Max(0, finish) + .001f);
        bool EdgeAllowed(Vector2 a, Vector2 b) => InBounds(b) && (!clockwise || Progress(b) >= Progress(a) - .001f)
            && SegmentRadius(a, b) >= minRadius - .01f;
        bool CanRemain(Vector2 p, float arrival) => !hazards.Any(h => h.ActiveUntil > arrival && Vector2.Distance(p, h.Center) <= h.Radius + Clearance);
        if (EdgeAllowed(start, goal) && SegmentClear(start, goal, time, speed, hazards)
            && CanRemain(goal, time + Vector2.Distance(start, goal) / speed)) return [goal];
        var nodes = new List<Vector2> { start, goal };
        foreach (var h in hazards.Where(h => h.ActiveUntil > time))
        for (var i = 0; i < 24; i++)
        {
            var angle = i * MathF.Tau / 24;
            var point = h.Center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (h.Radius + Clearance + .15f);
            if (InBounds(point)) nodes.Add(point);
        }
        var distances = Enumerable.Repeat(float.PositiveInfinity, nodes.Count).ToArray();
        var previous = Enumerable.Repeat(-1, nodes.Count).ToArray();
        var queue = new PriorityQueue<int, float>();
        distances[0] = 0;
        queue.Enqueue(0, 0);
        while (queue.TryDequeue(out var i, out var cost))
        {
            if (cost > distances[i]) continue;
            if (i == 1)
            {
                if (!CanRemain(goal, time + cost / speed)) return null;
                var result = new List<Vector2>();
                for (var j = 1; j != 0; j = previous[j]) result.Add(nodes[j]);
                result.Reverse();
                return result;
            }
            for (var j = 1; j < nodes.Count; j++)
            {
                var distance = Vector2.Distance(nodes[i], nodes[j]);
                var next = cost + distance;
                if (next >= distances[j] || !EdgeAllowed(nodes[i], nodes[j])
                    || !SegmentClear(nodes[i], nodes[j], time + cost / speed, speed, hazards)) continue;
                distances[j] = next;
                previous[j] = i;
                queue.Enqueue(j, next);
            }
        }
        return null;
    }

    private static float SegmentRadius(Vector2 a, Vector2 b)
    {
        var delta = b - a;
        var t = delta.LengthSquared() > 0 ? Math.Clamp(-Vector2.Dot(a, delta) / delta.LengthSquared(), 0, 1) : 0;
        return (a + t * delta).Length();
    }
}
