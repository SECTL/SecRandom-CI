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
using SuperAutoIsland.Interface;
using SuperAutoIsland.Interface.MetaData;
using SuperAutoIsland.Interface.MetaData.ArgsType;
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
        return IPluginService.LoadedPlugins.Any(info => info.Manifest.Id == pkgName && info.IsEnabled && new Version(info.Manifest.Version) >= version);
    }

    private static void RegisterSaiBlocks()
    {
        var saiServerService = IAppHost.GetService<ISaiServer>();
        saiServerService.RegisterBlocks("SecRandom-Ci", new RegisterData
        {
            Actions =
            [
                new BlockMetadata
                {
                    Id = "secrandom4ci.actions.resetRecord",
                    Name = "重置 SecRandom 记录",
                    Icon = ("删除人员", "\uED09"),
                    Args = new Dictionary<string, MetaArgsBase>
                    {
                        ["Mode"] = new DropDownMetaArgs
                        {
                            Name = "",
                            Type = MetaType.dropdown,
                            Options = [
                                ("点名/闪抽", "0"),
                                ("抽奖", "1")
                            ]
                        }
                    },
                    DropdownUseNumbers = true,
                    InlineField = false,
                    InlineBlock = false,
                    IsRule = false
                }
            ],
            Rules =
            [
                new BlockMetadata
                {
                    Id = "secrandom4ci.rules.lastCalledPerson",
                    Name = "SecRandom 上次抽到",
                    Icon = ("人员可用", "\uECF9"),
                    Args = new Dictionary<string, MetaArgsBase>
                    {
                        ["_string1"] = new CommonMetaArgs
                        {
                            Name = "",
                            Type = MetaType.dummy
                        },
                        ["OnRollCall"] = new CommonMetaArgs
                        {
                            Name = "包含点名",
                            Type = MetaType.boolean
                        },
                        ["OnQuickDraw"] = new CommonMetaArgs
                        {
                            Name = "包含闪抽",
                            Type = MetaType.boolean
                        },
                        ["OnLottery"] = new CommonMetaArgs
                        {
                            Name = "包含抽奖",
                            Type = MetaType.boolean
                        },
                        ["FilterByPersonName"] = new CommonMetaArgs
                        {
                            Name = "按姓名筛选",
                            Type = MetaType.boolean
                        },
                        ["PersonName"] = new CommonMetaArgs
                        {
                            Name = "姓名",
                            Type = MetaType.text
                        },
                        ["FilterByGroupName"] = new CommonMetaArgs
                        {
                            Name = "按组名筛选",
                            Type = MetaType.boolean
                        },
                        ["GroupName"] = new CommonMetaArgs
                        {
                            Name = "组名",
                            Type = MetaType.text
                        },
                        ["FilterByLotteryName"] = new CommonMetaArgs
                        {
                            Name = "按奖项筛选",
                            Type = MetaType.boolean
                        },
                        ["LotteryName"] = new CommonMetaArgs
                        {
                            Name = "奖项",
                            Type = MetaType.text
                        }
                    },
                    DropdownUseNumbers = false,
                    InlineField = false,
                    InlineBlock = false,
                    IsRule = true
                }
            ]
        });
    }
}