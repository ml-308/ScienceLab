using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ScienceLab.Chemistry;

namespace ScienceLab.Controls;

/// <summary>实验容器（烧杯/试管）。</summary>
public sealed class Container
{
    public required Point Pos { get; set; }
    public required double Width { get; set; }
    public required double Height { get; set; }
    public string Name { get; set; } = "烧杯";
    public List<Reagent> Reagents { get; } = new();
    public Color LiquidColor { get; set; } = Colors.Transparent;
    public bool HasPrecipitate { get; set; }
    public bool HasGas { get; set; }
    public bool IsSelected { get; set; }

    public Rect Bounds => new(Pos.X, Pos.Y, Width, Height);
}

/// <summary>2D 自由化学实验室画布。</summary>
public class ChemistryCanvas : Canvas
{
    public List<Container> Containers { get; } = new();

    private Container? _selected;

    public event Action? Log;

    private readonly List<string> _log = new();
    public IReadOnlyList<string> LogEntries => _log;

    private Color SurfaceColor => GetColor("SurfaceBrush");
    private Color BorderColor => GetColor("BorderBrush");
    private Color TextColor => GetColor("TextSecondaryBrush");
    private Color GlassColor => Color.FromArgb(90, 180, 200, 220);

    public ChemistryCanvas()
    {
        Background = new SolidColorBrush(GetColor("BgBrush"));
        ClipToBounds = true;
        MouseLeftButtonDown += OnClick;
        SizeChanged += (_, _) => InvalidateVisual();

        // 默认放两个烧杯
        AddContainer("烧杯 A", new Point(120, 300), 160, 140);
        AddContainer("烧杯 B", new Point(360, 300), 160, 140);
        InvalidateVisual();
    }

    public void AddContainer(string name, Point pos, double w, double h)
    {
        var c = new Container { Name = name, Pos = pos, Width = w, Height = h };
        Containers.Add(c);
        InvalidateVisual();
    }

    public void Select(Container? c)
    {
        foreach (var x in Containers) x.IsSelected = false;
        _selected = c;
        if (c != null) c.IsSelected = true;
        InvalidateVisual();
    }

    /// <summary>向选中容器滴入试剂。</summary>
    public void AddReagentToSelected(Reagent reagent)
    {
        if (_selected == null)
        {
            AppendLog($"⚠️ 请先点击选中一个容器，再添加试剂");
            return;
        }

        _selected.Reagents.Add(reagent);
        AppendLog($"向 {_selected.Name} 加入 {reagent.Name}（{reagent.Formula}）");

        // 计算反应
        var result = ReactionEngine.Evaluate(_selected.Reagents);
        if (result.Kind != ReactionKind.None)
            AppendLog($"→ {result.Message}");

        _selected.HasPrecipitate = result.Kind == ReactionKind.Precipitate;
        _selected.HasGas = result.Kind == ReactionKind.Gas;

        if (result.NewColor.HasValue)
            _selected.LiquidColor = result.NewColor.Value;
        else if (result.Kind == ReactionKind.Neutralize)
            _selected.LiquidColor = Colors.Transparent;
        else
        {
            // 根据试剂本身颜色
            var colored = _selected.Reagents.LastOrDefault(r => r.Color.A > 0 && r.Color != Colors.Transparent);
            _selected.LiquidColor = colored?.Color ?? Colors.Transparent;
        }

        InvalidateVisual();
    }

    public void ClearSelected()
    {
        if (_selected == null) return;
        AppendLog($"清空 {_selected.Name}");
        _selected.Reagents.Clear();
        _selected.LiquidColor = Colors.Transparent;
        _selected.HasPrecipitate = false;
        _selected.HasGas = false;
        InvalidateVisual();
    }

    public void ClearAll()
    {
        foreach (var c in Containers)
        {
            c.Reagents.Clear();
            c.LiquidColor = Colors.Transparent;
            c.HasPrecipitate = false;
            c.HasGas = false;
        }
        AppendLog("已清空全部容器");
        InvalidateVisual();
    }

    private void AppendLog(string msg)
    {
        _log.Add($"[{DateTime.Now:HH:mm:ss}] {msg}");
        if (_log.Count > 100) _log.RemoveAt(0);
        Log?.Invoke();
    }

