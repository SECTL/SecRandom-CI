using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Services.NotificationProviders;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Models.Notification;
using ClassIsland.Core.Models.Notification.Templates;
using HarmonyLib;
using SecRandom4Ci.Interface.Models;
using NotificationRequest = ClassIsland.Core.Models.Notification.NotificationRequest;

namespace SecRandom4Ci.Services.NotificationProviders;

[NotificationProviderInfo("A0C99B32-EFA4-4843-ADF6-54DE7C6FCD56", "SecRandom抽选结果", "\uECB7", "显示SecRandom的抽选结果")]
public class SecRandomNotificationProvider : NotificationProviderBase
{
    private NotificationRequest? _request = null;
    
    public void ShowResult(CallResult result)
    {
        var names = result.SelectedStudents
            .Select(student => student.StudentName)
            .Join() ?? "???";
        
        Dispatcher.UIThread.Invoke(() =>
        {
            if (_request != null)
            {
                var content = (TwoIconsMaskTemplateData?)_request.MaskContent.Content;
                content?.Text = $"点名结果: {names}";
                return;
            }
            
            _request = new NotificationRequest
            {
                MaskContent = NotificationContent.CreateTwoIconsMask(
                    $"点名结果: {names}", hasRightIcon: true, rightIcon: "\uECB7",
                    factory: x =>
                    {
                        x.Duration = TimeSpan.FromSeconds(result.DisplayDuration);
                        x.IsSpeechEnabled = false;
                    }),
            };
            _request.CompletedToken.Register(() => _request = null);
            ShowNotification(_request);
        });
    }
}