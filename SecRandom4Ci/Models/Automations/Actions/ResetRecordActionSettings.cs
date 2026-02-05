using CommunityToolkit.Mvvm.ComponentModel;
using SecRandom4Ci.Enums;

namespace SecRandom4Ci.Models.Automations.Actions;

public partial class ResetRecordActionSettings : ObservableRecipient
{
    [ObservableProperty] private SecRandomRecordMode _mode = SecRandomRecordMode.RollCall;
}