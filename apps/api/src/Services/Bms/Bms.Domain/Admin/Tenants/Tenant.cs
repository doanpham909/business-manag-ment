using Bms.Domain.Common.Rules;
using BuildingBlocks.Contracts.Domain.DDD;
using BuildingBlocks.Contracts.Domain.Entities;

namespace Bms.Domain.Admin.Tenants;

/// <summary>
/// Khách hàng của nền tảng (một nhóm công ty). Dữ liệu cấp nền tảng: không có tenant_id,
/// chỉ quản trị nền tảng tạo; người dùng trong tenant chỉ đọc tên.
/// </summary>
public sealed class Tenant : EventEntityAuditBase<Guid>, IAggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private Tenant() { }

    public static Tenant Create(string code, string name)
    {
        CheckRule(new TextMustNotBeEmptyRule(code, nameof(Code), "Mã khách hàng"));
        CheckRule(new TextMustNotBeEmptyRule(name, nameof(Name), "Tên khách hàng"));
        return new Tenant { Id = Guid.CreateVersion7(), Code = code.Trim(), Name = name.Trim(), IsActive = true };
    }
}
