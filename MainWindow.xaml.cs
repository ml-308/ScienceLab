using System.Windows;
using System.Windows.Controls;
using ScienceLab.Views;

namespace ScienceLab;

public partial class MainWindow : Window
{
    private static readonly (string Name, string Title, string Sub)[] NavMeta =
    {
        ("NavHome", "首页", "自由开放的数学 · 物理 · 化学学习工具集"),
        ("NavMath", "数学 · 坐标系", "自定义函数图像 · 自由缩放平移 · 测量工具"),
        ("NavPhysics", "物理实验室", "小物块 · 斜面滑台 · 运动仿真 · 实时受力与状态"),
        ("NavChemistry", "化学实验室", "2D 自由实验室 · 混合试剂 · 反应可视化 · 元素周期表"),
    };

    public MainWindow()
    {
        InitializeComponent();
        ContentHost.Content = ViewsFactory.Home;
    }

    public void Navigate(string navName)
    {
        var nav = navName switch
        {
            "NavMath" => NavMath,
            "NavPhysics" => NavPhysics,
            "NavChemistry" => NavChemistry,
            _ => NavHome,
        };
        nav.IsChecked = true;
        ApplyNav(navName);
    }

    private void OnNavChanged(object sender, RoutedEventArgs e)
    {
        if (ContentHost == null) return;
        ApplyNav((sender as RadioButton)?.Name ?? "NavHome");
    }

    private void ApplyNav(string navName)
    {
        ContentHost.Content = navName switch
        {
            "NavMath" => ViewsFactory.MathView,
            "NavPhysics" => ViewsFactory.PhysicsView,
            "NavChemistry" => ViewsFactory.ChemistryView,
            _ => ViewsFactory.Home,
        };

        foreach (var (name, title, sub) in NavMeta)
        {
            if (name == navName)
            {
                HeaderTitle.Text = title;
                HeaderSub.Text = sub;
                break;
            }
        }
    }

    private void OnToggleTheme(object sender, RoutedEventArgs e)
    {
        App.SetTheme(!App.IsDark);
        ThemeIcon.Text = App.IsDark ? "🌙" : "☀️";
    }
}
