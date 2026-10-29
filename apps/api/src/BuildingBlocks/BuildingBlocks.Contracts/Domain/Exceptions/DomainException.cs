using BuildingBlocks.Shared;

namespace BuildingBlocks.Contracts.Domain.Exceptions;

/// <summary>
/// Gốc của các exception nghiệp vụ. Message được trả nguyên văn cho người dùng
/// nên phải là câu thân thiện, KHÔNG chứa thông tin kỹ thuật.
/// </summary>
public abstract class DomainException(string title, string message) : Exception(message)
{
    public string Title { get; } = title;

    /// <summary>Mã lỗi định danh (vd: PRODUCT_CODE_DUPLICATE). Lớp con override để FE phân biệt.</summary>
    public virtual string? Code => null;
}

/// <summary>HTTP 404. Mỗi domain tạo {DomainName}NotFoundException kế thừa lớp này.</summary>
public abstract class NotFoundException(string message) : DomainException("Not Found", message)
{
    public override string? Code => ErrorCodes.NotFound;
}

/// <summary>HTTP 400.</summary>
public abstract class BadRequestException(string message) : DomainException("Bad Request", message)
{
    public override string? Code => ErrorCodes.BadRequest;
}

/// <summary>HTTP 409 — xung đột trạng thái dữ liệu (trùng bản ghi, sai trạng thái...).</summary>
public abstract class ConflictException(string message) : DomainException("Conflict", message)
{
    public override string? Code => ErrorCodes.Conflict;
}

/// <summary>HTTP 403 — không có quyền thao tác.</summary>
public abstract class AccessDeniedException(string message) : DomainException("Access Denied", message)
{
    public override string? Code => ErrorCodes.Forbidden;
}
