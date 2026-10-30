using BuildingBlocks.Contracts.Domain.Exceptions;

namespace Bms.Domain.Admin.Users;

public sealed class UserNotFoundException(Guid id) : NotFoundException($"Không tìm thấy người dùng (Id = {id}).");

public sealed class UsernameAlreadyExistsException(string username)
    : ConflictException($"Tên đăng nhập \"{username}\" đã được sử dụng.");

/// <summary>Sai tên đăng nhập hoặc mật khẩu — cùng một thông báo, không lộ tài khoản có tồn tại hay không.</summary>
public sealed class InvalidCredentialsException()
    : UnauthorizedException("Tên đăng nhập hoặc mật khẩu không đúng.");

public sealed class UserLockedException() : UnauthorizedException("Tài khoản đã bị khóa. Vui lòng liên hệ quản trị.");

public sealed class InvalidRefreshTokenException()
    : UnauthorizedException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");

public sealed class CompanyAccessDeniedException()
    : AccessDeniedException("Bạn không được vào công ty này hoặc công ty đã ngừng hoạt động.");

public sealed class AllCompaniesAccessDeniedException()
    : AccessDeniedException("Bạn không có quyền xem chế độ Tất cả công ty.");

public sealed class LastTenantAdminException()
    : ConflictException("Không được khóa hoặc gỡ quyền của quản trị cuối cùng.");
