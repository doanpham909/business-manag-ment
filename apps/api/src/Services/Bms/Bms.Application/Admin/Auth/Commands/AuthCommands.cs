using BuildingBlocks.Contracts.Messaging;
using FluentValidation;

namespace Bms.Application.Admin.Auth.Commands;

/// <summary>Đăng nhập bằng tên đăng nhập + mật khẩu (không nhập mã công ty — ADR-0002).</summary>
public sealed record LoginCommand(string Username, string Password) : ICommand<AuthSession>;

/// <summary>Chọn / đổi công ty đang làm việc, hoặc vào chế độ Tất cả công ty (chỉ đọc).</summary>
public sealed record SelectCompanyCommand(Guid? CompanyId, bool AllCompanies, string? CurrentRefreshToken)
    : ICommand<AuthSession>;

/// <summary>Làm mới phiên bằng refresh token (xoay vòng: token cũ bị thu hồi).</summary>
public sealed record RefreshSessionCommand(string RefreshToken) : ICommand<AuthSession>;

public sealed record LogoutCommand(string? RefreshToken) : ICommand;

internal sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Vui lòng nhập tên đăng nhập.").MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().WithMessage("Vui lòng nhập mật khẩu.").MaximumLength(200);
    }
}

internal sealed class SelectCompanyCommandValidator : AbstractValidator<SelectCompanyCommand>
{
    public SelectCompanyCommandValidator() =>
        RuleFor(x => x).Must(x => x.AllCompanies || x.CompanyId.HasValue)
            .WithName("CompanyId").WithMessage("Vui lòng chọn công ty.");
}
