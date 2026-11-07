using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ScienceLab.Controls;

/// <summary>
/// 斜面 + 小物块 运动仿真画布。
/// 实时显示物块位置、速度、加速度与受力箭头。
/// </summary>
public class PhysicsCanvas : Canvas
{
    // ---- 可调参数 ----
    public double AngleDeg { get; set; } = 30;   // 斜面倾角（度）
    public double Mass { get; set; } = 2;        // 质量 kg
    public double Mu { get; set; } = 0.2;        // 动摩擦系数
    public double Gravity { get; set; } = 9.8;   // 重力加速度

    // ---- 仿真状态 ----
    public double Position { get; private set; }  // 物块沿斜面的位移（相对底端，向上为正）单位：m
    public double Velocity { get; private set; }  // 沿斜面向上为正 m/s
    public double Acceleration { get; private set; }

    public bool Running { get; private set; }

    private readonly DispatcherTimer _timer;
    private DateTime _lastTick;
    private const double PixelsPerMeter = 60;    // 斜面 1m = 60px

    private Color SurfaceColor => GetColor("SurfaceBrush");
    private Color BorderColor => GetColor("BorderBrush");
    private Color TextColor => GetColor("TextSecondaryBrush");
    private Color BlockColor => GetColor("AccentOrangeBrush");
    private Color GColor => GetColor("AccentRedBrush");     // 重力
    private Color NColor => GetColor("AccentGreenBrush");   // 支持力
    private Color FColor => GetColor("AccentPurpleBrush");  // 摩擦力

    public PhysicsCanvas()
    {
        Background = new SolidColorBrush(GetColor("BgBrush"));
        ClipToBounds = true;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += OnTick;
        SizeChanged += (_, _) => InvalidateVisual();
    }

    public event Action? StateChanged;

    public void Reset(double startPos, double startVel)
    {
        Position = startPos;
        Velocity = startVel;
        Acceleration = ComputeAcceleration();
        Running = false;
        _lastTick = DateTime.Now;
        InvalidateVisual();
        StateChanged?.Invoke();
    }

    public void Play()
    {
        if (!Running)
        {
            _lastTick = DateTime.Now;
            Running = true;
            _timer.Start();
        }
    }

    public void Pause()
    {
        Running = false;
        _timer.Stop();
    }

    public void StepOnce(double dt = 0.02)
    {
        Integrate(dt);
        InvalidateVisual();
        StateChanged?.Invoke();
    }

    private double ComputeAcceleration()
    {
        double rad = AngleDeg * Math.PI / 180;
        double g = Gravity;
        double a = g * Math.Sin(rad); // 下滑方向加速度（沿斜面向下为正）
        // 动摩擦：方向与速度相反（速度沿斜面向上为正）
        if (Math.Abs(Velocity) > 1e-6)
        {
            a -= Math.Sign(Velocity) * Mu * g * Math.Cos(rad);
        }
        else
        {
            // 速度为零：判断能否克服最大静摩擦（近似用 μ 表示）
            double maxFriction = Mu * g * Math.Cos(rad);
            double driving = g * Math.Sin(rad);
            if (driving <= maxFriction)
                a = 0; // 静止，静摩擦平衡
            else
                a = driving - maxFriction; // 开始下滑
        }
        return a;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        double dt = (now - _lastTick).TotalSeconds;
        _lastTick = now;
        if (dt <= 0 || dt > 0.1) dt = 0.016;
        Integrate(dt);
        InvalidateVisual();
        StateChanged?.Invoke();
    }

    private void Integrate(double dt)
    {
        Acceleration = ComputeAcceleration();
        Velocity += Acceleration * dt;
        Position += Velocity * dt;

        // 到达斜面底端（位置 <= 0）停止
        if (Position <= 0)
        {
            Position = 0;
            Velocity = 0;
            Acceleration = 0;
            Pause();
        }
        // 到达斜面顶端（超出可用长度）停止
        double maxLen = MaxSlopeLength();
        if (Position > maxLen)
        {
            Position = maxLen;
            Velocity = 0;
            Acceleration = 0;
            Pause();
        }
    }

    private double MaxSlopeLength()
    {
        // 斜面长度由画布高度决定（底端在左下角，斜面向上）
        double rad = AngleDeg * Math.PI / 180;
        double h = ActualHeight;
        return Math.Max(0.5, (h - 60) / PixelsPerMeter / Math.Sin(rad));
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        double w = ActualWidth, h = ActualHeight;
        if (w <= 0 || h <= 0) return;

        dc.DrawRectangle(new SolidColorBrush(GetColor("BgBrush")), null, new Rect(0, 0, w, h));
        DrawScene(dc, w, h);
    }

