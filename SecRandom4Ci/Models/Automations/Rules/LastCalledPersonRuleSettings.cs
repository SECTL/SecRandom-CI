using CommunityToolkit.Mvvm.ComponentModel;

namespace SecRandom4Ci.Models.Automations.Rules;

public partial class LastCalledPersonRuleSettings : ObservableRecipient
{
    [ObservableProperty] private bool _onRollCall = true;
    [ObservableProperty] private bool _onQuickDraw = true;
    [ObservableProperty] private bool _onLottery = true;
    
    [ObservableProperty] private bool _filterByPersonName = true;
    [ObservableProperty] private string _personName = string.Empty;
    
    [ObservableProperty] private bool _filterByGroupName = false;
    [ObservableProperty] private string _groupName = string.Empty;
    
    [ObservableProperty] private bool _filterByLotteryName = false;
    [ObservableProperty] private string _lotteryName = string.Empty;
}