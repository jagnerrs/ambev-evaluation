using Ambev.DeveloperEvaluation.Common.Validation;
using FluentAssertions;
using FluentValidation.Results;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common.Validation;

/// <summary>
/// Unit tests for ValidationErrorDetail.
/// </summary>
public class ValidationErrorDetailTests
{
    [Fact(DisplayName = "Explicit operator converts ValidationFailure to ValidationErrorDetail")]
    public void ExplicitOperator_ValidationFailure_ConvertsToValidationErrorDetail()
    {
        var failure = new ValidationFailure("Property", "Error message") { ErrorCode = "ErrorCode" };

        var result = (ValidationErrorDetail)failure;

        result.Error.Should().Be("ErrorCode");
        result.Detail.Should().Be("Error message");
    }

    [Fact(DisplayName = "Explicit operator handles ValidationFailure with empty ErrorCode")]
    public void ExplicitOperator_ValidationFailureEmptyErrorCode_ConvertsCorrectly()
    {
        var failure = new ValidationFailure("Property", "Detail only");

        var result = (ValidationErrorDetail)failure;

        result.Detail.Should().Be("Detail only");
    }
}
