using AwesomeAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Infrastructure.Behaviors;

namespace Tests.Infrastructure.Behaviors
{
    [TestFixture]
    public class ValidationBehaviorTests
    {
        private ValidationBehavior<TestRequest, TestResponse> _validationBehavior;
        private IValidator<TestRequest> _validator1;
        private IValidator<TestRequest> _validator2;
        private List<IValidator<TestRequest>> _validators;
        private RequestHandlerDelegate<TestResponse> _next;
        private TestRequest _request;
        private TestResponse _response;
        private CancellationToken _cancellationToken;

        [SetUp]
        public void SetUp()
        {
            _validator1 = Substitute.For<IValidator<TestRequest>>();
            _validator2 = Substitute.For<IValidator<TestRequest>>();
            _validators = new List<IValidator<TestRequest>> { _validator1, _validator2 };
            _next = Substitute.For<RequestHandlerDelegate<TestResponse>>();
            _request = new TestRequest { Name = "Test" };
            _response = new TestResponse { Success = true };
            _cancellationToken = new CancellationToken();

            _validationBehavior = new ValidationBehavior<TestRequest, TestResponse>(_validators);
        }

        [Test]
        public void Constructor_WithValidators_InitializesCorrectly()
        {
            // Arrange & Act
            var behavior = new ValidationBehavior<TestRequest, TestResponse>(_validators);

            // Assert
            behavior.Should().NotBeNull();
        }

        [Test]
        public void Constructor_WithNullValidators_InitializesCorrectly()
        {
            // Arrange & Act
            var behavior = new ValidationBehavior<TestRequest, TestResponse>(null);

            // Assert
            behavior.Should().NotBeNull();
        }

        [Test]
        public void Constructor_WithEmptyValidators_InitializesCorrectly()
        {
            // Arrange & Act
            var behavior = new ValidationBehavior<TestRequest, TestResponse>(new List<IValidator<TestRequest>>());

            // Assert
            behavior.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_WithNoValidators_CallsNextAndReturnsResponse()
        {
            // Arrange
            var emptyValidators = new List<IValidator<TestRequest>>();
            var behavior = new ValidationBehavior<TestRequest, TestResponse>(emptyValidators);
            _next().Returns(_response);

            // Act
            var result = await behavior.Handle(_request, _next, _cancellationToken);

            // Assert
            result.Should().Be(_response);
            await _next.Received(1)();
        }

        [Test]
        public async Task Handle_WithValidRequest_CallsNextAndReturnsResponse()
        {
            // Arrange
            _validator1.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Returns(new ValidationResult());
            _validator2.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Returns(new ValidationResult());
            _next().Returns(_response);

            // Act
            var result = await _validationBehavior.Handle(_request, _next, _cancellationToken);

            // Assert
            result.Should().Be(_response);
            await _next.Received(1)();
        }

        [Test]
        public async Task Handle_WithValidRequest_CallsAllValidators()
        {
            // Arrange
            _validator1.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Returns(new ValidationResult());
            _validator2.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Returns(new ValidationResult());
            _next().Returns(_response);

            // Act
            await _validationBehavior.Handle(_request, _next, _cancellationToken);

            // Assert
            await _validator1.Received(1).ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken);
            await _validator2.Received(1).ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken);
        }

        [Test]
        public void Handle_WithInvalidRequest_ThrowsValidationException()
        {
            // Arrange
            var validationFailure = new ValidationFailure("Name", "Name is required");
            var validationResult = new ValidationResult(new[] { validationFailure });
            
            _validator1.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Returns(validationResult);
            _validator2.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Returns(new ValidationResult());

            // Act & Assert
            var exception = Assert.ThrowsAsync<ValidationException>(() =>
                _validationBehavior.Handle(_request, _next, _cancellationToken));

            exception.Errors.Should().HaveCount(1);
            exception.Errors.First().PropertyName.Should().Be("Name");
            exception.Errors.First().ErrorMessage.Should().Be("Name is required");
        }

