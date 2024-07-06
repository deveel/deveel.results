using System.ComponentModel.DataAnnotations;

namespace Deveel
{
    public static class OperationResultOfTTests
    {
        private static IOperationResult Success(Type valueType, object? value)
        {
            var operationResultType = typeof(OperationResult<>).MakeGenericType(valueType);
            var successMethod = operationResultType.GetMethod("Success", new[] { valueType })!;
            return (IOperationResult)successMethod.Invoke(null, new[] { value })!;
        }

        private static object? GetValue(IOperationResult result)
        {
            var valueProperty = result.GetType().GetProperty("Value")!;
            return valueProperty!.GetValue(result);
        }

        [Theory]
        [InlineData(typeof(int), 42)]
        [InlineData(typeof(string), "Hello, World!")]
        [InlineData(typeof(bool), true)]
        [InlineData(typeof(bool), false)]
        [InlineData(typeof(string), null)]
        public static void OperationResultOfT_Success(Type resultType, object? value)
        {
            var result = Success(resultType, value);
            Assert.Equal(OperationResultType.Success, result.ResultType);
            Assert.Null(result.Error);

            Assert.Equal(value, GetValue(result));
        }

        [Fact]
        public static void OperationResultOfT_NotChanged()
        {
            var result = OperationResult<int>.NotChanged();
            Assert.Equal(OperationResultType.Unchanged, result.ResultType);
            Assert.Null(result.Error);
            Assert.Equal(default, result.Value);
        }

        [Fact]
        public static void OperationResultOfT_WithValue_NotChanged()
        {
            var result = OperationResult<int>.NotChanged(33);
            Assert.Equal(OperationResultType.Unchanged, result.ResultType);
            Assert.Null(result.Error);
            Assert.Equal(33, result.Value);
        }


        [Fact]
        public static void OperationResult_FailWithCode()
        {
            var result = OperationResult<int>.Fail("err.1", "biz");
            Assert.Equal(OperationResultType.Error, result.ResultType);
            Assert.NotNull(result.Error);
            Assert.Equal("err.1", result.Error.Code);
            Assert.Equal("biz", result.Error.Domain);
        }

        [Fact]
        public static void OperationResult_FailWithInner()
        {
            var inner = new OperationError("err.2", "biz");
            var result = OperationResult<int>.Fail("err.1", "biz", inner: inner);
            Assert.Equal(OperationResultType.Error, result.ResultType);
            Assert.NotNull(result.Error);
            Assert.Equal("err.1", result.Error.Code);
            Assert.Equal("biz", result.Error.Domain);
            Assert.NotNull(result.Error.InnerError);
            Assert.Equal("err.2", result.Error.InnerError.Code);
            Assert.Equal("biz", result.Error.InnerError.Domain);
        }

        [Fact]
        public static void OperationResult_FailWithMessage()
        {
            var result = OperationResult<int>.Fail("err.1", "biz", "An error occurred");
            Assert.Equal(OperationResultType.Error, result.ResultType);
            Assert.NotNull(result.Error);
            Assert.Equal("err.1", result.Error.Code);
            Assert.Equal("biz", result.Error.Domain);
            Assert.Equal("An error occurred", result.Error.Message);
        }

        [Fact]
        public static void OperationResult_FailWithMessageAndInner()
        {
            var inner = new OperationError("err.2", "biz");
            var result = OperationResult<int>.Fail("err.1", "biz", "An error occurred", inner);
            Assert.Equal(OperationResultType.Error, result.ResultType);
            Assert.NotNull(result.Error);
            Assert.Equal("err.1", result.Error.Code);
            Assert.Equal("biz", result.Error.Domain);
            Assert.Equal("An error occurred", result.Error.Message);
            Assert.NotNull(result.Error.InnerError);
            Assert.Equal("err.2", result.Error.InnerError.Code);
            Assert.Equal("biz", result.Error.InnerError.Domain);
        }

        [Fact]
        public static void OperationResult_FailWithNullCode()
        {
            Assert.Throws<ArgumentNullException>(() => OperationResult<int>.Fail(null!, "biz"));
        }

        [Fact]
        public static void OperationResult_FailWithNullDomain()
        {
            Assert.Throws<ArgumentNullException>(() => OperationResult<int>.Fail("err.1", null!));
        }

        [Fact]
        public static void OperationResult_FailWithNullCodeAndDomain()
        {
            Assert.Throws<ArgumentNullException>(() => OperationResult<int>.Fail(null!, null!));
        }

        [Fact]
        public static void ImplicitlyConvertFromValue_Success()
        {
            var value = 42;
            OperationResult<int> result = value;

            Assert.Equal(OperationResultType.Success, result.ResultType);
            Assert.Null(result.Error);
            Assert.Equal(value, result.Value);
        }

        [Fact]
        public static void ImplicitlyConvertFromDomainError_Success()
        {
            var error = new DomainException("err.1");

            OperationResult<int> result = error;

            Assert.Equal(OperationResultType.Error, result.ResultType);
            Assert.NotNull(result.Error);
            Assert.Equal("err.1", result.Error.Code);
            Assert.Equal("MyDomain", result.Error.Domain);
            Assert.Equal(default, result.Value);
        }

