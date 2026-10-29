using BuildingBlocks.Contracts.Domain.DDD;
using BuildingBlocks.Shared.Results;
using MediatR;

namespace BuildingBlocks.Contracts.Messaging;

// CQRS contracts (giống QMS). Command = ghi (TransactionPipelineBehavior tự SaveChanges),
// Query = đọc (không SaveChanges, dùng FindAll/AsNoTracking).

/// <summary>Marker chung cho mọi command — behavior dựa vào đây thay vì hậu tố tên "Command".</summary>
public interface IBaseCommand;

/// <summary>
/// Gắn lên command để TransactionPipelineBehavior KHÔNG tự SaveChanges
/// (handler tự kiểm soát lưu — chỉ dùng khi thật cần, ví dụ batch tách từng phần).
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SkipTransactionAttribute : Attribute;

public interface ICommand : IRequest<Result>, IBaseCommand;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;

public interface IDomainEventHandler<in TEvent> : INotificationHandler<TEvent>
    where TEvent : IDomainEvent;
