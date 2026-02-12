using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.ListUsers
{
    /// <summary>
    /// Command for listing users with pagination.
    /// </summary>
    public record ListUsersCommand(int Page = 1, int PageSize = 10) : IRequest<ListUsersResult>;
}
