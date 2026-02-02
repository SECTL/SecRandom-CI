using ClassIsland.Core.Abstractions.Services;
using SecRandom4Ci.Enums;
using SecRandom4Ci.Interface.Enums;
using SecRandom4Ci.Models.Automations.Rules;

namespace SecRandom4Ci.Services.Automations;

public class RuleHandlerService(IRulesetService rulesetService, SecRandomService secRandomService)
{
    private IRulesetService RulesetService { get; } = rulesetService;
    private SecRandomService SecRandomService { get; } = secRandomService;

    public void Register()
    {
        SecRandomService.WhenReceivedFinishNotification += (sender, data) =>
        {
            RulesetService.NotifyStatusChanged();
        };
        
        RulesetService.RegisterRuleHandler("secrandom4ci.rules.lastCalledPerson", HandleLastCalledPerson);
    }

    private bool HandleLastCalledPerson(object? objectSettings)
    {
        if (objectSettings is not LastCalledPersonRuleSettings settings || SecRandomService.LastFinishedNotificationData == null)
        {
            return false;
        }

        return SecRandomService.LastFinishedNotificationData.Items
            .Any(item => SecRandomService.LastFinishedNotificationData.ResultType switch
            {
                ResultType.FinishedRollCall =>
                    settings.Type is CalledPersonType.RollCall or CalledPersonType.RollCallAndQuickDraw
                    && item.StudentName == settings.PersonName,
                ResultType.FinishedQuickDraw =>
                    settings.Type is CalledPersonType.QuickDraw or CalledPersonType.RollCallAndQuickDraw
                    && item.StudentName == settings.PersonName,
                ResultType.FinishedLottery =>
                    settings.Type is CalledPersonType.Lottery
                    && item.StudentName == settings.PersonName && item.LotteryName == settings.LotteryName,
                _ => false
            });
    }
}