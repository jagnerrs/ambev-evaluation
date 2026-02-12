using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Users.ListUsers
{
    /// <summary>
    /// Validator for ListUsersCommand.
    /// </summary>
    public class ListUsersCommandValidator : AbstractValidator<ListUsersCommand>
    {
        public ListUsersCommandValidator()
        {
            RuleFor(x => x.Page).GreaterThan(0);
            RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        }
    }
}
