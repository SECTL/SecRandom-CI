using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Shared;
using dotnetCampus.Ipc.CompilerServices.GeneratedProxies;
using SecRandom4Ci.Services.NotificationProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SecRandom4Ci.Interface.Models;
using SecRandom4Ci.Interface.Services;

namespace SecRandom4Ci.Services;

public class SecRandomService : ISecRandomService
{
    public IIpcService IpcService { get; }
    private SecRandomNotificationProvider SecRandomNotificationProvider { get; }
        = IAppHost.Host!.Services.GetServices<IHostedService>().OfType<SecRandomNotificationProvider>().First();

    public SecRandomService(IIpcService ipcService)
    {
        IpcService = ipcService;
        IpcService.IpcProvider.CreateIpcJoint<ISecRandomService>(this);
    }
    
    public void NotifyResult(CallResult result)
    {
        SecRandomNotificationProvider.ShowResult(result);
    }
}