        [Test]
        public void Handle_WithMultipleValidationFailures_ThrowsValidationExceptionWithAllErrors()
        {
            // Arrange
            var validationFailure1 = new ValidationFailure("Name", "Name is required");
            var validationFailure2 = new ValidationFailure("Age", "Age must be positive");
            var validationResult1 = new ValidationResult(new[] { validationFailure1 });
            var validationResult2 = new ValidationResult(new[] { validationFailure2 });
            
            _validator1.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Returns(validationResult1);
            _validator2.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Returns(validationResult2);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ValidationException>(() =>
                _validationBehavior.Handle(_request, _next, _cancellationToken));

            exception.Errors.Should().HaveCount(2);
            exception.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage == "Name is required");
            exception.Errors.Should().Contain(e => e.PropertyName == "Age" && e.ErrorMessage == "Age must be positive");
        }

        [Test]
        public async Task Handle_WithValidationFailure_DoesNotCallNext()
        {
            // Arrange
            var validationFailure = new ValidationFailure("Name", "Name is required");
            var validationResult = new ValidationResult(new[] { validationFailure });
            
            _validator1.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Returns(validationResult);
            _validator2.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Returns(new ValidationResult());

            // Act & Assert
            Assert.ThrowsAsync<ValidationException>(() =>
                _validationBehavior.Handle(_request, _next, _cancellationToken));

            await _next.DidNotReceive()();
        }

        [Test]
        public void Handle_WithNullValidationFailure_IgnoresNullFailures()
        {
            // Arrange
            var validationFailure = new ValidationFailure("Name", "Name is required");
            var validationResult = new ValidationResult(new[] { validationFailure, null });
            
            _validator1.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Returns(validationResult);
            _validator2.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Returns(new ValidationResult());

            // Act & Assert
            var exception = Assert.ThrowsAsync<ValidationException>(() =>
                _validationBehavior.Handle(_request, _next, _cancellationToken));

            exception.Errors.Should().HaveCount(1);
            exception.Errors.First().PropertyName.Should().Be("Name");
        }

        [Test]
        public async Task Handle_WithSingleValidator_WorksCorrectly()
        {
            // Arrange
            var singleValidator = new List<IValidator<TestRequest>> { _validator1 };
            var behavior = new ValidationBehavior<TestRequest, TestResponse>(singleValidator);
            
            _validator1.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Returns(new ValidationResult());
            _next().Returns(_response);

            // Act
            var result = await behavior.Handle(_request, _next, _cancellationToken);

            // Assert
            result.Should().Be(_response);
            await _validator1.Received(1).ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken);
            await _next.Received(1)();
        }

        [Test]
        public async Task Handle_WithValidationContextPassedCorrectly()
        {
            // Arrange
            ValidationContext<TestRequest> capturedContext = null;
            _validator1.ValidateAsync(Arg.Do<ValidationContext<TestRequest>>(ctx => capturedContext = ctx), _cancellationToken)
                .Returns(new ValidationResult());
            _validator2.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Returns(new ValidationResult());
            _next().Returns(_response);

            // Act
            await _validationBehavior.Handle(_request, _next, _cancellationToken);

            // Assert
            capturedContext.Should().NotBeNull();
            capturedContext.InstanceToValidate.Should().Be(_request);
        }

        [Test]
        public async Task Handle_WithCancellationToken_PassesTokenToValidators()
        {
            // Arrange
            var customCancellationToken = new CancellationToken(true);
            _validator1.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), customCancellationToken)
                .Returns(new ValidationResult());
            _validator2.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), customCancellationToken)
                .Returns(new ValidationResult());
            _next().Returns(_response);

            // Act
            await _validationBehavior.Handle(_request, _next, customCancellationToken);

            // Assert
            await _validator1.Received(1).ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), customCancellationToken);
            await _validator2.Received(1).ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), customCancellationToken);
        }

        [Test]
        public async Task Handle_WithValidatorException_ThrowsOriginalException()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Validator error");
            _validator1.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Throws(expectedException);
            _validator2.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Returns(new ValidationResult());

            // Act & Assert
            var thrownException = Assert.ThrowsAsync<InvalidOperationException>(() =>
                _validationBehavior.Handle(_request, _next, _cancellationToken));

            thrownException.Should().Be(expectedException);
            await _next.DidNotReceive()();
        }

        [Test]
        public async Task Handle_WithAsyncValidation_WaitsForAllValidators()
        {
            // Arrange
            var tcs1 = new TaskCompletionSource<ValidationResult>();
            var tcs2 = new TaskCompletionSource<ValidationResult>();
            
            _validator1.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Returns(tcs1.Task);
            _validator2.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Returns(tcs2.Task);
            _next().Returns(_response);

            // Act
            var handleTask = _validationBehavior.Handle(_request, _next, _cancellationToken);

            // Complete validations
            tcs1.SetResult(new ValidationResult());
            tcs2.SetResult(new ValidationResult());

            var result = await handleTask;

            // Assert
            result.Should().Be(_response);
            await _next.Received(1)();
        }

        [Test]
        public void Handle_WithMixedValidationResults_OnlyThrowsForFailures()
        {
            // Arrange
            var validationFailure = new ValidationFailure("Name", "Name is required");
            var validationResult1 = new ValidationResult(new[] { validationFailure });
            var validationResult2 = new ValidationResult(); // No failures
            
            _validator1.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Returns(validationResult1);
            _validator2.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Returns(validationResult2);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ValidationException>(() =>
                _validationBehavior.Handle(_request, _next, _cancellationToken));

            exception.Errors.Should().HaveCount(1);
            exception.Errors.First().PropertyName.Should().Be("Name");
        }

        [Test]
        public async Task Handle_WithNullRequest_StillCallsValidators()
        {
            // Arrange
            TestRequest nullRequest = null;
            _validator1.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Returns(new ValidationResult());
            _validator2.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken)
                .Returns(new ValidationResult());
            _next().Returns(_response);

            // Act
            var result = await _validationBehavior.Handle(nullRequest, _next, _cancellationToken);

            // Assert
            result.Should().Be(_response);
            await _validator1.Received(1).ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken);
            await _validator2.Received(1).ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), _cancellationToken);
        }

        // Test helper classes
        public class TestRequest : IRequest<TestResponse>
        {
            public string Name { get; set; }
        }

        public class TestResponse
        {
            public bool Success { get; set; }
        }
    }
} 