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
    }
}
