using SpiceNet.UWP.Services;
using SpiceNet.UWP.ViewModels;

namespace SpiceNet.UWP.Views;

public sealed partial class Main : Page
{
    private readonly MainViewModel Data = Service.MainPage;

    public Main()
    {
        this.InitializeComponent();
    }

}
