using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Services;
using Microsoft.Extensions.Logging;
using SecRandom4Ci.Interface.Enums;
using SecRandom4Ci.Models.Automations.Rules;

namespace SecRandom4Ci.Services.Automations;

public class RuleHandlerService(ILogger<RuleHandlerService> logger, IRulesetService rulesetService, SecRandomService secRandomService)
{
    private ILogger<RuleHandlerService> Logger { get; } = logger;
    private IRulesetService RulesetService { get; } = rulesetService;
    private SecRandomService SecRandomService { get; } = secRandomService;

    public void Register()
    {
        SecRandomService.WhenReceivedFinishNotification += (sender, data) =>
        {
            Logger.LogTrace("收到 SecRandom 消息，刷新规则集。");
            Dispatcher.UIThread.Invoke(() => RulesetService.NotifyStatusChanged());
        };
        
        RulesetService.RegisterRuleHandler("secrandom4ci.rules.lastCalledPerson", HandleLastCalledPerson);
    }

    private bool HandleLastCalledPerson(object? objectSettings)
    {
        var data = SecRandomService.LastFinishedNotificationData;
        if (objectSettings is not LastCalledPersonRuleSettings settings || data == null)
        {
            return false;
        }

        switch (data.ResultType)
        {
            case ResultType.FinishedRollCall when settings.OnRollCall:
            case ResultType.FinishedLottery when settings.OnLottery:
            case ResultType.FinishedQuickDraw when settings.OnQuickDraw:
                return data.Items
                    .Any(item =>
                    {
                        if (settings.FilterByPersonName && item.StudentName != settings.PersonName)
                        {
                            return false;
                        }
                
                        if (settings.FilterByGroupName && item.GroupName != settings.GroupName)
                        {
                            return false;
                        }
                
                        if (settings.FilterByLotteryName && item.LotteryName != settings.LotteryName)
                        {
                            return false;
                        }

                        return true;
                    });
            default:
                return false;
        }
    }
}