    private void OnClick(object sender, MouseButtonEventArgs e)
    {
        var p = e.GetPosition(this);
        // 从上层到下层查找容器
        for (int i = Containers.Count - 1; i >= 0; i--)
        {
            var c = Containers[i];
            if (c.Bounds.Contains(p))
            {
                Select(c);
                return;
            }
        }
        Select(null);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        double w = ActualWidth, h = ActualHeight;
        if (w <= 0 || h <= 0) return;

        dc.DrawRectangle(new SolidColorBrush(GetColor("BgBrush")), null, new Rect(0, 0, w, h));

        // 实验台面
        var tableBrush = new SolidColorBrush(SurfaceColor);
        var tablePen = new Pen(new SolidColorBrush(BorderColor), 1.5);
        dc.DrawRoundedRectangle(tableBrush, tablePen, new Rect(40, 460, w - 80, h - 480), 10, 10);

        foreach (var c in Containers)
            DrawContainer(dc, c);

        // 空态提示
        if (Containers.Count == 0)
        {
            var typeface = new Typeface("Segoe UI");
            var hint = new FormattedText("点击右侧「添加容器」开始实验", CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, typeface, 16, new SolidColorBrush(TextColor), 1.0);
            dc.DrawText(hint, new Point(w / 2 - hint.Width / 2, h / 2));
        }
    }

    private void DrawContainer(DrawingContext dc, Container c)
    {
        var glassBrush = new SolidColorBrush(GlassColor);
        var outlinePen = new Pen(new SolidColorBrush(c.IsSelected ? GetColor("PrimaryBrush") : GetColor("TextBrush")), c.IsSelected ? 3 : 1.5);

        // 烧杯主体（圆角矩形，上开口）
        var body = new Rect(c.Pos.X, c.Pos.Y, c.Width, c.Height);
        dc.DrawRoundedRectangle(glassBrush, outlinePen, body, 6, 6);

        // 液体
        if (c.LiquidColor.A > 0 && c.LiquidColor != Colors.Transparent)
        {
            var liquid = new Rect(c.Pos.X + 4, c.Pos.Y + c.Height * 0.4, c.Width - 8, c.Height * 0.55);
            dc.DrawRectangle(new SolidColorBrush(c.LiquidColor), null, liquid);
        }

        // 沉淀（底部）
        if (c.HasPrecipitate)
        {
            var precip = new Rect(c.Pos.X + 6, c.Pos.Y + c.Height * 0.8, c.Width - 12, c.Height * 0.15);
            dc.DrawRoundedRectangle(new SolidColorBrush(c.LiquidColor), null, precip, 3, 3);
        }

        // 气泡
        if (c.HasGas)
        {
            var rnd = new Random(42);
            var bubbleBrush = new SolidColorBrush(Color.FromArgb(160, 220, 230, 240));
            for (int i = 0; i < 8; i++)
            {
                double bx = c.Pos.X + 10 + rnd.NextDouble() * (c.Width - 20);
                double by = c.Pos.Y + c.Height * 0.5 + rnd.NextDouble() * c.Height * 0.3;
                double br = 2 + rnd.NextDouble() * 4;
                dc.DrawEllipse(bubbleBrush, null, new Point(bx, by), br, br);
            }
        }

        // 标签
        var typeface = new Typeface("Segoe UI");
        var nameText = new FormattedText(c.Name, CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight, typeface, 13, new SolidColorBrush(TextColor), 1.0);
        dc.DrawText(nameText, new Point(c.Pos.X, c.Pos.Y + c.Height + 6));

        // 试剂成分
        if (c.Reagents.Count > 0)
        {
            var comp = string.Join(" + ", c.Reagents.Select(r => r.Name));
            var compText = new FormattedText(comp, CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, typeface, 11, new SolidColorBrush(TextColor), 1.0);
            dc.DrawText(compText, new Point(c.Pos.X, c.Pos.Y + c.Height + 24));
        }
    }

    private static Color GetColor(string key)
    {
        var b = Application.Current.Resources[key] as SolidColorBrush;
        return b?.Color ?? Colors.Gray;
    }
}
