using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ScienceLab.Controls;
using ScienceLab.Utils;

namespace ScienceLab.Views;

public partial class MathView : UserControl
{
    private static readonly Color[] Palette =
    {
        Color.FromRgb(0x4D, 0x6B, 0xFE), // 蓝
        Color.FromRgb(0x2F, 0xBF, 0x71), // 绿
        Color.FromRgb(0xF5, 0xA5, 0x24), // 橙
        Color.FromRgb(0xEF, 0x44, 0x44), // 红
        Color.FromRgb(0xA7, 0x8B, 0xFA), // 紫
        Color.FromRgb(0x22, 0xD3, 0xEE), // 青
        Color.FromRgb(0xFB, 0x71, 0x85), // 粉
        Color.FromRgb(0xFB, 0xF2, 0x4C), // 黄
    };

    public ObservableCollection<PlotItem> Items { get; } = new();

    public MathView()
    {
        InitializeComponent();
        FuncList.ItemsSource = Items;
    }

    private void OnAddFunction(object sender, RoutedEventArgs e) => AddFunction(FuncInput.Text);

    private void OnInputKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) AddFunction(FuncInput.Text);
    }

    private void OnPresetSelected(object sender, SelectionChangedEventArgs e)
    {
        if (PresetBox.SelectedItem is ComboBoxItem item && item.Tag is string expr)
            FuncInput.Text = expr;
    }

    private void AddFunction(string expr)
    {
        if (string.IsNullOrWhiteSpace(expr)) return;
        if (!ExpressionParser.TryCompile(expr, out var fn, out var error))
        {
            MessageBox.Show($"函数表达式有误：\n{error}", "无法绘制", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var color = Palette[Items.Count % Palette.Length];
        var plot = new PlotFunction { Expression = expr.Trim(), Fn = fn!, Color = color };
        Canvas.Functions.Add(plot);
        Items.Add(new PlotItem(expr.Trim(), new SolidColorBrush(color)));
        Canvas.InvalidateVisual();
        FuncInput.Text = "";
    }

    private void OnRemoveFunction(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is PlotItem item)
        {
            int idx = Items.IndexOf(item);
            if (idx >= 0)
            {
                Items.RemoveAt(idx);
                Canvas.Functions.RemoveAt(idx);
                Canvas.InvalidateVisual();
            }
        }
    }

    private void OnReset(object sender, RoutedEventArgs e) => Canvas.ResetView();

    private void OnClear(object sender, RoutedEventArgs e)
    {
        Canvas.Functions.Clear();
        Items.Clear();
        Canvas.InvalidateVisual();
    }
}

public sealed class PlotItem
{
    public string Expression { get; }
    public SolidColorBrush Brush { get; }
    public PlotItem(string expr, SolidColorBrush brush) { Expression = expr; Brush = brush; }
}
