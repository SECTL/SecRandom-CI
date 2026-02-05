using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Shared;
using dotnetCampus.Ipc.CompilerServices.GeneratedProxies;
using SecRandom4Ci.Services.NotificationProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SecRandom4Ci.Interface.Enums;
using SecRandom4Ci.Interface.Models;
using SecRandom4Ci.Interface.Services;

namespace SecRandom4Ci.Services;

public class SecRandomService : ISecRandomService
{
    private ILogger<SecRandomService> Logger { get; }
    private IIpcService IpcService { get; }

    public event EventHandler<NotificationData>? WhenReceivedNotification;
    public NotificationData? LastNotificationData { get; private set; } = null;
    
    public event EventHandler<NotificationData>? WhenReceivedFinishNotification;
    public NotificationData? LastFinishedNotificationData { get; private set; } = null;

    public SecRandomService(ILogger<SecRandomService> logger, IIpcService ipcService)
    {
        Logger = logger;
        IpcService = ipcService;
        IpcService.IpcProvider.CreateIpcJoint<ISecRandomService>(this);
    }

    private List<NotificationItem> ProcessStudents(List<NotificationItem> before)
    {
        for (var i = 0; i < before.Count; i++)
        {
            Logger.LogInformation("--> {INDEX} {NAME}", i, before[i].StudentName);
        }

        return before;
    }

    public void NotifyResult(CallResult result)
    {
        for (var i = 0; i < result.SelectedStudents.Count; i++)
        {
            Logger.LogInformation("--> {INDEX} {NAME}", i, result.SelectedStudents[i].StudentName);
        }
        
        ShowNotification(new NotificationData
        {
            ResultType = ResultType.Legacy,
            ClassName = result.ClassName,
            DrawCount = result.DrawCount,
            DisplayDuration = result.DisplayDuration,
            Items = result.SelectedStudents
                .Select(student => new NotificationItem
                {
                    StudentId = student.StudentId,
                    StudentName = student.StudentName,
                    Exists = student.Exists,
                })
                .ToList()
        });
    }

    public void ShowNotification(NotificationData data)
    {
        Logger.LogDebug("收到通知消息 {TYPE} 人数 {DRAW_COUNT} 实际人数 {REAL_COUNT}",
            data.ResultType, data.DrawCount, data.Items.Count);

        LastNotificationData = data;
        WhenReceivedNotification?.Invoke(this, data);
        
        if (data.ResultType is
            ResultType.FinishedRollCall or ResultType.FinishedQuickDraw or ResultType.FinishedLottery)
        {
            LastFinishedNotificationData = data;
            WhenReceivedFinishNotification?.Invoke(this, data);
        }
    }

    public Version GetPluginVersion()
    {
        return Plugin.PluginVersion;
    }

    public string IsAlive()
    {
        return "Yes";
    }
}