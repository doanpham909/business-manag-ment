using Bms.Application.Authorization;
using Bms.Domain.Admin.Companies;
using Bms.Domain.Admin.Roles;
using Bms.Domain.Admin.Users;
using BuildingBlocks.Contracts.Domain.DDD;

namespace Bms.UnitTests.Admin;

public sealed class AdminDomainTests
{
    [Fact]
    public void CreateCompany_WhenNameIsEmpty_ThrowsBusinessRuleException()
    {
        var ex = Assert.Throws<BusinessRuleValidationException>(() => Company.Create("C1", " ", null, null, null));
        Assert.Equal("Name", ex.BrokenRule.FieldName);
    }

    [Fact]
    public void CreateUser_NormalizesUsernameToLowerCase()
    {
        var user = User.Create("  Admin ", "Quản trị", null, null, "hash");
        Assert.Equal("admin", user.Username);
        Assert.Equal(UserStatus.Active, user.Status);
    }

    [Fact]
    public void SetRoles_WhenUserHasNoAccessToCompany_ThrowsBusinessRuleException()
    {
        var user = User.Create("a", "A", null, null, "hash");
        Assert.Throws<BusinessRuleValidationException>(() => user.SetRoles(Guid.CreateVersion7(), [Guid.CreateVersion7()]));
    }

    [Fact]
    public void SetCompanyAccess_RemovingCompany_AlsoRemovesItsRoles()
    {
        var user = User.Create("a", "A", null, null, "hash");
        var (c1, c2) = (Guid.CreateVersion7(), Guid.CreateVersion7());
        user.SetCompanyAccess([c1, c2]);
        user.SetRoles(c1, [Guid.CreateVersion7()]);
        user.SetRoles(c2, [Guid.CreateVersion7()]);

        user.SetCompanyAccess([c2]);

        Assert.Equal([c2], user.CompanyAccesses.Select(a => a.CompanyId));
        Assert.All(user.RoleAssignments, r => Assert.Equal(c2, r.CompanyId));
    }

    [Fact]
    public void SetPermissions_RemovesDuplicates()
    {
        var role = Role.Create("r", "R", null);
        role.SetPermissions(["admin.user.view", "admin.user.view", "admin.role.view"]);
        Assert.Equal(2, role.Permissions.Count);
    }

    [Fact]
    public void RoleTemplates_OnlyReferenceKnownPermissions()
    {
        foreach (var template in RoleTemplates.All)
            Assert.All(template.PermissionCodes, code => Assert.Contains(code, PermissionCatalog.AllCodes));
    }

    [Fact]
    public void TenantAdminTemplate_DoesNotSeeSalaryByDefault()
    {
        var admin = RoleTemplates.All.Single(t => t.IsSystem);
        Assert.DoesNotContain("payroll.payroll.view-sensitive", admin.PermissionCodes);
        Assert.Contains("admin.company.create", admin.PermissionCodes);
    }
}
