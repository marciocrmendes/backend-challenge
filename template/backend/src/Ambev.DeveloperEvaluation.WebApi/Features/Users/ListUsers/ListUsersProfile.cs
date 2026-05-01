using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Users.ListUsers;
using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.GetUser;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.ListUsers;

public class ListUsersProfile : Profile
{
    public ListUsersProfile()
    {
        CreateMap<ListUsersRequest, ListUsersQuery>()
            .ConstructUsing(src => new ListUsersQuery(src.Page, src.PageSize));

        CreateMap<ListUsersResult, ListUsersResponse>();
        CreateMap<GetUserResult, GetUserResponse>();
    }
}
