using BuildingBlocks.Shared;

namespace BuildingBlocks.Contracts.Domain.Exceptions;

/// <summary>
/// Lỗi validate input (do ValidationPipelineBehavior ném từ FluentValidation) → HTTP 400.
/// </summary>
public sealed class ValidationException(IReadOnlyCollection<ValidationError> errors)
    : DomainException("Validation Failure", "One or more validation errors occurred.")
{
    public IReadOnlyCollection<ValidationError> Errors { get; } = errors;

    public override string? Code => ErrorCodes.Validation;
}

/// <summary>Một lỗi validate theo field.</summary>
public sealed record ValidationError(string PropertyName, string ErrorMessage);
