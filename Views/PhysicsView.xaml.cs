using System.Windows;
using System.Windows.Controls;

namespace ScienceLab.Views;

public partial class PhysicsView : UserControl
{
    public PhysicsView()
    {
        InitializeComponent();
        Canvas.StateChanged += UpdateStateDisplay;
        ApplyParameters();
        Canvas.Reset(StartPosSlider.Value, V0Slider.Value);
        UpdateStateDisplay();
    }

    private void ApplyParameters()
    {
        Canvas.AngleDeg = AngleSlider.Value;
        Canvas.Mass = MassSlider.Value;
        Canvas.Mu = MuSlider.Value;
        Canvas.Gravity = 9.8;
    }

    // XAML 解析期间 Slider.Minimum 会触发 ValueChanged，此时命名控件尚未赋值，
    // 必须对全部引用做判空，否则打开物理实验室即崩溃。
    private void OnAngleChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (Canvas == null || AngleText == null) return;
        Canvas.AngleDeg = e.NewValue;
        AngleText.Text = $"{e.NewValue:0}°";
        ResetState();
    }

    private void OnMassChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (Canvas == null || MassText == null) return;
        Canvas.Mass = e.NewValue;
        MassText.Text = $"{e.NewValue:0.0} kg";
        ResetState();
    }

    private void OnMuChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (Canvas == null || MuText == null) return;
        Canvas.Mu = e.NewValue;
        MuText.Text = $"{e.NewValue:0.00}";
        ResetState();
    }

    private void OnV0Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (V0Text == null) return;
        V0Text.Text = $"{e.NewValue:0.0} m/s";
    }

    private void OnStartPosChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (StartPosText == null) return;
        StartPosText.Text = $"{e.NewValue:0.0} m";
    }

    private void ResetState()
    {
        ApplyParameters();
        Canvas.Reset(StartPosSlider.Value, V0Slider.Value);
        if (PlayBtn != null) PlayBtn.Content = "▶ 播放";
    }

    private void OnPlayPause(object sender, RoutedEventArgs e)
    {
        if (Canvas.Running)
        {
            Canvas.Pause();
            PlayBtn.Content = "▶ 播放";
        }
        else
        {
            Canvas.Play();
            PlayBtn.Content = "⏸ 暂停";
        }
    }

    private void OnReset(object sender, RoutedEventArgs e)
    {
        ResetState();
        PlayBtn.Content = "▶ 播放";
    }

    private void UpdateStateDisplay()
    {
        if (PosText == null) return;
        PosText.Text = $"{Canvas.Position:0.00} m";
        VelText.Text = $"{Canvas.Velocity:0.00} m/s";
        AccText.Text = $"{Canvas.Acceleration:0.00} m/s²";
        // 若已停止，按钮恢复
        if (!Canvas.Running && PlayBtn != null) PlayBtn.Content = "▶ 播放";
    }
}
