using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using SecRandom4Ci.Enums;
using SecRandom4Ci.Models.Automations.Actions;
using SecRandom4Ci.Shared;

namespace SecRandom4Ci.Services.Automations.Actions;

[ActionInfo("secrandom4ci.actions.resetRecord", "重置 SecRandom 记录", "\uED09")]
public class ResetRecordAction : ActionBase<ResetRecordActionSettings>
{
    protected override async Task OnInvoke()
    {
        var url = Settings.Mode switch
        {
            SecRandomRecordMode.RollCall => "secrandom://roll_call/reset",
            SecRandomRecordMode.Lottery => "secrandom://lottery/reset",
            _ => string.Empty
        };
        
        if (url == string.Empty) return;

        _ = SecRandomIpcSendUrl.SendUrlAsync(url);
    }
}