using System.Reflection;
using BuildingBlocks.Contracts.Messaging;
using BuildingBlocks.Contracts.Persistence;
using BuildingBlocks.Shared.Results;
using MediatR;

namespace BuildingBlocks.Infrastructure.Behaviors;

/// <summary>
/// Tự gọi <see cref="IUnitOfWork.SaveChangesAsync"/> MỘT lần sau khi command handler chạy thành công
/// → handler KHÔNG tự gọi SaveChanges (giống quy ước QMS).
/// - Chỉ áp cho request implement ICommand (marker <see cref="IBaseCommand"/>), không dựa vào hậu tố tên.
/// - Bỏ qua khi command có [SkipTransaction] hoặc Result trả về thất bại (không lưu thay đổi dở dang).
/// - Một lần SaveChanges đã là một transaction DB (atomic) nên không cần TransactionScope như QMS.
/// </summary>
public sealed class TransactionPipelineBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IBaseCommand || HasSkipTransaction())
            return await next(cancellationToken);

        var response = await next(cancellationToken);

        if (response is Result { IsFailure: true })
        {
            unitOfWork.DiscardChanges();
            return response;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return response;
    }

    private static bool HasSkipTransaction() =>
        typeof(TRequest).GetCustomAttribute<SkipTransactionAttribute>() is not null;
}
