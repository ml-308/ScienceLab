using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ScienceLab.Controls;

/// <summary>
/// 可缩放、平移的平面直角坐标系画布。
/// 支持绘制任意自定义函数曲线、网格、坐标轴与刻度。
/// </summary>
public class CoordinateCanvas : Canvas
{
    // 视口变换：世界坐标 -> 屏幕坐标
    // screenX = (worldX - CenterX) * Scale + Width/2
    // screenY = Height/2 - (worldY - CenterY) * Scale
    public double CenterX { get; set; } = 0;
    public double CenterY { get; set; } = 0;
    public double Scale { get; set; } = 60; // 每世界单位像素数

    private Point _lastDrag;
    private bool _dragging;

    /// <summary>要绘制的函数（每个元素含表达式与颜色）。</summary>
    public List<PlotFunction> Functions { get; } = new();

    public Color GridColor => GetResourceColor("BorderBrush");
    public Color AxisColor => GetResourceColor("TextSecondaryBrush");
    public Color TextColor => GetResourceColor("TextSecondaryBrush");
    public Color BackgroundColor => GetResourceColor("BgBrush");

    public CoordinateCanvas()
    {
        Background = new SolidColorBrush(BackgroundColor);
        Focusable = true;
        ClipToBounds = true;
        MouseWheel += OnWheel;
        MouseDown += OnDown;
        MouseMove += OnMove;
        MouseUp += OnUp;
        SizeChanged += (_, _) => InvalidateVisual();
    }

    public void ResetView()
    {
        CenterX = CenterY = 0;
        Scale = 60;
        InvalidateVisual();
    }

    private void OnWheel(object sender, MouseWheelEventArgs e)
    {
        var p = e.GetPosition(this);
        double worldX = (p.X - ActualWidth / 2) / Scale + CenterX;
        double worldY = (ActualHeight / 2 - p.Y) / Scale + CenterY;
        double factor = e.Delta > 0 ? 1.15 : 1 / 1.15;
        Scale = Math.Clamp(Scale * factor, 0.5, 5000);
        // 以鼠标位置为缩放中心
        CenterX = worldX - (p.X - ActualWidth / 2) / Scale;
        CenterY = worldY - (ActualHeight / 2 - p.Y) / Scale;
        InvalidateVisual();
    }

