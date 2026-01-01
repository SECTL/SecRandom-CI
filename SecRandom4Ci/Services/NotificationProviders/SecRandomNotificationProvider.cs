using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Services.NotificationProviders;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Models.Notification;
using ClassIsland.Core.Models.Notification.Templates;
using HarmonyLib;
using SecRandom4Ci.Interface.Models;
using NotificationRequest = ClassIsland.Core.Models.Notification.NotificationRequest;

namespace SecRandom4Ci.Services.NotificationProviders;

[NotificationProviderInfo("1A46E9B1-F1B5-4FAA-9EF2-1C47FB92F8C3", "点名结果", "\uECB7", "点名结果。")]
public class SecRandomNotificationProvider : NotificationProviderBase
{
    private NotificationRequest? _request = null;
    
    public void ShowResult(CallResult result)
    {
        var names = result.SelectedStudents
            .Select(student => student.StudentName)
            .Join() ?? "???";
        Console.WriteLine($"============> {names}");
        
        Dispatcher.UIThread.Invoke(() =>
        {
            if (_request != null)
            {
                Console.WriteLine("============> 复用提醒");
                var content = (TwoIconsMaskTemplateData?)_request.MaskContent.Content;
                content?.Text = $"点名结果: {names}";
                return;
            }
            
            Console.WriteLine($"============> 提醒时长{result.DisplayDuration}");
            _request = new NotificationRequest
            {
                MaskContent = NotificationContent.CreateTwoIconsMask(
                    $"点名结果: {names}", hasRightIcon: true, rightIcon: "\uECB7",
                    factory: x =>
                    {
                        x.Duration = TimeSpan.FromSeconds(result.DisplayDuration);
                    }),
            };
            _request.CompletedToken.Register(() => _request = null);
            ShowNotification(_request);
        });
    }
}