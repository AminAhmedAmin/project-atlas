using Atlas.Application.Abstractions;
using Atlas.Application.Common;
using Atlas.Domain.Common;

namespace Atlas.Application.Users;

public sealed record CreateUserCommand(string Email, string Password, bool IsAdmin);

public sealed class CreateUserValidator : IValidator<CreateUserCommand>
{
    public IReadOnlyList<Error> Validate(CreateUserCommand instance) => new ValidationErrors()
        .Email(instance.Email, nameof(instance.Email), "E-mail", required: true)
        .Required(instance.Password, nameof(instance.Password), "Password", 100)
        .Errors;
}

public sealed class CreateUserHandler(IUserAdminService users, IValidator<CreateUserCommand> validator)
    : ICommandHandler<CreateUserCommand, string>
{
    public async Task<Result<string>> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        var errors = validator.Validate(command);
        if (errors.Count > 0)
        {
            return Result.Failure<string>(errors);
        }

        var email = EmailAddress.Create(command.Email).Value;
        var created = await users.CreateUserAsync(email, command.Password, cancellationToken);
        if (created.IsFailure || !command.IsAdmin)
        {
            return created;
        }

        var promoted = await users.SetAdminAsync(created.Value, isAdmin: true, cancellationToken);
        return promoted.IsSuccess ? created : Result.Failure<string>(promoted.Errors);
    }
}
