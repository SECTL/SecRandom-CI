using ClassIsland.Core;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;
using ClassIsland.Shared;
using SecRandom4Ci.Services.NotificationProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SecRandom4Ci.Services;

namespace SecRandom4Ci;

[PluginEntrance]
public class Plugin : PluginBase
{
    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        services.AddNotificationProvider<SecRandomNotificationProvider>();
        services.AddSingleton<SecRandomService>();

        AppBase.Current.AppStarted += (sender, args) =>
        {
            IAppHost.GetService<SecRandomService>();
        };
    }
}