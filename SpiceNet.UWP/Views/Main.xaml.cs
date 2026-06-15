using SpiceNet.UWP.ViewModels;

namespace SpiceNet.UWP.Views;

public sealed partial class Main : Page
{
    private MainViewModel Data;

    public Main()
    {
        this.InitializeComponent();
        Data = new();
    }
}
