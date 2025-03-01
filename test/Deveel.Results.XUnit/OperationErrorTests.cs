using System.ComponentModel.DataAnnotations;

namespace Deveel
{
    public static class OperationErrorTests
    {
        [Fact]
        public static void OperationError_NoDetails()
        {
            var error = new OperationError("err.1", "biz");
            Assert.Equal("err.1", error.Code);
            Assert.Equal("biz", error.Domain);
            Assert.Null(error.Message);
            Assert.Null(error.InnerError);
        }

        [Fact]
        public static void OperationError_WithMessage()
        {
            var error = new OperationError("err.1", "biz", "An error occurred");
            Assert.Equal("err.1", error.Code);
            Assert.Equal("biz", error.Domain);
            Assert.Equal("An error occurred", error.Message);
            Assert.Null(error.InnerError);
        }

        [Fact]
        public static void OperationError_WithInner()
        {
            var inner = new OperationError("err.2", "biz");
            var error = new OperationError("err.1", "biz", innerError: inner);
            Assert.Equal("err.1", error.Code);
            Assert.Equal("biz", error.Domain);
            Assert.Null(error.Message);
            Assert.NotNull(error.InnerError);
            Assert.Equal("err.2", error.InnerError.Code);
            Assert.Equal("biz", error.InnerError.Domain);
        }

        [Fact]
        public static void OperationError_WithMessageAndInner()
        {
            var inner = new OperationError("err.2", "biz");
            var error = new OperationError("err.1", "biz", "An error occurred", innerError: inner);
            Assert.Equal("err.1", error.Code);
            Assert.Equal("biz", error.Domain);
            Assert.Equal("An error occurred", error.Message);
            Assert.NotNull(error.InnerError);
            Assert.Equal("err.2", error.InnerError.Code);
            Assert.Equal("biz", error.InnerError.Domain);
        }

        [Fact]
        public static void OperationError_WithNullCode()
        {
            Assert.Throws<ArgumentNullException>(() => new OperationError(null, "biz"));
        }

        [Fact]
        public static void OperationError_WithNullDomain()
        {
            Assert.Throws<ArgumentNullException>(() => new OperationError("err.1", null));
        }

        [Fact]
        public static void OperationException_NoDetails()
        {
            var exception = new OperationException("err.1", "biz", "An error occurred");
            Assert.Equal("err.1", exception.ErrorCode);
            Assert.Equal("biz", exception.ErrorDomain);
            Assert.Equal("An error occurred", exception.Message);
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public static void OperationException_WithMessage()
        {
            var exception = new OperationException("err.1", "biz", "An error has occurred");
            Assert.Equal("err.1", exception.ErrorCode);
            Assert.Equal("biz", exception.ErrorDomain);
            Assert.Equal( "An error has occurred", exception.Message);
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public static void OperationException_WithMessageAndInner()
        {
            var inner = new OperationException("err.2", "biz");
            var exception = new OperationException("err.1", "biz", "An error has occurred", inner);
            Assert.Equal("err.1", exception.ErrorCode);
            Assert.Equal("biz", exception.ErrorDomain);
            Assert.Equal("An error has occurred", exception.Message);
            Assert.NotNull(exception.InnerException);

            var innerError = Assert.IsAssignableFrom<IOperationError>(exception.InnerException);
            Assert.Equal("err.2", innerError.Code);
            Assert.Equal("biz", innerError.Domain);
        }

        [Fact]
        public static void ValidationError_WithResults()
        {
            var results = new[] {
                new ValidationResult("First error of the validation", new []{ "Member1" }),
                new ValidationResult("Second error of the validation", new []{"Member2"})
            };

            var error = new OperationValidationError("err.1", "biz", results);

            Assert.Equal("err.1", error.Code);
            Assert.Equal("biz", error.Domain);
            Assert.Same(results, error.ValidationResults);
        }

        [Fact]
        public static void OperationException_InnerExceptionIsOperationError()
        {
            var inner = new OperationException("err.2", "biz");
            var exception = new OperationException("err.1", "biz", "An error has occurred", inner);

            Assert.Equal("err.1", exception.ErrorCode);
            Assert.Equal("biz", exception.ErrorDomain);
            Assert.Equal("An error has occurred", exception.Message);
            Assert.NotNull(exception.InnerException);

            var error = Assert.IsAssignableFrom<IOperationError>(exception);
            Assert.Equal("err.1", error.Code);
            Assert.Equal("biz", error.Domain);
            Assert.NotNull(error.InnerError);

            Assert.NotNull(exception.InnerException);
            var innerError = Assert.IsAssignableFrom<IOperationError>(exception.InnerException);
            Assert.Equal("err.2", innerError.Code);
            Assert.Equal("biz", innerError.Domain);
        }

		[Fact]
        public static void ValidationError_WithNullCode()
		{
			Assert.Throws<ArgumentNullException>(() => new OperationValidationError(null, "biz", Array.Empty<ValidationResult>()));
		}

		[Fact]
		public static void ValidationError_WithNullDomain()
		{
			Assert.Throws<ArgumentNullException>(() => new OperationValidationError("err.1", null, Array.Empty<ValidationResult>()));
		}

		[Fact]
		public static void ValidationError_WithNullResults()
		{
			Assert.Throws<ArgumentNullException>(() => new OperationValidationError("err.1", "biz", null));
		}

		[Fact]
        public static void ValidationError_WithResults_GetMemberErrors()
        {
            var results = new[] {
				new ValidationResult("First error of the validation", new []{ "Member1" }),
				new ValidationResult("Second error of the validation", new []{"Member2"})
			};

			var error = new OperationValidationError("err.1", "biz", results);
			var memberErrors = error.GetMemberErrors();
			Assert.NotNull(memberErrors);
			Assert.Equal(2, memberErrors.Count);
			Assert.True(memberErrors.TryGetValue("Member1", out var member1Errors));
			Assert.NotNull(member1Errors);
			Assert.Equal(1, member1Errors.Length);
			Assert.Equal("First error of the validation", member1Errors[0]);
			Assert.True(memberErrors.TryGetValue("Member2", out var member2Errors));
			Assert.NotNull(member2Errors);
			Assert.Equal(1, member2Errors.Length);
			Assert.Equal("Second error of the validation", member2Errors[0]);
		}

		[Fact]
		public static void ValidationError_WithResults_GetMemberErrors_Empty()
		{
			var error = new OperationValidationError("err.1", "biz", Array.Empty<ValidationResult>());
			var memberErrors = error.GetMemberErrors();
			Assert.NotNull(memberErrors);
			Assert.Empty(memberErrors);
		}

        [Fact]
        public static void ValidationError_WithResultsForSameMember()
        {
			var results = new[] {
				new ValidationResult("First error of the validation", new []{ "Member" }),
				new ValidationResult("Second error of the validation", new []{"Member"})
			};

			var error = new OperationValidationError("err.1", "biz", results);
			var memberErrors = error.GetMemberErrors();

			Assert.NotNull(memberErrors);
			Assert.Single(memberErrors);
			Assert.True(memberErrors.TryGetValue("Member", out var memberErrorsList));
			Assert.NotNull(memberErrorsList);
			Assert.Equal(2, memberErrorsList.Length);
			Assert.Equal("First error of the validation", memberErrorsList[0]);
			Assert.Equal("Second error of the validation", memberErrorsList[1]);
		}
	}
}
