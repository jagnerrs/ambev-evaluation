using Ambev.DeveloperEvaluation.Common.Validation;
using FluentAssertions;
using FluentValidation.Results;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common.Validation;

/// <summary>
/// Unit tests for ValidationResultDetail.
/// </summary>
public class ValidationResultDetailTests
{
    [Fact(DisplayName = "Constructor with valid ValidationResult sets IsValid true")]
    public void Constructor_ValidResult_SetsIsValidTrue()
    {
        var validationResult = new ValidationResult();

        var result = new ValidationResultDetail(validationResult);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Constructor with invalid ValidationResult sets IsValid false and maps errors")]
    public void Constructor_InvalidResult_SetsIsValidFalseAndMapsErrors()
    {
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Property", "Error message") { ErrorCode = "Code" }
        });

        var result = new ValidationResultDetail(validationResult);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Error.Should().Be("Code");
        result.Errors.First().Detail.Should().Be("Error message");
    }

    [Fact(DisplayName = "Parameterless constructor creates empty result")]
    public void ParameterlessConstructor_CreatesEmptyResult()
    {
        var result = new ValidationResultDetail();

        result.Errors.Should().BeEmpty();
    }
}
