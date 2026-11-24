using HalvingFun.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace HalvingFun;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        _services = services;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var home = _services.GetRequiredService<HomePage>();
        return new Window(new NavigationPage(home));
    }
}
