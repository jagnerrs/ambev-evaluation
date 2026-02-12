using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Application.Users.ListUsers
{
    /// <summary>
    /// Result of ListUsers operation.
    /// </summary>
    public class ListUsersResult
    {
        public List<ListUserItemResult> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    }

    /// <summary>
    /// Summary item in ListUsers result.
    /// </summary>
    public class ListUserItemResult
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserStatus Status { get; set; }
        public UserRole Role { get; set; }
    }
}
