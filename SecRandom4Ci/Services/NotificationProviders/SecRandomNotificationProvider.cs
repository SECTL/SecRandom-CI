using System.Collections.ObjectModel;
using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Services.NotificationProviders;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Models.Notification;
using DynamicData;
using SecRandom4Ci.Controls;
using SecRandom4Ci.Interface.Enums;
using SecRandom4Ci.Interface.Models;
using NotificationRequest = ClassIsland.Core.Models.Notification.NotificationRequest;

namespace SecRandom4Ci.Services.NotificationProviders;

[NotificationProviderInfo("A0C99B32-EFA4-4843-ADF6-54DE7C6FCD56", "SecRandom抽选结果", "\uECB6", "显示SecRandom的抽选结果")]
public class SecRandomNotificationProvider : NotificationProviderBase
{
    private ResultNotificationControl? _maskControl = null;
    private SecRandomService SecRandomService { get; }

    public SecRandomNotificationProvider(SecRandomService secRandomService)
    {
        SecRandomService = secRandomService;

        secRandomService.WhenReceivedNotification += (sender, data) =>
        {
            ShowNotificationData(data);
        };
    }
    
    public void ShowNotificationData(NotificationData data)
    {
        var prefix = data.ResultType switch
        {
            ResultType.Legacy => "抽选结果",
            ResultType.PartialRollCall => "正在点名",
            ResultType.FinishedRollCall => "点名结果",
            ResultType.PartialQuickDraw => "正在闪抽",
            ResultType.FinishedQuickDraw => "闪抽结果",
            ResultType.PartialLottery => "正在抽奖",
            ResultType.FinishedLottery => "抽奖结果",
            _ => "未知结果"
        };
        
        Dispatcher.UIThread.Invoke(() =>
        {
            if (_maskControl != null)
            {
                _maskControl.Model.Prefix = prefix;
                _maskControl.Model.Items.Clear();
                _maskControl.Model.Items.AddRange(data.Items);
                return;
            }

            _maskControl = new ResultNotificationControl
            {
                Model =
                {
                    Prefix = prefix,
                    Items = new ObservableCollection<NotificationItem>(data.Items)
                }
            };
            
            var request = new NotificationRequest
            {
                MaskContent = new NotificationContent(_maskControl)
                {
                    Duration = TimeSpan.FromSeconds(data.DisplayDuration),
                    IsSpeechEnabled = false
                }
            };
            request.CompletedToken.Register(() => _maskControl = null);
            ShowNotification(request);
        });
    }
}