using Ambev.DeveloperEvaluation.Application.Users.DeleteUser;
using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Application.Users.ListUsers;
using Ambev.DeveloperEvaluation.Application.Users.UpdateUser;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class UserHandlersTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();

    [Fact(DisplayName = "Given existing user When getting user Then returns mapped result")]
    public async Task GetUserHandler_WhenUserExists_ReturnsMappedResult()
    {
        var user = CreateUser();
        var expected = new GetUserResult { Id = user.Id, Email = user.Email, Name = user.Username };
        var handler = new GetUserHandler(_userRepository, _mapper);
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _mapper.Map<GetUserResult>(user).Returns(expected);

        var result = await handler.Handle(new GetUserCommand(user.Id), CancellationToken.None);

        result.Should().BeSameAs(expected);
    }

    [Fact(DisplayName = "Given missing user When getting user Then throws")]
    public async Task GetUserHandler_WhenUserDoesNotExist_Throws()
    {
        var id = Guid.NewGuid();
        var handler = new GetUserHandler(_userRepository, _mapper);
        _userRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((User?)null);

        Func<Task> act = () => handler.Handle(new GetUserCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact(DisplayName = "Given users page When listing users Then returns mapped page")]
    public async Task ListUsersHandler_WithPaging_ReturnsMappedResult()
    {
        var users = new[] { CreateUser(), CreateUser() };
        var mapped = users.Select(u => new GetUserResult { Id = u.Id, Email = u.Email }).ToArray();
        var handler = new ListUsersHandler(_userRepository, _mapper);
        _userRepository.GetAllAsync(2, 3, Arg.Any<CancellationToken>()).Returns((users, 8));
        _mapper.Map<IEnumerable<GetUserResult>>(users).Returns(mapped);

        var result = await handler.Handle(new ListUsersQuery(2, 3), CancellationToken.None);

        result.Items.Should().BeEquivalentTo(mapped);
        result.TotalCount.Should().Be(8);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(3);
    }

    [Fact(DisplayName = "Given existing user When updating with password Then hashes password and persists changes")]
    public async Task UpdateUserHandler_WithPassword_UpdatesAndHashesPassword()
    {
        var user = CreateUser();
        var command = new UpdateUserCommand
        {
            Id = user.Id,
            Username = "updated",
            Email = "updated@example.com",
            Phone = "+5511888888888",
            Password = "N3wP@ssword",
            Role = UserRole.Admin,
            Status = UserStatus.Active
        };
        var mapped = new UpdateUserResult { Id = user.Id, Username = command.Username, Email = command.Email };
        var handler = new UpdateUserHandler(_userRepository, _mapper, _passwordHasher);

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.HashPassword(command.Password).Returns("hashed-password");
        _userRepository.UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<User>());
        _mapper.Map<UpdateUserResult>(Arg.Any<User>()).Returns(mapped);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeSameAs(mapped);
        user.Username.Should().Be("updated");
        user.Password.Should().Be("hashed-password");
        user.Role.Should().Be(UserRole.Admin);
        user.UpdatedAt.Should().NotBeNull();
        await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given existing user When updating without password Then keeps existing password")]
    public async Task UpdateUserHandler_WithoutPassword_KeepsPassword()
    {
        var user = CreateUser();
        var originalPassword = user.Password;
        var command = new UpdateUserCommand
        {
            Id = user.Id,
            Username = "updated",
            Email = "updated@example.com",
            Phone = "+5511888888888",
            Password = string.Empty,
            Role = UserRole.Manager,
            Status = UserStatus.Inactive
        };
        var handler = new UpdateUserHandler(_userRepository, _mapper, _passwordHasher);

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _userRepository.UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<User>());
        _mapper.Map<UpdateUserResult>(Arg.Any<User>()).Returns(new UpdateUserResult { Id = user.Id });

        await handler.Handle(command, CancellationToken.None);

        user.Password.Should().Be(originalPassword);
        _passwordHasher.DidNotReceive().HashPassword(Arg.Any<string>());
    }

    [Fact(DisplayName = "Given missing user When updating Then throws")]
    public async Task UpdateUserHandler_WhenUserDoesNotExist_Throws()
    {
        var id = Guid.NewGuid();
        var handler = new UpdateUserHandler(_userRepository, _mapper, _passwordHasher);
        _userRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((User?)null);

        Func<Task> act = () => handler.Handle(new UpdateUserCommand { Id = id }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given existing user When deleting Then returns success")]
    public async Task DeleteUserHandler_WhenUserExists_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        var handler = new DeleteUserHandler(_userRepository);
        _userRepository.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await handler.Handle(new DeleteUserCommand(id), CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact(DisplayName = "Given missing user When deleting Then throws")]
    public async Task DeleteUserHandler_WhenUserDoesNotExist_Throws()
    {
        var id = Guid.NewGuid();
        var handler = new DeleteUserHandler(_userRepository);
        _userRepository.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(false);

        Func<Task> act = () => handler.Handle(new DeleteUserCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    private static User CreateUser() => new()
    {
        Id = Guid.NewGuid(),
        Username = "user",
        Email = "user@example.com",
        Phone = "+5511999999999",
        Password = "existing-hash",
        Role = UserRole.Customer,
        Status = UserStatus.Active
    };
}
