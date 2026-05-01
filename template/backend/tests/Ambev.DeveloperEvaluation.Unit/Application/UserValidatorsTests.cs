using Ambev.DeveloperEvaluation.Application.Users.DeleteUser;
using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Application.Users.ListUsers;
using Ambev.DeveloperEvaluation.Application.Users.UpdateUser;
using Ambev.DeveloperEvaluation.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class UserValidatorsTests
{
    [Theory(DisplayName = "Given list users paging values When validating Then follows paging rules")]
    [InlineData(1, 1, true)]
    [InlineData(1, 100, true)]
    [InlineData(0, 10, false)]
    [InlineData(1, 101, false)]
    public void ListUsersValidator_ValidatesPaging(int page, int pageSize, bool expectedValid)
    {
        var validator = new ListUsersValidator();

        var result = validator.Validate(new ListUsersQuery(page, pageSize));

        result.IsValid.Should().Be(expectedValid);
    }

    [Fact(DisplayName = "Given empty get user id When validating Then fails")]
    public void GetUserValidator_WithEmptyId_Fails()
    {
        var validator = new GetUserValidator();

        var result = validator.Validate(new GetUserCommand(Guid.Empty));

        result.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "Given empty delete user id When validating Then fails")]
    public void DeleteUserValidator_WithEmptyId_Fails()
    {
        var validator = new DeleteUserValidator();

        var result = validator.Validate(new DeleteUserCommand(Guid.Empty));

        result.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "Given valid update user command When validating Then succeeds")]
    public void UpdateUserValidator_WithValidCommand_Succeeds()
    {
        var validator = new UpdateUserValidator();

        var result = validator.Validate(ValidUpdateUserCommand());

        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Given invalid update user command When validating Then fails")]
    public void UpdateUserValidator_WithInvalidCommand_Fails()
    {
        var command = ValidUpdateUserCommand();
        command.Id = Guid.Empty;
        command.Username = "ab";
        command.Email = "invalid";
        command.Phone = "invalid";
        command.Password = "short";
        command.Role = UserRole.None;
        command.Status = UserStatus.Unknown;
        var validator = new UpdateUserValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.PropertyName)
            .Should().Contain(["Id", "Username", "Email", "Phone", "Password", "Role", "Status"]);
    }

    private static UpdateUserCommand ValidUpdateUserCommand() => new()
    {
        Id = Guid.NewGuid(),
        Username = "valid-user",
        Email = "valid@example.com",
        Phone = "+5511999999999",
        Password = "P@ssw0rd!",
        Role = UserRole.Customer,
        Status = UserStatus.Active
    };
}
