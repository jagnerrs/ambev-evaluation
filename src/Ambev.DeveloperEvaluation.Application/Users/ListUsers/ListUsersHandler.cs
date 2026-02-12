using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.ListUsers
{
    /// <summary>
    /// Handler for ListUsersCommand.
    /// </summary>
    public class ListUsersHandler : IRequestHandler<ListUsersCommand, ListUsersResult>
    {
        private readonly IUserRepository _userRepository;

        public ListUsersHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ListUsersResult> Handle(ListUsersCommand request, CancellationToken cancellationToken)
        {
            var validator = new ListUsersCommandValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var (items, totalCount) = await _userRepository.GetAllAsync(request.Page, request.PageSize, cancellationToken);

            return new ListUsersResult
            {
                Items = items.Select(s => new ListUserItemResult
                {
                    Id = s.Id,
                    Username = s.Username,
                    Email = s.Email,
                    Phone = s.Phone,
                    Role = s.Role,
                    Status = s.Status
                }).ToList(),
                TotalCount = totalCount,
                CurrentPage = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
