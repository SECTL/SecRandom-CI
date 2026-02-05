using ClassIsland.Core;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;
using ClassIsland.Shared;
using SecRandom4Ci.Services.NotificationProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SecRandom4Ci.Controls.Automations.RuleSettingsControls;
using SecRandom4Ci.Models.Automations.Rules;
using SecRandom4Ci.Services;
using SecRandom4Ci.Services.Automations;

namespace SecRandom4Ci;

[PluginEntrance]
public class Plugin : PluginBase
{
    internal static Version PluginVersion = Version.Parse("0.0.0.1");
    
    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        PluginVersion = Version.Parse(Info.Manifest.Version);
        
        // 注册服务
        services.AddSingleton<SecRandomService>();
        services.AddSingleton<RuleHandlerService>();
        
        // 注册 ClassIsland 元素
        services.AddNotificationProvider<SecRandomNotificationProvider>();
        services.AddRule<LastCalledPersonRuleSettings, LastCalledPersonRuleSettingsControl>(
            "secrandom4ci.rules.lastCalledPerson", "SecRandom 上次抽到", "\uECF9");

        AppBase.Current.AppStarted += (sender, args) =>
        {
            IAppHost.GetService<SecRandomService>();
            IAppHost.GetService<RuleHandlerService>().Register();
        };
    }
}