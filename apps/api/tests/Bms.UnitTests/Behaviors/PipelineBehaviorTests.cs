using Bms.UnitTests.Fakes;
using BuildingBlocks.Contracts.Messaging;
using BuildingBlocks.Infrastructure.Behaviors;
using BuildingBlocks.Shared.Results;
using FluentValidation;
using ValidationException = BuildingBlocks.Contracts.Domain.Exceptions.ValidationException;

namespace Bms.UnitTests.Behaviors;

public sealed class PipelineBehaviorTests
{
    private sealed record CreateThingCommand(string Name) : ICommand<Guid>;

    [SkipTransaction]
    private sealed record SkippedCommand : ICommand;

    private sealed record GetThingQuery : IQuery<string>;

    private sealed class CreateThingValidator : AbstractValidator<CreateThingCommand>
    {
        public CreateThingValidator() => RuleFor(x => x.Name).NotEmpty().MaximumLength(5);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Validation_InvalidRequest_ThrowsValidationExceptionWithErrors()
    {
        var behavior = new ValidationPipelineBehavior<CreateThingCommand, Result<Guid>>([new CreateThingValidator()]);
        var handlerCalled = false;

        var exception = await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(
            new CreateThingCommand(""),
            _ => { handlerCalled = true; return Task.FromResult(Result.Success(Guid.Empty)); },
            Ct));

        Assert.False(handlerCalled);
        Assert.Contains(exception.Errors, e => e.PropertyName == nameof(CreateThingCommand.Name));
    }

    [Fact]
    public async Task Validation_ValidRequest_CallsHandler()
    {
        var behavior = new ValidationPipelineBehavior<CreateThingCommand, Result<Guid>>([new CreateThingValidator()]);
        var id = Guid.CreateVersion7();

        var result = await behavior.Handle(new CreateThingCommand("abc"), _ => Task.FromResult(Result.Success(id)), Ct);

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value);
    }

    [Fact]
    public async Task Transaction_SuccessfulCommand_SavesOnce()
    {
        var unitOfWork = new FakeUnitOfWork();
        var behavior = new TransactionPipelineBehavior<CreateThingCommand, Result<Guid>>(unitOfWork);

        await behavior.Handle(new CreateThingCommand("abc"), _ => Task.FromResult(Result.Success(Guid.Empty)), Ct);

        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Transaction_FailedCommand_DoesNotSaveAndDiscardsChanges()
    {
        var unitOfWork = new FakeUnitOfWork();
        var behavior = new TransactionPipelineBehavior<CreateThingCommand, Result<Guid>>(unitOfWork);

        await behavior.Handle(
            new CreateThingCommand("abc"),
            _ => Task.FromResult(Result.Failure<Guid>(new Error("X", "failed"))),
            Ct);

        Assert.Equal(0, unitOfWork.SaveCount);
        // Thay đổi dở dang không được "rò" sang lần SaveChanges sau trong cùng scope
        Assert.Equal(1, unitOfWork.DiscardCount);
    }

    [Fact]
    public async Task Transaction_QueryOrSkippedCommand_DoesNotSave()
    {
        var unitOfWork = new FakeUnitOfWork();

        await new TransactionPipelineBehavior<GetThingQuery, Result<string>>(unitOfWork)
            .Handle(new GetThingQuery(), _ => Task.FromResult(Result.Success("ok")), Ct);
        await new TransactionPipelineBehavior<SkippedCommand, Result>(unitOfWork)
            .Handle(new SkippedCommand(), _ => Task.FromResult(Result.Success()), Ct);

        Assert.Equal(0, unitOfWork.SaveCount);
    }
}
