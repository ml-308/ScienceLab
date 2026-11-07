using System.Windows.Controls;
using System.Windows.Input;

namespace ScienceLab.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
    }

    private void OpenMath(object sender, MouseButtonEventArgs e)
        => MainWindowRef?.Navigate("NavMath");

    private void OpenPhysics(object sender, MouseButtonEventArgs e)
        => MainWindowRef?.Navigate("NavPhysics");

    private void OpenChemistry(object sender, MouseButtonEventArgs e)
        => MainWindowRef?.Navigate("NavChemistry");

    private static MainWindow? MainWindowRef
        => System.Windows.Application.Current.MainWindow as MainWindow;
}
