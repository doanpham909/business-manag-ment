using Bms.Application.Common.Paging;
using Bms.Domain.Admin.Catalogs;
using BuildingBlocks.Contracts.Messaging;
using BuildingBlocks.Contracts.Persistence;
using BuildingBlocks.Shared.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Bms.Application.Admin.Catalogs;

public sealed record CatalogTypeResponse(string Code, string Name);

public sealed record CatalogItemResponse(
    Guid Id, string Type, string Code, string Name, string? Description, int SortOrder, bool IsActive, uint Version);

public sealed record GetCatalogTypesQuery : IQuery<IReadOnlyList<CatalogTypeResponse>>;

public sealed record GetCatalogItemsQuery(
    string Type, int PageIndex = 1, int PageSize = 20, string? SearchTerm = null, bool ActiveOnly = false)
    : IQuery<PagedResult<CatalogItemResponse>>;

public sealed record CreateCatalogItemCommand(string Type, string Code, string Name, string? Description, int SortOrder)
    : ICommand<Guid>;

public sealed record UpdateCatalogItemCommand(Guid Id, string Name, string? Description, int SortOrder, uint? Version)
    : ICommand;

public sealed record SetCatalogItemActiveCommand(Guid Id, bool IsActive) : ICommand;

internal sealed class CatalogQueryHandlers(IRepositoryBase<CatalogItem, Guid> items)
    : IQueryHandler<GetCatalogTypesQuery, IReadOnlyList<CatalogTypeResponse>>,
      IQueryHandler<GetCatalogItemsQuery, PagedResult<CatalogItemResponse>>
{
    public Task<Result<IReadOnlyList<CatalogTypeResponse>>> Handle(GetCatalogTypesQuery request, CancellationToken ct)
    {
        IReadOnlyList<CatalogTypeResponse> types =
            CatalogTypes.Names.Select(t => new CatalogTypeResponse(t.Key, t.Value)).ToList();
        return Task.FromResult<Result<IReadOnlyList<CatalogTypeResponse>>>(Result.Success(types));
    }

    public async Task<Result<PagedResult<CatalogItemResponse>>> Handle(GetCatalogItemsQuery request, CancellationToken ct)
    {
        var query = items.FindAll(i => i.Type == request.Type);
        if (request.ActiveOnly)
            query = query.Where(i => i.IsActive);
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = $"%{request.SearchTerm.Trim()}%";
            query = query.Where(i => EF.Functions.ILike(i.Code, term) || EF.Functions.ILike(i.Name, term));
        }

        return await query.OrderBy(i => i.SortOrder).ThenBy(i => i.Name)
            .Select(i => new CatalogItemResponse(i.Id, i.Type, i.Code, i.Name, i.Description, i.SortOrder, i.IsActive, i.Version))
            .ToPagedResultAsync(request.PageIndex, request.PageSize, ct);
    }
}

internal sealed class CreateCatalogItemCommandHandler(IRepositoryBase<CatalogItem, Guid> items)
    : ICommandHandler<CreateCatalogItemCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCatalogItemCommand request, CancellationToken ct)
    {
        if (!CatalogTypes.Names.ContainsKey(request.Type))
            throw new CatalogTypeInvalidException(request.Type);

        var code = request.Code.Trim();
        if (await items.FindAll(i => i.Type == request.Type && i.Code == code).AnyAsync(ct))
            throw new CatalogCodeAlreadyExistsException(code);

        var item = CatalogItem.Create(request.Type, code, request.Name, request.Description, request.SortOrder);
        await items.AddAsync(item, ct);
        return item.Id;
    }
}

internal sealed class UpdateCatalogItemCommandHandler(IRepositoryBase<CatalogItem, Guid> items)
    : ICommandHandler<UpdateCatalogItemCommand>
{
    public async Task<Result> Handle(UpdateCatalogItemCommand request, CancellationToken ct)
    {
        var item = await items.FindByIdAsync(request.Id, ct) ?? throw new CatalogItemNotFoundException(request.Id);
        item.EnsureVersion(request.Version);
        item.Update(request.Name, request.Description, request.SortOrder);
        return Result.Success();
    }
}

internal sealed class SetCatalogItemActiveCommandHandler(IRepositoryBase<CatalogItem, Guid> items)
    : ICommandHandler<SetCatalogItemActiveCommand>
{
    public async Task<Result> Handle(SetCatalogItemActiveCommand request, CancellationToken ct)
    {
        var item = await items.FindByIdAsync(request.Id, ct) ?? throw new CatalogItemNotFoundException(request.Id);
        item.SetActive(request.IsActive);
        return Result.Success();
    }
}

internal sealed class CreateCatalogItemCommandValidator : AbstractValidator<CreateCatalogItemCommand>
{
    public CreateCatalogItemCommandValidator()
    {
        RuleFor(x => x.Type).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Code).NotEmpty().WithMessage("Vui lòng nhập mã.").MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().WithMessage("Vui lòng nhập tên.").MaximumLength(255);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

internal sealed class UpdateCatalogItemCommandValidator : AbstractValidator<UpdateCatalogItemCommand>
{
    public UpdateCatalogItemCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Vui lòng nhập tên.").MaximumLength(255);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
