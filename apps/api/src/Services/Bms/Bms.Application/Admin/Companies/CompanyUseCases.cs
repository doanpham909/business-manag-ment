using Bms.Application.Common.Paging;
using Bms.Domain.Admin.Companies;
using BuildingBlocks.Contracts.Messaging;
using BuildingBlocks.Contracts.Persistence;
using BuildingBlocks.Shared.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Bms.Application.Admin.Companies;

public sealed record CompanyResponse(
    Guid Id, string Code, string Name, string? TaxCode, string? Address, string? Phone, CompanyStatus Status, uint Version);

public sealed record GetCompaniesQuery(int PageIndex = 1, int PageSize = 20, string? SearchTerm = null)
    : IQuery<PagedResult<CompanyResponse>>;

public sealed record CreateCompanyCommand(string Code, string Name, string? TaxCode, string? Address, string? Phone)
    : ICommand<Guid>;

public sealed record UpdateCompanyCommand(
    Guid Id, string Name, string? TaxCode, string? Address, string? Phone, uint? Version) : ICommand;

public sealed record SetCompanyActiveCommand(Guid Id, bool IsActive) : ICommand;

internal sealed class GetCompaniesQueryHandler(IRepositoryBase<Company, Guid> companies)
    : IQueryHandler<GetCompaniesQuery, PagedResult<CompanyResponse>>
{
    public async Task<Result<PagedResult<CompanyResponse>>> Handle(GetCompaniesQuery request, CancellationToken ct)
    {
        var query = companies.FindAll();
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = $"%{request.SearchTerm.Trim()}%";
            query = query.Where(c => EF.Functions.ILike(c.Code, term) || EF.Functions.ILike(c.Name, term));
        }

        return await query.OrderBy(c => c.Code)
            .Select(c => new CompanyResponse(c.Id, c.Code, c.Name, c.TaxCode, c.Address, c.Phone, c.Status, c.Version))
            .ToPagedResultAsync(request.PageIndex, request.PageSize, ct);
    }
}

internal sealed class CreateCompanyCommandHandler(IRepositoryBase<Company, Guid> companies)
    : ICommandHandler<CreateCompanyCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCompanyCommand request, CancellationToken ct)
    {
        var code = request.Code.Trim();
        if (await companies.FindAll(c => c.Code == code).AnyAsync(ct))
            throw new CompanyCodeAlreadyExistsException(code);

        var company = Company.Create(code, request.Name, request.TaxCode, request.Address, request.Phone);
        await companies.AddAsync(company, ct);
        return company.Id;
    }
}

internal sealed class UpdateCompanyCommandHandler(IRepositoryBase<Company, Guid> companies)
    : ICommandHandler<UpdateCompanyCommand>
{
    public async Task<Result> Handle(UpdateCompanyCommand request, CancellationToken ct)
    {
        var company = await companies.FindByIdAsync(request.Id, ct) ?? throw new CompanyNotFoundException(request.Id);
        company.EnsureVersion(request.Version);
        company.Update(request.Name, request.TaxCode, request.Address, request.Phone);
        return Result.Success();
    }
}

internal sealed class SetCompanyActiveCommandHandler(IRepositoryBase<Company, Guid> companies)
    : ICommandHandler<SetCompanyActiveCommand>
{
    public async Task<Result> Handle(SetCompanyActiveCommand request, CancellationToken ct)
    {
        var company = await companies.FindByIdAsync(request.Id, ct) ?? throw new CompanyNotFoundException(request.Id);
        company.SetStatus(request.IsActive ? CompanyStatus.Active : CompanyStatus.Inactive);
        return Result.Success();
    }
}

internal sealed class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Vui lòng nhập mã công ty.").MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().WithMessage("Vui lòng nhập tên công ty.").MaximumLength(255);
        RuleFor(x => x.TaxCode).MaximumLength(20);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.Phone).MaximumLength(20);
    }
}

internal sealed class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
{
    public UpdateCompanyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Vui lòng nhập tên công ty.").MaximumLength(255);
        RuleFor(x => x.TaxCode).MaximumLength(20);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.Phone).MaximumLength(20);
    }
}
