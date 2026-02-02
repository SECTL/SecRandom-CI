using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using SecRandom4Ci.Interface.Models;

namespace SecRandom4Ci.Controls;

public partial class ResultNotificationControl : UserControl
{
    public partial class ControlModel : ObservableRecipient
    {
        [ObservableProperty] private string _prefix = string.Empty;
        [ObservableProperty] private ObservableCollection<NotificationItem> _items = [];
    }

    public ControlModel Model { get; } = new();
    
    public ResultNotificationControl()
    {
        DataContext = this;
        InitializeComponent();
    }
}