using System.Security.Claims;
using Bms.Infrastructure.Security;
using BuildingBlocks.Shared;
using Microsoft.AspNetCore.Authorization;

namespace Bms.UnitTests.Infrastructure;

public sealed class PermissionAuthorizationTests
{
    private static async Task<bool> AuthorizeAsync(string permission, params Claim[] claims)
    {
        var requirement = new PermissionRequirement(permission);
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext([requirement], user, null);
        await new PermissionAuthorizationHandler().HandleAsync(context);
        return context.HasSucceeded;
    }

    [Fact]
    public async Task Succeeds_WhenUserHasPermissionClaim() =>
        Assert.True(await AuthorizeAsync("admin.user.create", new Claim(ClaimNames.Permission, "admin.user.create")));

    [Fact]
    public async Task Fails_WhenPermissionMissing() =>
        Assert.False(await AuthorizeAsync("admin.user.create", new Claim(ClaimNames.Permission, "admin.user.view")));

    [Fact]
    public async Task AllCompaniesMode_AllowsViewButBlocksWrite()
    {
        Claim[] claims =
        [
            new(ClaimNames.AllCompanies, "true"),
            new(ClaimNames.Permission, "admin.user.view"),
            new(ClaimNames.Permission, "admin.user.create"),
        ];

        Assert.True(await AuthorizeAsync("admin.user.view", claims));
        Assert.False(await AuthorizeAsync("admin.user.create", claims));
    }
}
