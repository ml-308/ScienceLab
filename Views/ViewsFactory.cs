using System.Windows.Controls;

namespace ScienceLab.Views;

public static class ViewsFactory
{
    private static HomeView? _home;
    private static MathView? _math;
    private static PhysicsView? _physics;
    private static ChemistryView? _chemistry;

    public static HomeView Home => _home ??= new HomeView();
    public static MathView MathView => _math ??= new MathView();
    public static PhysicsView PhysicsView => _physics ??= new PhysicsView();
    public static ChemistryView ChemistryView => _chemistry ??= new ChemistryView();
}
