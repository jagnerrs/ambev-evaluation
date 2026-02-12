namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.ListUsers
{
    /// <summary>
    /// Response for ListUsers operation.
    /// </summary>
    public class ListUsersResponse
    {
        public List<ListUserItemResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