    private void OnDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            _dragging = true;
            _lastDrag = e.GetPosition(this);
            CaptureMouse();
        }
    }

    private void OnMove(object sender, MouseEventArgs e)
    {
        if (_dragging)
        {
            var p = e.GetPosition(this);
            CenterX -= (p.X - _lastDrag.X) / Scale;
            CenterY += (p.Y - _lastDrag.Y) / Scale;
            _lastDrag = p;
            InvalidateVisual();
        }
    }

    private void OnUp(object sender, MouseButtonEventArgs e)
    {
        _dragging = false;
        ReleaseMouseCapture();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        double w = ActualWidth, h = ActualHeight;
        if (w <= 0 || h <= 0) return;

        // 更新背景色（跟随主题）
        Background = new SolidColorBrush(BackgroundColor);
        dc.DrawRectangle(Background, null, new Rect(0, 0, w, h));

        DrawGrid(dc, w, h);
        DrawAxes(dc, w, h);
        foreach (var f in Functions) DrawFunction(dc, f, w, h);
    }

    private void DrawGrid(DrawingContext dc, double w, double h)
    {
        // 选择“美观”的网格间距：1,2,5 × 10^n
        double targetPx = 70; // 目标网格像素间距
        double rawStep = targetPx / Scale;
        double mag = Math.Pow(10, Math.Floor(Math.Log10(rawStep)));
        double norm = rawStep / mag;
        double step = norm switch
        {
            < 1.5 => 1, < 3.5 => 2, < 7.5 => 5, _ => 10,
        } * mag;

        var pen = new Pen(new SolidColorBrush(GridColor), 1);

        // 垂直网格线
        double leftWorld = (0 - w / 2) / Scale + CenterX;
        double rightWorld = (w - w / 2) / Scale + CenterX;
        double startX = Math.Floor(leftWorld / step) * step;
        for (double x = startX; x <= rightWorld + step; x += step)
        {
            double sx = (x - CenterX) * Scale + w / 2;
            dc.DrawLine(pen, new Point(sx, 0), new Point(sx, h));
        }

        // 水平网格线
        double topWorld = (h / 2) / Scale + CenterY;
        double bottomWorld = -(h / 2) / Scale + CenterY;
        double startY = Math.Floor(bottomWorld / step) * step;
        for (double y = startY; y <= topWorld + step; y += step)
        {
            double sy = h / 2 - (y - CenterY) * Scale;
            dc.DrawLine(pen, new Point(0, sy), new Point(w, sy));
        }
    }

    private void DrawAxes(DrawingContext dc, double w, double h)
    {
        var axisPen = new Pen(new SolidColorBrush(AxisColor), 1.5);
        double ox = (0 - CenterX) * Scale + w / 2; // y 轴屏幕 x
        double oy = h / 2 - (0 - CenterY) * Scale; // x 轴屏幕 y

        // x 轴（若在视野内）
        if (oy >= 0 && oy <= h)
            dc.DrawLine(axisPen, new Point(0, oy), new Point(w, oy));
        // y 轴
        if (ox >= 0 && ox <= w)
            dc.DrawLine(axisPen, new Point(ox, 0), new Point(ox, h));

        // 刻度标签
        double targetPx = 70;
        double rawStep = targetPx / Scale;
        double mag = Math.Pow(10, Math.Floor(Math.Log10(rawStep)));
        double norm = rawStep / mag;
        double step = norm switch { < 1.5 => 1, < 3.5 => 2, < 7.5 => 5, _ => 10 } * mag;

        var typeface = new Typeface("Segoe UI");
        var brush = new SolidColorBrush(TextColor);
        double leftWorld = (0 - w / 2) / Scale + CenterX;
        double rightWorld = (w - w / 2) / Scale + CenterX;

        double startX = Math.Floor(leftWorld / step) * step;
        for (double x = startX; x <= rightWorld + step; x += step)
        {
            if (Math.Abs(x) < step * 1e-6) continue;
            double sx = (x - CenterX) * Scale + w / 2;
            if (sx < 0 || sx > w) continue;
            var label = FormatTick(x);
            var text = new FormattedText(label, CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, typeface, 11, brush, 1.0);
            double ty = oy + 6;
            if (ty + text.Height > h) ty = h - text.Height;
            if (ty < 0) ty = 0;
            dc.DrawText(text, new Point(sx - text.Width / 2, ty));
        }

        double topWorld = (h / 2) / Scale + CenterY;
        double bottomWorld = -(h / 2) / Scale + CenterY;
        double startY = Math.Floor(bottomWorld / step) * step;
        for (double y = startY; y <= topWorld + step; y += step)
        {
            if (Math.Abs(y) < step * 1e-6) continue;
            double sy = h / 2 - (y - CenterY) * Scale;
            if (sy < 0 || sy > h) continue;
            var label = FormatTick(y);
            var text = new FormattedText(label, CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, typeface, 11, brush, 1.0);
            double tx = ox - text.Width - 6;
            if (tx < 0) tx = 2;
            dc.DrawText(text, new Point(tx, sy - text.Height / 2));
        }

        // 原点标签
        var otext = new FormattedText("O", CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight, typeface, 11, brush, 1.0);
        dc.DrawText(otext, new Point(ox - otext.Width - 4, oy + 4));
    }

    private void DrawFunction(DrawingContext dc, PlotFunction f, double w, double h)
    {
        if (f.Fn == null) return;
        var pen = new Pen(new SolidColorBrush(f.Color), f.Thickness);
        pen.Freeze();

        double leftWorld = (0 - w / 2) / Scale + CenterX;
        double rightWorld = (w - w / 2) / Scale + CenterX;
        double stepPx = 2.0;
        double stepWorld = stepPx / Scale;

        Point? prev = null;
        bool prevValid = false;

        for (double x = leftWorld; x <= rightWorld; x += stepWorld)
        {
            double y;
            try { y = f.Fn(x); }
            catch { prevValid = false; prev = null; continue; }

            if (double.IsNaN(y) || double.IsInfinity(y) || Math.Abs(y) > 1e8)
            {
                prevValid = false; prev = null;
                continue;
            }

            double sx = (x - CenterX) * Scale + w / 2;
            double sy = h / 2 - (y - CenterY) * Scale;
            var cur = new Point(sx, sy);

            if (prevValid && prev.HasValue)
            {
                // 若跨越过大（如 tan 的渐近线），断开
                if (Math.Abs(sy - prev.Value.Y) < h * 3)
                    dc.DrawLine(pen, prev.Value, cur);
                else prevValid = false;
            }
            prev = cur;
            prevValid = true;
        }
    }

    private static string FormatTick(double v)
    {
        if (Math.Abs(v) < 1e-9) return "0";
        if (Math.Abs(v) >= 10000 || (Math.Abs(v) < 0.001))
            return v.ToString("0.##e0", CultureInfo.InvariantCulture);
        return v.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static Color GetResourceColor(string key)
    {
        var b = Application.Current.Resources[key] as SolidColorBrush;
        return b?.Color ?? Colors.Gray;
    }
}

public sealed class PlotFunction
{
    public required string Expression { get; init; }
    public required Func<double, double> Fn { get; init; }
    public Color Color { get; set; } = Colors.White;
    public double Thickness { get; set; } = 2.0;
}
