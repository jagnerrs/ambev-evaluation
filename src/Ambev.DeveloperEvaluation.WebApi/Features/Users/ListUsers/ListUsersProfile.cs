using Ambev.DeveloperEvaluation.Application.Users.ListUsers;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.ListUsers
{
    /// <summary>
    /// AutoMapper profile for ListUsers.
    /// </summary>
    public class ListUsersProfile : Profile
    {
        public ListUsersProfile()
        {
            CreateMap<ListUserItemResult, ListUserItemResponse>();
        }
    }
}
