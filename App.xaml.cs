using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace ScienceLab;

public partial class App : Application
{
    public static bool IsDark { get; private set; } = true;
    private static bool _selfTest;

    public static string LogDir { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ScienceLab", "logs");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 「发现新版本」检查：命令行开关 + 无人值守 probe
        UpdateKit.UpdateCheck.Configure(e.Args);
        var probeExit = UpdateKit.UpdateCheck.TryProbe(e.Args);
        if (probeExit.HasValue)
        {
            Shutdown(probeExit.Value);
            return;
        }

        try { Directory.CreateDirectory(LogDir); } catch { /* 忽略 */ }

        _selfTest = e.Args.Contains("--selftest");
        DispatcherUnhandledException += OnDispatcherException;

        if (_selfTest)
        {
            RunSelfTest(e);
            return;
        }

        // 正常启动后预热各模块视图（后台优先级，避免首次点击卡顿）
        Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, () =>
        {
            _ = Views.ViewsFactory.MathView;
            _ = Views.ViewsFactory.PhysicsView;
            _ = Views.ViewsFactory.ChemistryView;
        });

        // 启动后检查是否有新版本（发现则弹窗提示，可跳过该版本）
        Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
        {
            UpdateKit.UpdatePrompt.ScheduleAutoCheck(Current.MainWindow);
        }));
    }

    private void OnDispatcherException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogCrash(e.Exception);

        if (_selfTest)
        {
            // 自测模式下不吞异常：交给 selftest 记录后退出
            e.Handled = false;
            return;
        }

        MessageBox.Show(
            "发生错误，程序已拦截并记录日志。\n\n" +
            e.Exception.Message + "\n\n" +
            "日志位置：" + LogDir,
            "科学实验室", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private static void LogCrash(Exception ex)
    {
        try
        {
            var file = Path.Combine(LogDir, $"crash-{DateTime.Now:yyyyMMdd}.log");
            var sb = new StringBuilder();
            sb.AppendLine($"===== {DateTime.Now:yyyy-MM-dd HH:mm:ss} =====");
            sb.AppendLine(ex.ToString());
            if (ex.InnerException != null)
            {
                sb.AppendLine("--- inner ---");
                sb.AppendLine(ex.InnerException.ToString());
            }
            sb.AppendLine();
            File.AppendAllText(file, sb.ToString());
        }
        catch { /* 日志失败不能影响主流程 */ }
    }

    /// <summary>自测：创建主窗口并逐个导航到各模块，验证不崩溃。</summary>
    private void RunSelfTest(StartupEventArgs e)
    {
        // 日志输出路径：--log <path> 显式指定，否则用 LogDir
        var outPath = e.Args.Length > 1 && e.Args[0] == "--log"
            ? e.Args[1]
            : Path.Combine(LogDir, "selftest.log");

        var results = new StringBuilder();
        var win = new MainWindow
        {
            // 移到屏幕外且不激活，避免用户鼠标输入干扰自测
            ShowActivated = false,
            ShowInTaskbar = false,
            Left = -3000,
            Top = -3000,
        };
        try
        {
            win.Show();
            // Loaded 优先级高于 Input，连续鼠标输入不会饿死本次刷新
            win.Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.Loaded);

            foreach (var nav in new[] { "NavMath", "NavPhysics", "NavChemistry", "NavHome" })
            {
                try
                {
                    win.Navigate(nav);
                    // 强制完成布局、数据绑定与渲染管线（Loaded 优先级高于 Input）
                    win.Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.Loaded);
                    results.AppendLine($"OK   {nav}");
                }
                catch (Exception ex)
                {
                    results.AppendLine($"FAIL {nav}: {ex.GetType().Name}: {ex.Message}");
                    results.AppendLine(ex.StackTrace ?? "(no stack)");
                    var inner = ex.InnerException;
                    while (inner != null)
                    {
                        results.AppendLine($"INNER {inner.GetType().Name}: {inner.Message}");
                        results.AppendLine(inner.StackTrace ?? "(no stack)");
                        inner = inner.InnerException;
                    }
                    LogCrash(ex);
                }
            }
        }
        finally
        {
            win.Close();
        }

        Console.Write(results.ToString());
        try { File.WriteAllText(outPath, results.ToString()); } catch { }
        Shutdown(0);
    }

    public static void SetTheme(bool dark)
    {
        IsDark = dark;
        var brushes = dark
            ? new (string, string)[]
            {
                ("BgBrush", "#0D0D10"), ("SurfaceBrush", "#141419"), ("SurfaceHoverBrush", "#1C1C23"),
                ("BorderBrush", "#24242C"), ("PrimaryBrush", "#4D6BFE"), ("PrimaryHoverBrush", "#5B77FF"),
                ("PrimarySoftBrush", "#1A2440"),
                ("TextBrush", "#ECECF1"), ("TextSecondaryBrush", "#8E8E9A"), ("TextFaintBrush", "#5C5C66"),
                ("AccentGreenBrush", "#3ECF8E"), ("AccentOrangeBrush", "#F0A13A"),
                ("AccentRedBrush", "#F0645C"), ("AccentPurpleBrush", "#A78BFA"),
            }
            : new (string, string)[]
            {
                ("BgBrush", "#F7F7F9"), ("SurfaceBrush", "#FFFFFF"), ("SurfaceHoverBrush", "#F0F1F5"),
                ("BorderBrush", "#E6E7EC"), ("PrimaryBrush", "#4D6BFE"), ("PrimaryHoverBrush", "#3D5BF0"),
                ("PrimarySoftBrush", "#E9EDFF"),
                ("TextBrush", "#1A1B1F"), ("TextSecondaryBrush", "#6B6F7A"), ("TextFaintBrush", "#9BA0AA"),
                ("AccentGreenBrush", "#1E9E5A"), ("AccentOrangeBrush", "#D98A16"),
                ("AccentRedBrush", "#DC2626"), ("AccentPurpleBrush", "#8B5CF6"),
            };

        foreach (var (key, hex) in brushes)
        {
            var color = (Color)ColorConverter.ConvertFromString(hex);
            if (Current.Resources[key] is SolidColorBrush b)
                b.Color = color;
        }
    }
}
