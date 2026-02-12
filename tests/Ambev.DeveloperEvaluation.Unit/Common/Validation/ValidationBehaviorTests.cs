using Ambev.DeveloperEvaluation.Common.Validation;
using FluentAssertions;
using FluentValidation;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common.Validation;

/// <summary>
/// Unit tests for ValidationBehavior.
/// </summary>
public class ValidationBehaviorTests
{
    [Fact(DisplayName = "Handle passes through when no validators and request is valid")]
    public async Task Handle_NoValidators_CallsNext()
    {
        var validators = Array.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);
        var request = new TestRequest("valid");
        var expectedResponse = new TestResponse();
        var next = Substitute.For<RequestHandlerDelegate<TestResponse>>();
        next().Returns(expectedResponse);

        var result = await behavior.Handle(request, next, CancellationToken.None);

        result.Should().Be(expectedResponse);
        await next.Received(1)();
    }

    [Fact(DisplayName = "Handle passes through when validation succeeds")]
    public async Task Handle_ValidationSucceeds_CallsNext()
    {
        var validator = new TestRequestValidator();
        var validators = new IValidator<TestRequest>[] { validator };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);
        var request = new TestRequest("valid");
        var expectedResponse = new TestResponse();
        var next = Substitute.For<RequestHandlerDelegate<TestResponse>>();
        next().Returns(expectedResponse);

        var result = await behavior.Handle(request, next, CancellationToken.None);

        result.Should().Be(expectedResponse);
        await next.Received(1)();
    }

    [Fact(DisplayName = "Handle throws ValidationException when validation fails")]
    public async Task Handle_ValidationFails_ThrowsValidationException()
    {
        var validator = new TestRequestValidator();
        var validators = new IValidator<TestRequest>[] { validator };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);
        var request = new TestRequest("");
        var next = Substitute.For<RequestHandlerDelegate<TestResponse>>();

        var act = () => behavior.Handle(request, next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await next.DidNotReceive()();
    }

    public sealed record TestRequest(string Value = "") : IRequest<TestResponse>;

    public sealed record TestResponse;

    private class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Value).NotEmpty().WithMessage("Value is required");
        }
    }
}
