namespace BuildingBlocks.Shared;

/// <summary>Hằng số cấp ứng dụng.</summary>
public static class AppConstants
{
    public const string AppName = "BMS - Quản lý doanh nghiệp";

    /// <summary>Tiền tố route API phiên bản 1 (versioning theo route prefix).</summary>
    public const string ApiV1Prefix = "api/v1";
}

/// <summary>Tên claim đọc từ JWT/HttpContext.</summary>
public static class ClaimNames
{
    public const string Subject = "sub";
    public const string TenantId = "tenant_id";
    public const string CompanyId = "company_id";
    /// <summary>"true" khi token ở chế độ Tất cả công ty (chỉ đọc).</summary>
    public const string AllCompanies = "all_companies";
    /// <summary>Một claim cho mỗi công ty được đọc ở chế độ Tất cả công ty.</summary>
    public const string ReadableCompany = "readable_company";
    public const string Permission = "perm";
    public const string Name = "name";
    public const string Username = "username";
}
