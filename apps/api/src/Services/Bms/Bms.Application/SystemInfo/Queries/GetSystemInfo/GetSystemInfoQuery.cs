using BuildingBlocks.Contracts.Messaging;

namespace Bms.Application.SystemInfo.Queries.GetSystemInfo;

/// <summary>Query mẫu: lấy thông tin hệ thống (tên app, version, môi trường, giờ server).</summary>
public sealed record GetSystemInfoQuery : IQuery<SystemInfoResponse>;

/// <summary>Dữ liệu trả về của <see cref="GetSystemInfoQuery"/>.</summary>
public sealed record SystemInfoResponse(
    string Name,
    string Version,
    string Environment,
    DateTimeOffset ServerTime);
