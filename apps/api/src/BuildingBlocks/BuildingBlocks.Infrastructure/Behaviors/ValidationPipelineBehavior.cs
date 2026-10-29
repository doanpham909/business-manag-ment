using FluentValidation;
using MediatR;
using ValidationError = BuildingBlocks.Contracts.Domain.Exceptions.ValidationError;
using ValidationException = BuildingBlocks.Contracts.Domain.Exceptions.ValidationException;

namespace BuildingBlocks.Infrastructure.Behaviors;

/// <summary>
/// Chạy toàn bộ FluentValidation validator của request trước handler.
/// Có lỗi → ném <see cref="ValidationException"/> (GlobalExceptionHandler trả HTTP 400 + errors[]).
/// Khác QMS (trả ValidationResult qua reflection): ném exception đơn giản hơn và áp dụng cho mọi kiểu response.
/// </summary>
public sealed class ValidationPipelineBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        // Chạy tuần tự: validator có thể dùng chung DbContext (không thread-safe) khi check DB
        var errors = new List<ValidationError>();
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            errors.AddRange(result.Errors
                .Where(failure => failure is not null)
                .Select(failure => new ValidationError(failure.PropertyName, failure.ErrorMessage)));
        }

        if (errors.Count > 0)
            throw new ValidationException(errors.Distinct().ToArray());

        return await next(cancellationToken);
    }
}
