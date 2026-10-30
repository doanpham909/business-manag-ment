using Bms.Domain.Hrm.Departments;
using Bms.Domain.Projects;
using BuildingBlocks.Contracts.Domain.DDD;

namespace Bms.UnitTests.Admin;

public sealed class OrganizationDomainTests
{
    private static ProjectInfo Info(string name = "Dự án", DateOnly? start = null, DateOnly? end = null) =>
        new(name, null, "DAUTU", "DANDUNG", "Hà Nội", null, "Người A", start, end, "Mô tả");

    [Fact]
    public void Department_MoveUnderItsDescendant_ThrowsBusinessRuleException()
    {
        var root = Department.Create("A", "A", null, null, 0);
        var child = Department.Create("B", "B", root, null, 0);

        Assert.Throws<BusinessRuleValidationException>(() => root.MoveTo(child));
    }

    [Fact]
    public void Department_Path_ContainsAncestors()
    {
        var root = Department.Create("A", "A", null, null, 0);
        var child = Department.Create("B", "B", root, null, 0);

        Assert.Equal($"/{root.Id}/{child.Id}/", child.Path);
    }

    [Fact]
    public void Project_WhenEndBeforeStart_ThrowsBusinessRuleException() =>
        Assert.Throws<BusinessRuleValidationException>(() =>
            Project.Create("P", Info(start: new DateOnly(2026, 2, 1), end: new DateOnly(2026, 1, 1))));

    [Fact]
    public void Project_Copy_KeepsAllInfoExceptCode_AndStartsInProgress()
    {
        var source = Project.Create("P1", Info());
        source.ChangeStatus(ProjectStatus.Completed);

        var copy = source.Copy("P2");

        Assert.Equal(("P2", ProjectStatus.InProgress), (copy.Code, copy.Status));
        Assert.Equal((source.Name, source.Location, source.ManagerName, source.Description),
            (copy.Name, copy.Location, copy.ManagerName, copy.Description));
        Assert.NotEqual(source.Id, copy.Id);
    }

    [Theory]
    [InlineData(ProjectStatus.InProgress, true)]
    [InlineData(ProjectStatus.Completed, false)]
    [InlineData(ProjectStatus.Closed, false)]
    public void Project_AcceptsNewDocuments_OnlyWhenInProgress(ProjectStatus status, bool expected)
    {
        var project = Project.Create("P", Info());
        if (status != ProjectStatus.InProgress)
            project.ChangeStatus(ProjectStatus.Completed);
        if (status == ProjectStatus.Closed)
            project.ChangeStatus(ProjectStatus.Closed);

        Assert.Equal(expected, project.AcceptsNewDocuments);
    }

    [Fact]
    public void Project_CannotJumpFromInProgressToClosed() =>
        Assert.Throws<BusinessRuleValidationException>(() =>
            Project.Create("P", Info()).ChangeStatus(ProjectStatus.Closed));
}
