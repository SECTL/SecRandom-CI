using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using SecRandom4Ci.Interface.Models;
using SecRandom4Ci.Services;

namespace SecRandom4Ci.Models.Automations.Triggers;

[TriggerInfo("secrandom4ci.triggers.receivedNotification", "SecRandom 发送最终抽选结果时", "\uECF9")]
public class ReceivedNotificationTrigger(SecRandomService secRandomService) : TriggerBase
{
    private SecRandomService SecRandomService { get; } = secRandomService;
    
    public override void Loaded()
    {
        SecRandomService.WhenReceivedFinishNotification += SecRandomServiceOnWhenReceivedFinishNotification;
    }

    public override void UnLoaded()
    {
        SecRandomService.WhenReceivedFinishNotification -= SecRandomServiceOnWhenReceivedFinishNotification;
    }

    private void SecRandomServiceOnWhenReceivedFinishNotification(object? sender, NotificationData e)
    {
        Trigger();
    }
}