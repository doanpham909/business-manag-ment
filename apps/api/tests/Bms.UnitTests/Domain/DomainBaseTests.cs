using BuildingBlocks.Contracts.Domain.DDD;
using BuildingBlocks.Contracts.Domain.Entities;
using BuildingBlocks.Shared.Results;

namespace Bms.UnitTests.Domain;

public sealed class DomainBaseTests
{
    private sealed class NameMustNotBeEmptyRule(string name) : IBusinessRule
    {
        public bool IsBroken() => string.IsNullOrWhiteSpace(name);

        public string Message => "Tên không được để trống.";

        public string FieldName => "Name";
    }

    private sealed class Customer : EntityAuditBase<Guid>
    {
        private Customer(string name)
        {
            Id = Guid.CreateVersion7();
            Name = name;
        }

        public string Name { get; }

        public static Customer Create(string name)
        {
            CheckRule(new NameMustNotBeEmptyRule(name));
            return new Customer(name);
        }
    }

    private sealed class Money(decimal amount, string currency) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return amount;
            yield return currency;
        }
    }

    [Fact]
    public void CheckRule_BrokenRule_ThrowsBusinessRuleValidationException()
    {
        var exception = Assert.Throws<BusinessRuleValidationException>(() => Customer.Create(" "));

        Assert.Equal("Name", exception.BrokenRule.FieldName);
        Assert.Equal("Tên không được để trống.", exception.Message);
    }

    [Fact]
    public void Create_ValidInput_GeneratesVersion7Id()
    {
        var customer = Customer.Create("ACME");

        Assert.Equal(7, customer.Id.Version);
        Assert.False(customer.IsDeleted);
    }

    [Fact]
    public void ValueObject_SameComponents_AreEqual()
    {
        Assert.Equal(new Money(10, "VND"), new Money(10, "VND"));
        Assert.True(new Money(10, "VND") == new Money(10, "VND"));
        Assert.True(new Money(10, "VND") != new Money(10, "USD"));
        Assert.Equal(new Money(10, "VND").GetHashCode(), new Money(10, "VND").GetHashCode());
    }

    [Fact]
    public void Result_Invariants_AreEnforced()
    {
        var success = Result.Success(42);
        var failure = Result.Failure<int>(new Error("E", "boom"));

        Assert.True(success.IsSuccess);
        Assert.Equal(Error.None, success.Error);
        Assert.True(failure.IsFailure);
        Assert.Equal(default, failure.Value);
        Assert.Throws<InvalidOperationException>(() => failure.GetValueOrThrow());
        Assert.Throws<InvalidOperationException>(() => Result.Failure(Error.None));
    }

    [Fact]
    public void PagedResult_NormalizesPaging()
    {
        Assert.Equal(1, PagedResult<int>.NormalizePageIndex(0));
        Assert.Equal(PagedResult<int>.DefaultPageSize, PagedResult<int>.NormalizePageSize(0));
        Assert.Equal(PagedResult<int>.UpperPageSize, PagedResult<int>.NormalizePageSize(1000));

        var page = PagedResult<int>.Create([1, 2], pageIndex: 1, pageSize: 2, totalCount: 5);
        Assert.True(page.HasNextPage);
        Assert.False(page.HasPreviousPage);
    }
}
