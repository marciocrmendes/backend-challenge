using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Integration.TestData;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Repositories;

public class UserRepositoryTests
{
    [Fact(DisplayName = "Given user When creating Then can retrieve by id and email")]
    public async Task CreateAsync_WhenUserIsValid_PersistsAndRetrievesUser()
    {
        await using var context = InMemoryContextFactory.CreateContext();
        var repository = new UserRepository(context);
        var user = CreateUser("ana@example.com", "ana", DateTime.UtcNow);

        var created = await repository.CreateAsync(user);
        var byId = await repository.GetByIdAsync(created.Id);
        var byEmail = await repository.GetByEmailAsync("ana@example.com");

        created.Id.Should().NotBeEmpty();
        byId.Should().NotBeNull();
        byId!.Email.Should().Be("ana@example.com");
        byEmail.Should().NotBeNull();
        byEmail!.Id.Should().Be(created.Id);
    }

    [Fact(DisplayName = "Given users When listing page Then returns newest users and total count")]
    public async Task GetAllAsync_WithPaging_ReturnsOrderedPageAndTotalCount()
    {
        await using var context = InMemoryContextFactory.CreateContext();
        var repository = new UserRepository(context);
        var oldest = CreateUser("old@example.com", "old", DateTime.UtcNow.AddDays(-3));
        var middle = CreateUser("middle@example.com", "middle", DateTime.UtcNow.AddDays(-2));
        var newest = CreateUser("new@example.com", "new", DateTime.UtcNow.AddDays(-1));
        await repository.CreateAsync(oldest);
        await repository.CreateAsync(middle);
        await repository.CreateAsync(newest);

        var (items, totalCount) = await repository.GetAllAsync(page: 1, pageSize: 2);

        totalCount.Should().Be(3);
        items.Select(u => u.Email).Should().ContainInOrder("new@example.com", "middle@example.com");
    }

    [Fact(DisplayName = "Given existing user When updating Then persists changes")]
    public async Task UpdateAsync_WhenUserExists_PersistsChanges()
    {
        var databaseName = Guid.NewGuid().ToString();
        var options = InMemoryContextFactory.CreateOptions(databaseName);
        Guid userId;

        await using (var context = InMemoryContextFactory.CreateContext(options))
        {
            var repository = new UserRepository(context);
            var user = await repository.CreateAsync(CreateUser("before@example.com", "before", DateTime.UtcNow));
            userId = user.Id;
            user.Email = "after@example.com";
            user.Activate();
            await repository.UpdateAsync(user);
        }

        await using (var context = InMemoryContextFactory.CreateContext(options))
        {
            var repository = new UserRepository(context);
            var stored = await repository.GetByIdAsync(userId);

            stored.Should().NotBeNull();
            stored!.Email.Should().Be("after@example.com");
            stored.Status.Should().Be(UserStatus.Active);
            stored.UpdatedAt.Should().NotBeNull();
        }
    }

    [Fact(DisplayName = "Given user id When deleting Then removes user and reports missing deletes")]
    public async Task DeleteAsync_RemovesExistingUserAndReturnsFalseForMissing()
    {
        await using var context = InMemoryContextFactory.CreateContext();
        var repository = new UserRepository(context);
        var user = await repository.CreateAsync(CreateUser("delete@example.com", "delete", DateTime.UtcNow));

        var deleted = await repository.DeleteAsync(user.Id);
        var missingDelete = await repository.DeleteAsync(user.Id);
        var stored = await repository.GetByIdAsync(user.Id);

        deleted.Should().BeTrue();
        missingDelete.Should().BeFalse();
        stored.Should().BeNull();
    }

    private static User CreateUser(string email, string username, DateTime createdAt) => new()
    {
        Id = Guid.NewGuid(),
        Email = email,
        Username = username,
        Password = "P@ssw0rd!",
        Phone = "+5511999999999",
        Status = UserStatus.Active,
        Role = UserRole.Customer,
        CreatedAt = createdAt
    };
}
