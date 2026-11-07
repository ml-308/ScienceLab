using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace ScienceLab.Chemistry;

/// <summary>化学试剂。</summary>
public sealed class Reagent
{
    public required string Name { get; init; }
    public required string Formula { get; init; }
    public required Color Color { get; init; }
    public required string Type { get; init; } // acid / base / salt / indicator / neutral
    public string? Description { get; init; }
}

/// <summary>试剂库。</summary>
public static class ReagentLibrary
{
    public static readonly List<Reagent> All = new()
    {
        new Reagent { Name = "水", Formula = "H₂O", Color = Color.FromRgb(0x3B, 0x82, 0xF6), Type = "neutral", Description = "无色透明溶剂" },
        new Reagent { Name = "稀盐酸", Formula = "HCl", Color = Colors.Transparent, Type = "acid", Description = "强酸，与碱中和" },
        new Reagent { Name = "稀硫酸", Formula = "H₂SO₄", Color = Colors.Transparent, Type = "acid", Description = "强酸" },
        new Reagent { Name = "氢氧化钠溶液", Formula = "NaOH", Color = Colors.Transparent, Type = "base", Description = "强碱" },
        new Reagent { Name = "酚酞试液", Formula = "C₂₀H₁₄O₄", Color = Color.FromRgb(0xE8, 0xE8, 0xF0), Type = "indicator", Description = "遇碱变红" },
        new Reagent { Name = "石蕊试液", Formula = "—", Color = Color.FromRgb(0x7B, 0x68, 0xEE), Type = "indicator", Description = "遇酸变红，遇碱变蓝" },
        new Reagent { Name = "硫酸铜溶液", Formula = "CuSO₄", Color = Color.FromRgb(0x3B, 0xA8, 0xE0), Type = "salt", Description = "蓝色溶液" },
        new Reagent { Name = "碳酸钠溶液", Formula = "Na₂CO₃", Color = Colors.Transparent, Type = "salt", Description = "与酸反应产生 CO₂ 气泡" },
        new Reagent { Name = "氯化铁溶液", Formula = "FeCl₃", Color = Color.FromRgb(0xD9, 0x9A, 0x3B), Type = "salt", Description = "黄色溶液" },
        new Reagent { Name = "硝酸银溶液", Formula = "AgNO₃", Color = Colors.Transparent, Type = "salt", Description = "与盐酸生成白色沉淀" },
    };

    public static Reagent? Find(string name) => All.Find(r => r.Name == name);
}

/// <summary>反应结果类型。</summary>
public enum ReactionKind { None, ColorChange, Precipitate, Gas, Neutralize }

/// <summary>一次反应的结果描述。</summary>
public sealed class ReactionResult
{
    public ReactionKind Kind { get; init; }
    public string Message { get; init; } = "";
    public Color? NewColor { get; init; }
}

/// <summary>反应规则引擎：根据容器内试剂组合判定反应。</summary>
public static class ReactionEngine
{
    public static ReactionResult Evaluate(IReadOnlyList<Reagent> reagents)
    {
        bool has(string name) => reagents.Any(x => x.Name == name);

        // 酸碱中和（酚酞）
        if (has("稀盐酸") && has("氢氧化钠溶液") && has("酚酞试液"))
            return new ReactionResult { Kind = ReactionKind.Neutralize, Message = "酸碱中和：酚酞由红变无色，生成 NaCl 和 H₂O", NewColor = Colors.Transparent };
        if (has("稀硫酸") && has("氢氧化钠溶液") && has("酚酞试液"))
            return new ReactionResult { Kind = ReactionKind.Neutralize, Message = "酸碱中和：酚酞由红变无色", NewColor = Colors.Transparent };

        // 酚酞遇碱
        if (has("氢氧化钠溶液") && has("酚酞试液"))
            return new ReactionResult { Kind = ReactionKind.ColorChange, Message = "酚酞遇碱变红", NewColor = Color.FromRgb(0xF0, 0x4F, 0x7A) };
        // 石蕊遇酸/碱
        if (has("稀盐酸") && has("石蕊试液"))
            return new ReactionResult { Kind = ReactionKind.ColorChange, Message = "石蕊遇酸变红", NewColor = Color.FromRgb(0xEF, 0x44, 0x44) };
        if (has("氢氧化钠溶液") && has("石蕊试液"))
            return new ReactionResult { Kind = ReactionKind.ColorChange, Message = "石蕊遇碱变蓝", NewColor = Color.FromRgb(0x3B, 0x5B, 0xDB) };

        // CuSO4 + NaOH 蓝色沉淀
        if (has("硫酸铜溶液") && has("氢氧化钠溶液"))
            return new ReactionResult { Kind = ReactionKind.Precipitate, Message = "生成蓝色沉淀 Cu(OH)₂", NewColor = Color.FromRgb(0x2E, 0x6E, 0xD8) };

        // 酸 + Na2CO3 气泡
        if ((has("稀盐酸") || has("稀硫酸")) && has("碳酸钠溶液"))
            return new ReactionResult { Kind = ReactionKind.Gas, Message = "产生 CO₂ 气泡", NewColor = null };

        // AgNO3 + HCl 白色沉淀
        if (has("硝酸银溶液") && has("稀盐酸"))
            return new ReactionResult { Kind = ReactionKind.Precipitate, Message = "生成白色沉淀 AgCl", NewColor = Color.FromRgb(0xF0, 0xF0, 0xF0) };

        // 单一颜色试剂
        if (has("硫酸铜溶液")) return new ReactionResult { Kind = ReactionKind.None, Message = "", NewColor = Color.FromRgb(0x3B, 0xA8, 0xE0) };
        if (has("氯化铁溶液")) return new ReactionResult { Kind = ReactionKind.None, Message = "", NewColor = Color.FromRgb(0xD9, 0x9A, 0x3B) };

        return new ReactionResult { Kind = ReactionKind.None, Message = "", NewColor = null };
    }
}
