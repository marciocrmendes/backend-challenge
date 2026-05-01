using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.ListUsers;

public record ListUsersQuery(int Page, int PageSize) : IRequest<ListUsersResult>;
