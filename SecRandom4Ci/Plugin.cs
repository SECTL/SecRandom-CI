using ClassIsland.Core;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;
using ClassIsland.Shared;
using SecRandom4Ci.Services.NotificationProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SecRandom4Ci.Controls.Automations.ActionSettingsControls;
using SecRandom4Ci.Controls.Automations.RuleSettingsControls;
using SecRandom4Ci.Models.Automations.Rules;
using SecRandom4Ci.Models.Automations.Triggers;
using SecRandom4Ci.Services;
using SecRandom4Ci.Services.Automations;
using SecRandom4Ci.Services.Automations.Actions;
using SuperAutoIsland.Interface.Metadata;
using SuperAutoIsland.Interface.Services;

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
        services.AddTrigger<ReceivedNotificationTrigger>();
        services.AddRule<LastCalledPersonRuleSettings, LastCalledPersonRuleSettingsControl>(
            "secrandom4ci.rules.lastCalledPerson", "SecRandom 上次抽到", "\uECF9");
        services.AddAction<ResetRecordAction, ResetRecordActionSettingsControl>();

        AppBase.Current.AppStarted += (sender, args) =>
        {
            IAppHost.GetService<SecRandomService>();
            IAppHost.GetService<RuleHandlerService>().Register();
            
            
            if (IsPluginInstalled("lrs2187.sai"))
            {
                RegisterSaiBlocks();
            }
        };
    }
    
    private static bool IsPluginInstalled(string pkgName, Version? version = null)
    {
        return IPluginService.LoadedPlugins.Any(info =>
            info.Manifest.Id == pkgName &&
            info.IsEnabled &&
            (version is null || new Version(info.Manifest.Version) >= version));
    }

    private static void RegisterSaiBlocks()
    {
        var saiServerService = IAppHost.GetService<ISaiServer>();
        saiServerService.RegisterBlocks("SecRandom-Ci", it => it
            .AddBlock(new BlockMetadata("secrandom4ci.actions.resetRecord")
            {
                Kind = BlockKind.Action,
                Name = "重置 SecRandom 记录",
                Icon = ("删除人员", "\uED09"),
                Fields = new Dictionary<string, Field>
                {
                    ["Mode"] = BasicFields.Dropdown("", [
                        ("点名/闪抽", "0"),
                        ("抽奖", "1")
                    ], useNumbers: true)
                }
            })
            .AddBlock(new BlockMetadata("secrandom4ci.rules.lastCalledPerson")
            {
                Kind = BlockKind.Rule,
                Name = "SecRandom 上次抽到",
                Icon = ("人员可用", "\uECF9"),
                Fields = new Dictionary<string, Field>
                {
                    ["_string1"] = BasicFields.Dummy(""),
                    ["OnRollCall"] = BasicFields.Boolean("包含点名", true),
                    ["OnQuickDraw"] = BasicFields.Boolean("包含闪抽", true),
                    ["OnLottery"] = BasicFields.Boolean("包含抽奖", true),
                    ["FilterByPersonName"] = BasicFields.Boolean("按姓名筛选", true),
                    ["PersonName"] = BasicFields.Text("姓名", "学生21"),
                    ["FilterByGroupName"] = BasicFields.Boolean("按组名筛选"),
                    ["GroupName"] = BasicFields.Text("组名"),
                    ["FilterByLotteryName"] = BasicFields.Boolean("按奖项筛选"),
                    ["LotteryName"] = BasicFields.Text("奖项")
                }
            }));
    }
}