        [Fact]
        public static void ImplicitlyConvertFromOperationResult_Success()
        {

            var result = OperationResult.Success;
            OperationResult<int> resultOfT = result;

            Assert.Equal(OperationResultType.Success, resultOfT.ResultType);
            Assert.Null(resultOfT.Error);
            Assert.Equal(default, resultOfT.Value);

        }

        [Fact]
        public static void OperationResult_FailForValidation()
        {
            var validations = new[] {
                new ValidationResult("err.1", new []{ "Member1" }),
                new ValidationResult("err.2", new []{ "Member2" })
            };

            var result = OperationResult<int>.ValidationFailed("err.1", "biz", validations);

            Assert.Equal(OperationResultType.Error, result.ResultType);
            Assert.NotNull(result.Error);
            Assert.Equal("err.1", result.Error.Code);
            Assert.Equal("biz", result.Error.Domain);
            var opError = Assert.IsType<OperationValidationError>(result.Error);

            Assert.Equal(validations.Length, opError.ValidationResults.Count);
        }

        [Fact]
        public static void OperationResult_Fail_AsException()
        {
            var error = new OperationError("err.1", "test", "An error has occurred");
            var result = OperationResult<int>.Fail(error);

            var ex = result.AsException();

            Assert.True(result.IsError());
            Assert.NotNull(ex);
            Assert.Equal(error.Code, ex.ErrorCode);
            Assert.Equal(error.Domain, ex.ErrorDomain);
            Assert.Equal(error.Message, ex.Message);
        }

        [Fact]
        public static void OperationResult_FailWithInnerError_AsException()
        {
            var inner = new OperationError("err.0", "test", "Because of this error");
            var error = new OperationError("err.1", "test", "An error as occurred", inner);

            var result = OperationResult<int>.Fail(error);

            var ex = result.AsException();

            Assert.NotNull(ex);

            Assert.True(result.IsError());
            Assert.False(result.HasValidationErrors());

            Assert.Equal(error.Code, ex.ErrorCode);
            Assert.Equal(error.Domain, ex.ErrorDomain);
            Assert.Equal(error.Message, ex.Message);
            Assert.NotNull(ex.InnerException);

            var innerEx = Assert.IsType<OperationException>(ex.InnerException);

            Assert.NotNull(innerEx);
            Assert.Equal(inner.Code, innerEx.ErrorCode);
            Assert.Equal(inner.Domain, innerEx.ErrorDomain);
            Assert.Equal(inner.Message, innerEx.Message);
            Assert.Null(innerEx.InnerException);
        }

        [Fact]
        public static void OperationResult_Success_AsException()
        {
            var result = OperationResult<int>.Success(34);

            var ex = result.AsException();

            Assert.False(result.IsError());
            Assert.Null(ex);
        }

        [Fact]
        public static void OperationResult_SuccessWithResult_ImplicitConvert()
        {
            var result = OperationResult<int>.Success(42);

            int value = result;

            Assert.Equal(42, value);
        }

        [Fact]
        public static void OperationResult_Fail_ImplicitConvert()
        {
            var result = OperationResult<int>.Fail("err.1", "test", "An error has occurred");

            Assert.Throws<OperationException>(() => {
                int value = result;
            });
        }

        [Fact]
        public static void Match_Success()
        {
            var result = OperationResult<int>.Success(42);
            var value = result.Match(r => $"The result is {r}");
            Assert.Equal("The result is 42", value);
        }

        [Fact]
        public static void Match_Error()
        {
            var error = new OperationError("err.1", "biz");
            var result = OperationResult<int>.Fail(error);
            var value = result.Match(ifError: e => $"The error {e.Code} was generated in domain {e.Domain}");
            Assert.Equal("The error err.1 was generated in domain biz", value);
        }

        [Fact]
        public static void Match_Unchanged()
        {
            var result = OperationResult<int>.NotChanged(33);
            var value = result.Match(x => 42, ifUnchanged: x => 0);
            Assert.Equal(0, value);
        }

        [Fact]
        public static async Task MatchAsync_Success()
        {
            var result = OperationResult<int>.Success(42);
            var value = await result.MatchAsync(r => Task.FromResult<string>($"The result is {r}"));
            Assert.Equal("The result is 42", value);
        }

        [Fact]
        public static async Task MatchAsync_Error()
        {
            var error = new OperationError("err.1", "biz");
            var result = OperationResult<int>.Fail(error);
            var value = await result.MatchAsync(ifError: e => Task.FromResult($"The error {e.Code} was generated in domain {e.Domain}"));
            Assert.Equal("The error err.1 was generated in domain biz", value);
        }

        [Fact]
        public static async Task MatchAsync_Unchanged()
        {
            var result = OperationResult<int>.NotChanged(11);
            var value = await result.MatchAsync(r => Task.FromResult(r), ifUnchanged: r => Task.FromResult(0));
            Assert.Equal(0, value);
        }


        class DomainException : OperationException
        {
            public DomainException(string code)
                : base(code, "MyDomain")
            {
            }
        }
    }
}