    private void DrawScene(DrawingContext dc, double w, double h)
    {
        double rad = AngleDeg * Math.PI / 180;
        // 斜面底端（原点）位置
        double originX = w * 0.25;
        double originY = h - 40;
        // 斜面长度（像素）
        double slopeLenPx = (h - 60) / Math.Sin(rad);
        double slopeLenM = slopeLenPx / PixelsPerMeter;
        // 斜面顶端坐标
        double topX = originX + slopeLenPx * Math.Cos(rad);
        double topY = originY - slopeLenPx * Math.Sin(rad);

        // 地面
        var groundPen = new Pen(new SolidColorBrush(BorderColor), 2);
        dc.DrawLine(groundPen, new Point(0, originY), new Point(w, originY));

        // 斜面
        var slopeBrush = new SolidColorBrush(SurfaceColor);
        var slopePen = new Pen(new SolidColorBrush(BorderColor), 2);
        // 斜面用一条粗线表示，或用填充三角形表示
        var slopeGeom = new StreamGeometry();
        using (var ctx = slopeGeom.Open())
        {
            ctx.BeginFigure(new Point(originX, originY), true, true);
            ctx.LineTo(new Point(topX, topY), true, false);
            ctx.LineTo(new Point(topX, originY), true, false);
        }
        dc.DrawGeometry(slopeBrush, slopePen, slopeGeom);

        // 角度标注
        var typeface = new Typeface("Segoe UI");
        var labelBrush = new SolidColorBrush(TextColor);
        var angleText = new FormattedText($"{AngleDeg:0}°", System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight, typeface, 14, labelBrush, 1.0);
        dc.DrawText(angleText, new Point(originX + 30, originY - 26));

        // ---- 物块 ----
        double posM = Math.Clamp(Position, 0, slopeLenM);
        // 物块沿斜面的位置（像素）
        double alongPx = posM * PixelsPerMeter;
        double bx = originX + alongPx * Math.Cos(rad);
        double by = originY - alongPx * Math.Sin(rad);

        double blockSize = 34;
        // 物块矩形（沿斜面旋转绘制）
        var blockBrush = new SolidColorBrush(BlockColor);
        var blockPen = new Pen(new SolidColorBrush(GetColor("TextBrush")), 1.5);
        var blockRect = new Rect(bx - blockSize / 2, by - blockSize / 2, blockSize, blockSize);

        dc.PushTransform(new RotateTransform(AngleDeg, bx, by));
        dc.DrawRoundedRectangle(blockBrush, blockPen, blockRect, 4, 4);
        // 物块质量标签
        var massText = new FormattedText($"{Mass:0.#}kg", System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight, typeface, 11, new SolidColorBrush(GetColor("TextBrush")), 1.0);
        dc.DrawText(massText, new Point(bx - massText.Width / 2, by - massText.Height / 2));
        dc.Pop();

        // ---- 受力箭头（在物块中心） ----
        double cx = bx, cy = by; // 矩形中心即 bx,by
        // 屏幕坐标：y 向下为正。斜面沿右上方向，单位向量 (cosθ, -sinθ)
        double upX = Math.Cos(rad), upY = -Math.Sin(rad);        // 沿斜面向上
        double normalX = -Math.Sin(rad), normalY = -Math.Cos(rad); // 垂直斜面向上（离开斜面）
        DrawForceArrow(dc, cx, cy, GColor, "G", 0, 1);            // 重力竖直向下
        DrawForceArrow(dc, cx, cy, NColor, "N", normalX, normalY); // 支持力
        // 摩擦力：沿斜面，与速度方向相反（速度沿斜面向上为正）
        double fDir = Velocity >= 0 ? -1 : 1;
        DrawForceArrow(dc, cx, cy, FColor, "f", fDir * upX, fDir * upY);

        // ---- 图例 ----
        double legendX = w - 150, legendY = 20;
        var legendBg = new SolidColorBrush(Color.FromArgb(120, 0, 0, 0));
        dc.DrawRoundedRectangle(legendBg, new Pen(new SolidColorBrush(BorderColor), 1),
            new Rect(legendX - 12, legendY - 10, 130, 90), 8, 8);
        DrawLegendItem(dc, legendX, legendY, GColor, "G  重力");
        DrawLegendItem(dc, legendX, legendY + 22, NColor, "N  支持力");
        DrawLegendItem(dc, legendX, legendY + 44, FColor, "f  摩擦力");
    }

    private void DrawForceArrow(DrawingContext dc, double x, double y, Color color, string label, double dirX, double dirY)
    {
        double len = 40;
        double ex = x + dirX * len;
        double ey = y + dirY * len;
        var brush = new SolidColorBrush(color);
        var pen = new Pen(brush, 2.5);
        dc.DrawLine(pen, new Point(x, y), new Point(ex, ey));
        // 箭头
        double ang = Math.Atan2(dirY, dirX);
        double a1 = ang + Math.PI * 0.85, a2 = ang - Math.PI * 0.85;
        double al = 8;
        dc.DrawLine(pen, new Point(ex, ey), new Point(ex - al * Math.Cos(a1), ey - al * Math.Sin(a1)));
        dc.DrawLine(pen, new Point(ex, ey), new Point(ex - al * Math.Cos(a2), ey - al * Math.Sin(a2)));
        // 标签
        var typeface = new Typeface("Segoe UI");
        var text = new FormattedText(label, System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight, typeface, 12, brush, 1.0);
        dc.DrawText(text, new Point(ex + 4, ey - text.Height / 2));
    }

    private void DrawLegendItem(DrawingContext dc, double x, double y, Color color, string label)
    {
        var typeface = new Typeface("Segoe UI");
        var brush = new SolidColorBrush(color);
        dc.DrawLine(new Pen(brush, 2.5), new Point(x, y + 8), new Point(x + 20, y + 8));
        var text = new FormattedText(label, System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight, typeface, 12, new SolidColorBrush(TextColor), 1.0);
        dc.DrawText(text, new Point(x + 26, y));
    }

    private static Color GetColor(string key)
    {
        var b = Application.Current.Resources[key] as SolidColorBrush;
        return b?.Color ?? Colors.Gray;
    }
}
