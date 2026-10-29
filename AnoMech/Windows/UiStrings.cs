using System;
using System.Collections.Generic;
using AnoMech.Core.Game.Ai;
using AnoMech.Scenarios;
using AnoMech.Scenarios.Dsr;
using AnoMech.Scenarios.Dsr.P1Knights;
using AnoMech.Scenarios.Dsr.P2Thordan;
using AnoMech.Scenarios.Dsr.P3Nidhogg;
using AnoMech.Scenarios.Dsr.P4Eyes;
using AnoMech.Scenarios.Dsr.P5Death;
using AnoMech.Scenarios.Dsr.P5Wrath;
using AnoMech.Scenarios.Dsr.P6DoubleDragons;

namespace AnoMech.Windows;

internal static class UiStrings
{
    internal const string Scenarios = "機制";
    internal const string SelectScenario = "請選擇機制";
    internal const string Setup = "設定";
    internal const string ScenarioSettings = "機制選項";
    internal const string Role = "職責：";
    internal const string Strategy = "打法：";
    internal const string Region = "區域：";
    internal const string Waymarks = "場標：";
    internal const string NoStratsInRegion = "（此區域尚無打法）";
    internal const string GodMode = "無敵模式";
    internal const string AutoRestart = "自動重開";
    internal const string Streak = "連續成功：";
    internal const string Solo = "單人";
    internal const string Start = "開始";
    internal const string Restart = "重開";
    internal const string Stop = "停止";
    internal const string Leave = "離開";
    internal const string ReturnToInn = "回旅館";
    internal const string WaitingToStart = "等待開始…";
    internal const string InnOnlyHint = "僅能在旅館開始機制";
    internal const string InnOnlyHelp = "請先回到旅館再開始練習。";
    internal const string MpDisabled = "連線模式下無法變更。";
    internal const string MpHostStrat = "連線時以房主選擇的打法為準。";
    internal const string CategoryUltimate = "絕本";
    internal const string RunFailedTitle = " — 失敗";
    internal const string ConfigTitle = "AnoMech 設定###AnoMechConfig";
    internal const string OpenMenuOnInn = "進入旅館時開啟選單";
    internal const string SuppressBgm = "關閉機制 BGM";
    internal const string ResultMarks = "顯示成功／失敗標記";
    internal const string ShowSpeed = "顯示速度控制";
    internal const string ResolveActions = "解析自身技能";
    internal const string Speed = "速度：";

    internal static readonly string[] RoleLabels =
        ["自動", "MT", "OT", "H1", "H2", "M1", "M2", "R1", "R2"];

    internal static string Category(string category) =>
        category switch { "Ultimate" => CategoryUltimate, _ => category };

    internal static string ZoneName(IZone zone) =>
        zone is DsrZone ? "絕龍詩" : zone.Name;

    internal static string ScenarioMenuLabel(IScenario scenario)
    {
        if (scenario is DsrP6DoubleDragonsScenario)
        {
            return scenario.Name.StartsWith("Hallowed", StringComparison.Ordinal)
                ? "P6 聖翼②～燒灼"
                : "P6 龍息～邪念";
        }
        return ScenarioLabels.TryGetValue(scenario.GetType(), out var label) ? label : scenario.DisplayName();
    }

    internal static string WaymarkPreset(string name) =>
        WaymarkLabels.TryGetValue(name, out var label) ? label : name;

    internal static string StratName(string name) =>
        StratLabels.TryGetValue(name, out var label) ? label : name;

    internal static int PreferTuufStratIndex(IScenario scenario)
    {
        var strats = scenario.AiStrats;
        for (var i = 0; i < strats.Count; i++)
            if (IsTuuf(strats[i].Name)) return i;
        return 0;
    }

    internal static int PreferTuufStratIndex(IReadOnlyList<int> indices, IReadOnlyList<IScenarioAi> strats)
    {
        foreach (var i in indices)
            if (IsTuuf(strats[i].Name)) return i;
        return indices.Count > 0 ? indices[0] : -1;
    }

    internal static int PreferTuufWaymarkIndex(IZone zone)
    {
        var presets = zone.WaymarkPresets;
        for (var i = 0; i < presets.Count; i++)
            if (presets[i].Name.Contains("TUUF", StringComparison.OrdinalIgnoreCase)) return i;
        return 0;
    }

    private static bool IsTuuf(string name) =>
        name.Contains("TUUF", StringComparison.OrdinalIgnoreCase);

    private static readonly Dictionary<Type, string> ScenarioLabels = new()
    {
        [typeof(DsrP1KnightsScenario)] = "P1 騎士",
        [typeof(DsrP2ThordanScenario)] = "P2 托爾丹",
        [typeof(DsrP2MeteorsScenario)] = "P2 隕石",
        [typeof(DsrP3NidhoggScenario)] = "P3 尼德霍格",
        [typeof(DsrP4EyesScenario)] = "P4 眼睛",
        [typeof(DsrP5WrathScenario)] = "P5 天空之怒",
        [typeof(DsrP5DeathScenario)] = "P5 死宣",
        [typeof(DsrP6CauterizeScenario)] = "P6 燒灼（單段）",
    };

    private static readonly Dictionary<string, string> WaymarkLabels = new(StringComparer.Ordinal)
    {
        ["DSR Phase 1"] = "P1 場標",
        ["NAUR"] = "NAUR",
        ["TUUF (P2+)"] = "TUUF（P2 起）",
    };

    private static readonly Dictionary<string, string> StratLabels = new(StringComparer.Ordinal)
    {
        ["Toolbox (PF)"] = "Toolbox（PF）",
        ["NAUR (PF)"] = "NAUR（PF）",
        ["TUUF (Meteors)"] = "TUUF（隕石）",
        ["TUUF (Easthogg)"] = "TUUF（東尼德霍格）",
        ["NA PF (snake swaps)"] = "NA PF（蛇換）",
        ["TUUF (T/H red)"] = "TUUF（T/H 紅）",
        ["NA PF"] = "NA PF",
        ["TUUF (clockwise twisters)"] = "TUUF（順時針旋風）",
        ["NAUR (conga, dooms anchor)"] = "NAUR（conga）",
        ["TUUF (Dooms N, anchored)"] = "TUUF（死宣北定）",
        ["NA PF (Light vs Dark)"] = "NA PF（光暗）",
        ["TUUF (5-1 Wroth)"] = "TUUF（5-1 邪念）",
    };

    internal static string RunStatusLabel(string english) => english switch
    {
        "Idle" => "待命",
        "Running" => "進行中",
        "Failed" => "失敗",
        "Cleared" => "過本",
        "Cleared with mistakes" => "過本（有失誤）",
        "Stopped" => "已停止",
        "Running with mistakes" => "進行中（有失誤）",
        _ => english,
    };
}
