using Bms.Application.SystemInfo.Queries.GetSystemInfo;
using Bms.UnitTests.Fakes;
using BuildingBlocks.Shared;

namespace Bms.UnitTests.Application;

public sealed class GetSystemInfoQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAppInfoWithServerTime()
    {
        var now = new DateTimeOffset(2026, 10, 29, 8, 0, 0, TimeSpan.Zero);
        var handler = new GetSystemInfoQueryHandler(new FakeHostEnvironment("Development"), new FixedTimeProvider(now));

        var result = await handler.Handle(new GetSystemInfoQuery(), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var info = result.GetValueOrThrow();
        Assert.Equal(AppConstants.AppName, info.Name);
        Assert.Equal("Development", info.Environment);
        Assert.Equal(now, info.ServerTime);
        Assert.Equal("0.1.0", info.Version);
    }
}
