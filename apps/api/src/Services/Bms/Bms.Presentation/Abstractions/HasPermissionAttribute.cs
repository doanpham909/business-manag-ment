using Microsoft.AspNetCore.Authorization;

namespace Bms.Presentation.Abstractions;

/// <summary>Endpoint yêu cầu một mã quyền ({module}.{resource}.{action}). Backend là nơi kiểm tra quyền thật.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class HasPermissionAttribute(string permission) : AuthorizeAttribute($"perm:{permission}");
