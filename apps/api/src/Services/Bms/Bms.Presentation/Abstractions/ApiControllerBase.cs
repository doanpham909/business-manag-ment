using BuildingBlocks.Shared;
using BuildingBlocks.Shared.Responses;
using BuildingBlocks.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bms.Presentation.Abstractions;

/// <summary>
/// Base controller (tương đương ApiController của QMS).
/// Route khai báo trên từng controller với tiền tố <see cref="AppConstants.ApiV1Prefix"/> ("api/v1").
/// Thành công → 200 + Result envelope; thất bại (Result.Failure) → ErrorResponse cùng shape với global exception handler.
/// </summary>
[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize] // mặc định phải đăng nhập; endpoint công khai gắn [AllowAnonymous]
[Produces("application/json")]
public abstract class ApiControllerBase(ISender sender) : ControllerBase
{
    protected ISender Sender { get; } = sender;

    /// <summary>Gửi request qua MediatR và chuyển Result thành IActionResult.</summary>
    protected async Task<IActionResult> SendAsync<TResponse>(
        IRequest<Result<TResponse>> request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(request, cancellationToken);
        return result.IsSuccess ? Ok(result) : HandleFailure(result);
    }

    /// <summary>Như trên, cho command không trả giá trị (ICommand → Result).</summary>
    protected async Task<IActionResult> SendAsync(
        IRequest<Result> request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(request, cancellationToken);
        return result.IsSuccess ? Ok(result) : HandleFailure(result);
    }

    /// <summary>Map Result thất bại → 400 (validation kèm errors[] hoặc lỗi nghiệp vụ chung).</summary>
    protected IActionResult HandleFailure(Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot handle failure for a successful result.");

        var errors = result is IValidationResult validation
            ? validation.Errors.Select(e => new ErrorDetail(e.Code, e.Message)).ToArray()
            : null;

        var response = new ErrorResponse(
            result.Error.Code,
            errors is null ? "BadRequest" : "ValidationError",
            errors is null ? "Bad Request" : "Validation Error",
            StatusCodes.Status400BadRequest,
            result.Error.Message,
            errors,
            System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier);

        return BadRequest(response);
    }
}
