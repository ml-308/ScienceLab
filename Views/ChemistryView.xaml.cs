using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ScienceLab.Chemistry;

namespace ScienceLab.Views;

public partial class ChemistryView : UserControl
{
    public ObservableCollection<ReagentVm> Reagents { get; } = new();
    public ObservableCollection<string> Logs { get; } = new();

    private int _containerIndex = 2;

    public ChemistryView()
    {
        InitializeComponent();

        foreach (var r in ReagentLibrary.All)
            Reagents.Add(new ReagentVm(r));

        ReagentList.ItemsSource = Reagents;
        LogList.ItemsSource = Logs;
        ElementGrid.ItemsSource = PeriodicTable.Elements;

        Canvas.Log += RefreshLog;
    }

    private void RefreshLog()
    {
        Logs.Clear();
        foreach (var entry in Canvas.LogEntries)
            Logs.Add(entry);
    }

    private void OnAddReagent(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is ReagentVm vm)
            Canvas.AddReagentToSelected(vm.Reagent);
    }

    private void OnAddContainer(object sender, RoutedEventArgs e)
    {
        _containerIndex++;
        Canvas.AddContainer($"烧杯 {_containerIndex}", new Point(120 + (_containerIndex % 4) * 160, 300), 160, 140);
    }

    private void OnClearSelected(object sender, RoutedEventArgs e) => Canvas.ClearSelected();

    private void OnClearAll(object sender, RoutedEventArgs e) => Canvas.ClearAll();

    private void OnElementClick(object sender, MouseButtonEventArgs e)
    {
        if ((sender as Border)?.Tag is Element el)
        {
            MessageBox.Show(
                $"元素：{el.Name}（{el.Symbol}）\n原子序数：{el.AtomicNumber}\n相对原子质量：{el.AtomicMass:0.###}\n类别：{el.Category}",
                "元素详情", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

public sealed class ReagentVm
{
    public Reagent Reagent { get; }
    public string Name => Reagent.Name;
    public string Formula => Reagent.Formula;
    public SolidColorBrush ColorBrush { get; }

    public ReagentVm(Reagent r)
    {
        Reagent = r;
        ColorBrush = new SolidColorBrush(r.Color.A > 0 ? r.Color : Color.FromRgb(0x88, 0x90, 0xA0));
    }
}
