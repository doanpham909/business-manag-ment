using BuildingBlocks.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Bms.Infrastructure.Security;

/// <summary>Yêu cầu có một mã permission (policy "perm:{code}").</summary>
internal sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public const string PolicyPrefix = "perm:";

    public string Permission { get; } = permission;
}

/// <summary>
/// Kiểm tra claim "perm". Ở chế độ Tất cả công ty (chỉ đọc) chỉ cho các quyền xem/xuất —
/// thao tác ghi bị từ chối 403 trước khi tới interceptor.
/// </summary>
internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private static readonly string[] ReadOnlyActions = [".view", ".export", ".view-sensitive"];

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var user = context.User;
        if (!user.HasClaim(ClaimNames.Permission, requirement.Permission))
            return Task.CompletedTask;

        var allCompanies = user.HasClaim(ClaimNames.AllCompanies, "true");
        if (allCompanies && !ReadOnlyActions.Any(a => requirement.Permission.EndsWith(a, StringComparison.Ordinal)))
            return Task.CompletedTask;

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

/// <summary>Sinh policy động từ tên "perm:{code}" thay vì đăng ký từng policy.</summary>
internal sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(PermissionRequirement.PolicyPrefix, StringComparison.Ordinal))
            return await base.GetPolicyAsync(policyName);

        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(policyName[PermissionRequirement.PolicyPrefix.Length..]))
            .Build();
    }
}
