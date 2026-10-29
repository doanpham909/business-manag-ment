using System.Reflection;
using BuildingBlocks.Contracts.Messaging;
using BuildingBlocks.Shared;
using BuildingBlocks.Shared.Results;
using Microsoft.Extensions.Hosting;

namespace Bms.Application.SystemInfo.Queries.GetSystemInfo;

/// <summary>Handler của <see cref="GetSystemInfoQuery"/>. Không truy cập DB.</summary>
internal sealed class GetSystemInfoQueryHandler(IHostEnvironment environment, TimeProvider timeProvider)
    : IQueryHandler<GetSystemInfoQuery, SystemInfoResponse>
{
    /// <summary>Version lấy từ assembly (thuộc tính &lt;Version&gt; trong Directory.Build.props).</summary>
    private static readonly string AppVersion =
        typeof(GetSystemInfoQueryHandler).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "0.0.0";

    public Task<Result<SystemInfoResponse>> Handle(GetSystemInfoQuery request, CancellationToken cancellationToken)
    {
        var response = new SystemInfoResponse(
            AppConstants.AppName,
            AppVersion,
            environment.EnvironmentName,
            timeProvider.GetUtcNow());

        return Task.FromResult(Result.Success(response));
    }
}
