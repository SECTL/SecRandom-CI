using ClassIsland.Core.Icons;
using ClassIsland.Shared;
using SuperAutoIsland.Interface.Services.Automations;

namespace SecRandom4Ci.Services.Automations.Blocks;

public class LastCalledPeopleBlock : DataBlockBase
{
    public override string Id => "secrandom4ci.data.lastCalledPeople";
    public override string Name => "SecRandom 上次抽到的所有人";
    public override (string, string) Icon => ("人员可用", FluentIcons.PeopleTeamRegular);
    public override string DataOutput => "Array";

    public override Task<object> Handler(object? data)
    {
        var secRandomService = IAppHost.GetService<SecRandomService>();
        var notification = secRandomService.LastFinishedNotificationData;
        var items = notification?.Items ?? [];
        var result = items
            .Select(item => string.Join("-", new[]
            {
                item.IsLottery ? item.LotteryName : null,
                item.HasGroup ? item.GroupName : null,
                item.StudentName
            }.Where(value => !string.IsNullOrWhiteSpace(value))))
            .ToArray();

        return Task.FromResult<object>(result);
    }
}
