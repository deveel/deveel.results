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
            var result = OperationResult<int>.NotChanged;
            Assert.Equal(OperationResultType.Unchanged, result.ResultType);
            Assert.Null(result.Error);
            Assert.Equal(default, result.Value);
        }

        [Fact]
        public static void OperationResultOfT_Cancelled()
        {
            var result = OperationResult<int>.Cancelled;
            Assert.Equal(OperationResultType.Cancelled, result.ResultType);
            Assert.Null(result.Error);
            Assert.Equal(default, result.Value);
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

        class DomainException : OperationException
        {
            public DomainException(string code)
                : base(code, "MyDomain")
            {
            }
        }
    }
}
