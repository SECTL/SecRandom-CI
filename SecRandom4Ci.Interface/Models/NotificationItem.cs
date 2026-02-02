using CommunityToolkit.Mvvm.ComponentModel;

namespace SecRandom4Ci.Interface.Models;

public partial class NotificationItem : ObservableRecipient
{
    [ObservableProperty] private bool _isLottery = false;
    [ObservableProperty] private string _lotteryName = string.Empty;
    
    [ObservableProperty] private bool _hasGroup = false;
    [ObservableProperty] private string _groupName = string.Empty;
    
    [ObservableProperty] private int _studentId = 0;
    [ObservableProperty] private string _studentName = string.Empty;
    
    [ObservableProperty] private bool _exists = true;
}