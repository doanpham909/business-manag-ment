using Bms.Application.Common.Paging;
using BuildingBlocks.Contracts.Abstractions;
using BuildingBlocks.Contracts.Auditing;
using BuildingBlocks.Contracts.Messaging;
using BuildingBlocks.Contracts.Persistence;
using BuildingBlocks.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Bms.Application.Admin.AuditLogs;

public sealed record AuditLogResponse(
    Guid Id, Guid? CompanyId, Guid? UserId, string EntityType, string EntityId, AuditAction Action,
    string Changes, DateTimeOffset OccurredAt);

/// <summary>Xem nhật ký thay đổi (ADM-08): trong công ty được đọc, gồm cả dữ liệu cấp tập đoàn (không gắn công ty).</summary>
public sealed record GetAuditLogsQuery(
    int PageIndex = 1, int PageSize = 20, string? EntityType = null, string? EntityId = null,
    Guid? UserId = null, DateTimeOffset? From = null, DateTimeOffset? To = null)
    : IQuery<PagedResult<AuditLogResponse>>;

internal sealed class GetAuditLogsQueryHandler(IRepositoryBase<AuditLog, Guid> logs, ICompanyContext company)
    : IQueryHandler<GetAuditLogsQuery, PagedResult<AuditLogResponse>>
{
    public async Task<Result<PagedResult<AuditLogResponse>>> Handle(GetAuditLogsQuery request, CancellationToken ct)
    {
        var readable = company.ReadableCompanyIds.ToArray();
        var query = logs.FindAll(l => l.CompanyId == null || readable.Contains(l.CompanyId.Value));

        if (!string.IsNullOrWhiteSpace(request.EntityType))
            query = query.Where(l => l.EntityType == request.EntityType);
        if (!string.IsNullOrWhiteSpace(request.EntityId))
            query = query.Where(l => l.EntityId == request.EntityId);
        if (request.UserId.HasValue)
            query = query.Where(l => l.UserId == request.UserId);
        if (request.From.HasValue)
            query = query.Where(l => l.OccurredAt >= request.From);
        if (request.To.HasValue)
            query = query.Where(l => l.OccurredAt <= request.To);

        return await query.OrderByDescending(l => l.OccurredAt)
            .Select(l => new AuditLogResponse(l.Id, l.CompanyId, l.UserId, l.EntityType, l.EntityId, l.Action, l.Changes, l.OccurredAt))
            .ToPagedResultAsync(request.PageIndex, request.PageSize, ct);
    }
}
