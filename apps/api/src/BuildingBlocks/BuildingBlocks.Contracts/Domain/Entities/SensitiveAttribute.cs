namespace BuildingBlocks.Contracts.Domain.Entities;

/// <summary>Đánh dấu thuộc tính nhạy cảm: nhật ký thay đổi chỉ ghi "***", không ghi giá trị.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SensitiveAttribute : Attribute;
