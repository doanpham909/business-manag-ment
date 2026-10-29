using System.Security.Claims;
using Bms.Infrastructure.Identity;
using BuildingBlocks.Contracts.Domain.DDD;
using BuildingBlocks.Contracts.Domain.Exceptions;
using BuildingBlocks.Infrastructure.Exceptions;
using BuildingBlocks.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Bms.UnitTests.Infrastructure;

public sealed class ExceptionAndContextTests
{
    private sealed class OrderNotFoundException() : NotFoundException("Không tìm thấy đơn hàng.");

    private sealed class AlwaysBrokenRule : IBusinessRule
    {
        public bool IsBroken() => true;

        public string Message => "Rule broken";

        public string FieldName => "Quantity";
    }

    public static TheoryData<Exception, int> StatusCases => new()
    {
        { new ValidationException([new ValidationError("Name", "Required")]), 400 },
        { new OrderNotFoundException(), 404 },
        { new BusinessRuleValidationException(new AlwaysBrokenRule()), 422 },
        { new InvalidOperationException("secret connection info"), 500 },
        { new DbUpdateConcurrencyException("row changed"), 409 },
        { new DbUpdateException("save failed", new FakeDbException("23505")), 409 },
        { new DbUpdateException("save failed", new FakeDbException("23503")), 500 },
    };

    /// <summary>Giả lập lỗi provider (Npgsql PostgresException cũng kế thừa DbException + SqlState).</summary>
    private sealed class FakeDbException(string sqlState) : System.Data.Common.DbException("db error")
    {
        public override string SqlState => sqlState;
    }

    [Theory]
    [MemberData(nameof(StatusCases))]
    public void Map_ReturnsExpectedStatus(Exception exception, int expectedStatus)
    {
        Assert.Equal(expectedStatus, ExceptionMapper.Map(exception, "trace").Status);
    }

    [Fact]
    public void Map_UnknownException_DoesNotLeakMessage()
    {
        var response = ExceptionMapper.Map(new InvalidOperationException("secret connection info"), "trace");

        Assert.Equal(ErrorCodes.ServerError, response.Code);
        Assert.DoesNotContain("secret", response.Detail);
    }

    [Fact]
    public void Map_BusinessRule_ReturnsFieldError()
    {
        var response = ExceptionMapper.Map(new BusinessRuleValidationException(new AlwaysBrokenRule()), null);

        var error = Assert.Single(response.Errors!);
        Assert.Equal("Quantity", error.Code);
    }

    [Fact]
    public void ExecutionContext_ReadsSubAndTenantClaims()
    {
        var userId = Guid.CreateVersion7();
        var tenantId = Guid.CreateVersion7();
        var identity = new ClaimsIdentity(
            [new Claim(ClaimNames.Subject, userId.ToString()), new Claim(ClaimNames.TenantId, tenantId.ToString())],
            authenticationType: "Test");
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = new(identity) } };

        Assert.Equal(userId, new CurrentUser(accessor).UserId);
        Assert.Equal(tenantId, new TenantContext(accessor).TenantId);
    }

    [Fact]
    public void ExecutionContext_Anonymous_ReturnsNull()
    {
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };

        Assert.Null(new CurrentUser(accessor).UserId);
        Assert.Null(new TenantContext(accessor).TenantId);
    }
}
