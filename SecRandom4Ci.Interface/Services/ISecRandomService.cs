using dotnetCampus.Ipc.CompilerServices.Attributes;
using SecRandom4Ci.Interface.Models;

namespace SecRandom4Ci.Interface.Services;

[IpcPublic(IgnoresIpcException = true)]
public interface ISecRandomService
{
    void NotifyResult(CallResult result);
    string IsAlive();
}