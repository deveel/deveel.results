namespace Deveel;

public static class OperationResultBindAndTapTests
{
    #region Bind Tests - IOperationResult to IOperationResult

    [Fact]
    public static void Bind_WithSuccessResult_ExecutesFunction()
    {
        var result = OperationResult.Success;
        var functionExecuted = false;

        var bindResult = result.Bind(() =>
        {
            functionExecuted = true;
            return OperationResult.Success;
        });

        Assert.True(functionExecuted);
        Assert.True(bindResult.IsSuccess());
    }

    [Fact]
    public static void Bind_WithSuccessResult_ReturnsNewResult()
    {
        var result = OperationResult.Success;
        var expectedError = new OperationError("err.1", "test");

        var bindResult = result.Bind(() => OperationResult.Fail(expectedError));

        Assert.True(bindResult.IsError());
        Assert.Equal(expectedError.Code, bindResult.Error?.Code);
        Assert.Equal(expectedError.Domain, bindResult.Error?.Domain);
    }

    [Fact]
    public static void Bind_WithErrorResult_DoesNotExecuteFunction()
    {
        var error = new OperationError("err.1", "test");
        var result = OperationResult.Fail(error);
        var functionExecuted = false;

        var bindResult = result.Bind(() =>
        {
            functionExecuted = true;
            return OperationResult.Success;
        });

        Assert.False(functionExecuted);
        Assert.True(bindResult.IsError());
        Assert.Equal(error.Code, bindResult.Error?.Code);
    }

    [Fact]
    public static void Bind_WithNullResult_ThrowsArgumentNullException()
    {
        IOperationResult result = null!;

        Assert.Throws<ArgumentNullException>(() => result.Bind(() => OperationResult.Success));
    }

    [Fact]
    public static void Bind_WithNullFunction_ThrowsArgumentNullException()
    {
        var result = OperationResult.Success;

        Assert.Throws<ArgumentNullException>(() => result.Bind(null!));
    }

    [Fact]
    public static void Bind_WithOperationErrorException_CatchesAndReturnsError()
    {
        var result = OperationResult.Success;
        var exception = new OperationException("err.1", "test", "An error occurred");

        var bindResult = result.Bind(() => throw exception);

        Assert.True(bindResult.IsError());
        Assert.NotNull(bindResult.Error);
    }

    [Fact]
    public static void Bind_WithRegularException_CatchesAndReturnsUnhandledError()
    {
        var result = OperationResult.Success;

        var bindResult = result.Bind(() => throw new InvalidOperationException("Something went wrong"));

        Assert.True(bindResult.IsError());
        Assert.NotNull(bindResult.Error);
        Assert.Equal("UnhandledException", bindResult.Error.Code);
    }

    #endregion

    #region BindAsync Tests - IOperationResult to Task<IOperationResult>

    [Fact]
    public static async Task BindAsync_WithSuccessResult_ExecutesFunction()
    {
        var result = OperationResult.Success;
        var functionExecuted = false;

        var bindResult = await result.BindAsync(async () =>
        {
            functionExecuted = true;
            await Task.Delay(0);
            return OperationResult.Success;
        });

        Assert.True(functionExecuted);
        Assert.True(bindResult.IsSuccess());
    }

    [Fact]
    public static async Task BindAsync_WithSuccessResult_ReturnsNewResult()
    {
        var result = OperationResult.Success;
        var expectedError = new OperationError("err.1", "test");

        var bindResult = await result.BindAsync(async () =>
        {
            await Task.Delay(0);
            return OperationResult.Fail(expectedError);
        });

        Assert.True(bindResult.IsError());
        Assert.Equal(expectedError.Code, bindResult.Error?.Code);
    }

    [Fact]
    public static async Task BindAsync_WithErrorResult_DoesNotExecuteFunction()
    {
        var error = new OperationError("err.1", "test");
        var result = OperationResult.Fail(error);
        var functionExecuted = false;

        var bindResult = await result.BindAsync(async () =>
        {
            functionExecuted = true;
            await Task.Delay(0);
            return OperationResult.Success;
        });

        Assert.False(functionExecuted);
        Assert.True(bindResult.IsError());
    }

    [Fact]
    public static async Task BindAsync_WithNullResult_ThrowsArgumentNullException()
    {
        IOperationResult result = null!;

        await Assert.ThrowsAsync<ArgumentNullException>(() => result.BindAsync(async () =>
        {
            await Task.Delay(0);
            return OperationResult.Success;
        }));
    }

    [Fact]
    public static async Task BindAsync_WithNullFunction_ThrowsArgumentNullException()
    {
        var result = OperationResult.Success;

        await Assert.ThrowsAsync<ArgumentNullException>(() => result.BindAsync(null!));
    }

    [Fact]
    public static async Task BindAsync_WithRegularException_CatchesAndReturnsUnhandledError()
    {
        var result = OperationResult.Success;

        var bindResult = await result.BindAsync(async () =>
        {
            await Task.Delay(0);
            throw new InvalidOperationException("Something went wrong");
        });

        Assert.True(bindResult.IsError());
        Assert.Equal("UnhandledException", bindResult.Error?.Code);
    }

    #endregion

    #region Bind<T> Tests - IOperationResult to IOperationResult<T>

    [Fact]
    public static void BindGeneric_WithSuccessResult_ExecutesFunction()
    {
        var result = OperationResult.Success;
        var functionExecuted = false;

        var bindResult = result.Bind(() =>
        {
            functionExecuted = true;
            return OperationResult<int>.Success(42);
        });

        Assert.True(functionExecuted);
        Assert.True(bindResult.IsSuccess());
        Assert.Equal(42, bindResult.Value);
    }

    [Fact]
    public static void BindGeneric_WithSuccessResult_ReturnsNewResult()
    {
        var result = OperationResult.Success;
        var expectedError = new OperationError("err.1", "test");

        var bindResult = result.Bind(() => OperationResult<string>.Fail(expectedError));

        Assert.True(bindResult.IsError());
        Assert.Equal(expectedError.Code, bindResult.Error?.Code);
        Assert.Null(bindResult.Value);
    }

    [Fact]
    public static void BindGeneric_WithErrorResult_DoesNotExecuteFunction()
    {
        var error = new OperationError("err.1", "test");
        var result = OperationResult.Fail(error);
        var functionExecuted = false;

        var bindResult = result.Bind(() =>
        {
            functionExecuted = true;
            return OperationResult<int>.Success(42);
        });

        Assert.False(functionExecuted);
        Assert.True(bindResult.IsError());
        Assert.Equal(error.Code, bindResult.Error?.Code);
    }

    [Fact]
    public static void BindGeneric_WithNullResult_ThrowsArgumentNullException()
    {
        IOperationResult result = null!;

        Assert.Throws<ArgumentNullException>(() => result.Bind(() => OperationResult<int>.Success(42)));
    }

    [Fact]
    public static void BindGeneric_WithNullFunction_ThrowsArgumentNullException()
    {
        var result = OperationResult.Success;

        Assert.Throws<ArgumentNullException>(() => result.Bind(null!));
    }

    [Fact]
    public static void BindGeneric_WithRegularException_CatchesAndReturnsUnhandledError()
    {
        var result = OperationResult.Success;

        var bindResult = result.Bind(() => throw new InvalidOperationException("Something went wrong"));

        Assert.True(bindResult.IsError());
        Assert.Equal("UnhandledException", bindResult.Error?.Code);
    }

    #endregion

    #region BindAsync<T> Tests - IOperationResult to Task<IOperationResult<T>>

    [Fact]
    public static async Task BindAsyncGeneric_WithSuccessResult_ExecutesFunction()
    {
        var result = OperationResult.Success;
        var functionExecuted = false;

        var bindResult = await result.BindAsync(async () =>
        {
            functionExecuted = true;
            await Task.Delay(0);
            return OperationResult<int>.Success(42);
        });

        Assert.True(functionExecuted);
        Assert.True(bindResult.IsSuccess());
        // Cast to check the value
        var resultOfInt = (IOperationResult<int>)bindResult;
        Assert.Equal(42, resultOfInt.Value);
    }

    [Fact]
    public static async Task BindAsyncGeneric_WithSuccessResult_ReturnsNewResult()
    {
        var result = OperationResult.Success;
        var expectedError = new OperationError("err.1", "test");

        var bindResult = await result.BindAsync(async () =>
        {
            await Task.Delay(0);
            return OperationResult<string>.Fail(expectedError);
        });

        Assert.True(bindResult.IsError());
        Assert.Equal(expectedError.Code, bindResult.Error?.Code);
    }

    [Fact]
    public static async Task BindAsyncGeneric_WithErrorResult_DoesNotExecuteFunction()
    {
        var error = new OperationError("err.1", "test");
        var result = OperationResult.Fail(error);
        var functionExecuted = false;

        var bindResult = await result.BindAsync(async () =>
        {
            functionExecuted = true;
            await Task.Delay(0);
            return OperationResult<int>.Success(42);
        });

        Assert.False(functionExecuted);
        Assert.True(bindResult.IsError());
    }

    [Fact]
    public static async Task BindAsyncGeneric_WithNullResult_ThrowsArgumentNullException()
    {
        IOperationResult result = null!;

        await Assert.ThrowsAsync<ArgumentNullException>(() => result.BindAsync(async () =>
        {
            await Task.Delay(0);
            return OperationResult<int>.Success(42);
        }));
    }

    [Fact]
    public static async Task BindAsyncGeneric_WithNullFunction_ThrowsArgumentNullException()
    {
        var result = OperationResult.Success;

        await Assert.ThrowsAsync<ArgumentNullException>(() => result.BindAsync(null!));
    }

    [Fact]
    public static async Task BindAsyncGeneric_WithRegularException_CatchesAndReturnsUnhandledError()
    {
        var result = OperationResult.Success;

        var bindResult = await result.BindAsync(async () =>
        {
            await Task.Delay(0);
            throw new InvalidOperationException("Something went wrong");
        });

        Assert.True(bindResult.IsError());
        Assert.Equal("UnhandledException", bindResult.Error?.Code);
    }

    #endregion

    #region Bind<T> Tests - IOperationResult<T> to IOperationResult

    [Fact]
    public static void BindFromGeneric_WithSuccessResult_ExecutesFunction()
    {
        var result = OperationResult<int>.Success(42);
        var functionExecuted = false;
        int? receivedValue = null;

        var bindResult = result.Bind((value) =>
        {
            functionExecuted = true;
            receivedValue = value;
            return OperationResult.Success;
        });

        Assert.True(functionExecuted);
        Assert.Equal(42, receivedValue);
        Assert.True(bindResult.IsSuccess());
    }

    [Fact]
    public static void BindFromGeneric_WithSuccessResult_ReturnsNewResult()
    {
        var result = OperationResult<int>.Success(42);
        var expectedError = new OperationError("err.1", "test");

        var bindResult = result.Bind((value) => OperationResult.Fail(expectedError));

        Assert.True(bindResult.IsError());
        Assert.Equal(expectedError.Code, bindResult.Error?.Code);
    }

    [Fact]
    public static void BindFromGeneric_WithErrorResult_DoesNotExecuteFunction()
    {
        var error = new OperationError("err.1", "test");
        var result = OperationResult<int>.Fail(error);
        var functionExecuted = false;

        var bindResult = result.Bind((value) =>
        {
            functionExecuted = true;
            return OperationResult.Success;
        });

        Assert.False(functionExecuted);
        Assert.True(bindResult.IsError());
        Assert.Equal(error.Code, bindResult.Error?.Code);
    }

    [Fact]
    public static void BindFromGeneric_WithNullValue_PassesNullToFunction()
    {
        IOperationResult<string?> result = OperationResult<string?>.Success(null);
        string? receivedValue = "default";

        var bindResult = result.Bind((value) =>
        {
            receivedValue = value;
            return OperationResult.Success;
        });

        Assert.True(bindResult.IsSuccess());
        Assert.Null(receivedValue);
    }

    [Fact]
    public static void BindFromGeneric_WithNullResult_ThrowsArgumentNullException()
    {
        IOperationResult<int> result = null!;

        Assert.Throws<ArgumentNullException>(() => result.Bind(v => OperationResult.Success));
    }

    [Fact]
    public static void BindFromGeneric_WithNullFunction_ThrowsArgumentNullException()
    {
        var result = OperationResult<int>.Success(42);

        Assert.Throws<ArgumentNullException>(() => result.Bind(null!));
    }

    [Fact]
    public static void BindFromGeneric_WithRegularException_CatchesAndReturnsUnhandledError()
    {
        var result = OperationResult<int>.Success(42);

        var bindResult = result.Bind((value) => throw new InvalidOperationException("Something went wrong"));

        Assert.True(bindResult.IsError());
        Assert.Equal("UnhandledException", bindResult.Error?.Code);
    }

    #endregion

    #region BindAsync<T> Tests - IOperationResult<T> to Task<IOperationResult>

    [Fact]
    public static async Task BindAsyncFromGeneric_WithSuccessResult_ExecutesFunction()
    {
        var result = OperationResult<int>.Success(42);
        var functionExecuted = false;
        int? receivedValue = null;

        var bindResult = await result.BindAsync(async (value) =>
        {
            functionExecuted = true;
            receivedValue = value;
            await Task.Delay(0);
            return OperationResult.Success;
        });

        Assert.True(functionExecuted);
        Assert.Equal(42, receivedValue);
        Assert.True(bindResult.IsSuccess());
    }

    [Fact]
    public static async Task BindAsyncFromGeneric_WithSuccessResult_ReturnsNewResult()
    {
        var result = OperationResult<int>.Success(42);
        var expectedError = new OperationError("err.1", "test");

        var bindResult = await result.BindAsync(async (value) =>
        {
            await Task.Delay(0);
            return OperationResult.Fail(expectedError);
        });

        Assert.True(bindResult.IsError());
        Assert.Equal(expectedError.Code, bindResult.Error?.Code);
    }

    [Fact]
    public static async Task BindAsyncFromGeneric_WithErrorResult_DoesNotExecuteFunction()
    {
        var error = new OperationError("err.1", "test");
        var result = OperationResult<int>.Fail(error);
        var functionExecuted = false;

        var bindResult = await result.BindAsync(async (value) =>
        {
            functionExecuted = true;
            await Task.Delay(0);
            return OperationResult.Success;
        });

        Assert.False(functionExecuted);
        Assert.True(bindResult.IsError());
    }

    [Fact]
    public static async Task BindAsyncFromGeneric_WithNullResult_ThrowsArgumentNullException()
    {
        IOperationResult<int> result = null!;

        await Assert.ThrowsAsync<ArgumentNullException>(() => result.BindAsync(async v =>
        {
            await Task.Delay(0);
            return OperationResult.Success;
        }));
    }

    [Fact]
    public static async Task BindAsyncFromGeneric_WithNullFunction_ThrowsArgumentNullException()
    {
        var result = OperationResult<int>.Success(42);

        await Assert.ThrowsAsync<ArgumentNullException>(() => result.BindAsync(null!));
    }

    [Fact]
    public static async Task BindAsyncFromGeneric_WithRegularException_CatchesAndReturnsUnhandledError()
    {
        var result = OperationResult<int>.Success(42);

        var bindResult = await result.BindAsync(async (value) =>
        {
            await Task.Delay(0);
            throw new InvalidOperationException("Something went wrong");
        });

        Assert.True(bindResult.IsError());
        Assert.Equal("UnhandledException", bindResult.Error?.Code);
    }

    #endregion

    #region Tap Tests - IOperationResult

    [Fact]
    public static void Tap_WithSuccessResult_ExecutesAction()
    {
        var result = OperationResult.Success;
        var actionExecuted = false;
        IOperationResult? receivedResult = null;

        var tapResult = result.Tap((r) =>
        {
            actionExecuted = true;
            receivedResult = r;
        });

        Assert.True(actionExecuted);
        Assert.NotNull(receivedResult);
        Assert.True(tapResult.IsSuccess());
    }

    [Fact]
    public static void Tap_WithErrorResult_ExecutesAction()
    {
        var error = new OperationError("err.1", "test");
        var result = OperationResult.Fail(error);
        var actionExecuted = false;

        var tapResult = result.Tap((r) =>
        {
            actionExecuted = true;
        });

        Assert.True(actionExecuted);
        Assert.True(tapResult.IsError());
    }

    [Fact]
    public static void Tap_WithNullResult_ThrowsArgumentNullException()
    {
        IOperationResult result = null!;

        Assert.Throws<ArgumentNullException>(() => result.Tap(r => { }));
    }

    [Fact]
    public static void Tap_WithNullAction_ThrowsArgumentNullException()
    {
        var result = OperationResult.Success;

        Assert.Throws<ArgumentNullException>(() => result.Tap(null!));
    }

    [Fact]
    public static void Tap_WithActionThrowingRegularException_CatchesAndReturnsUnhandledError()
    {
        var result = OperationResult.Success;

        var tapResult = result.Tap((r) => throw new InvalidOperationException("Something went wrong"));

        Assert.True(tapResult.IsError());
        Assert.Equal("UnhandledException", tapResult.Error?.Code);
    }

    #endregion

    #region TapAsync Tests - IOperationResult

    [Fact]
    public static async Task TapAsync_WithSuccessResult_ExecutesAction()
    {
        var result = OperationResult.Success;
        var actionExecuted = false;
        IOperationResult? receivedResult = null;

        var tapResult = await result.TapAsync(async (r) =>
        {
            actionExecuted = true;
            receivedResult = r;
            await Task.Delay(0);
        });

        Assert.True(actionExecuted);
        Assert.NotNull(receivedResult);
        Assert.True(tapResult.IsSuccess());
    }

    [Fact]
    public static async Task TapAsync_WithErrorResult_ExecutesAction()
    {
        var error = new OperationError("err.1", "test");
        IOperationResult result = OperationResult.Fail(error);
        var actionExecuted = false;

        var tapResult = await result.TapAsync(async (r) =>
        {
            actionExecuted = true;
            await Task.Delay(0);
        });

        Assert.True(actionExecuted);
        Assert.True(tapResult.IsError());
    }
    [Fact]
    public static async Task TapAsync_WithNullResult_ThrowsArgumentNullException()
    {
        IOperationResult result = null!;

        await Assert.ThrowsAsync<ArgumentNullException>(() => result.TapAsync(async r => await Task.Delay(0)));
    }

    [Fact]
    public static async Task TapAsync_WithNullAction_ThrowsArgumentNullException()
    {
        var result = OperationResult.Success;

        await Assert.ThrowsAsync<ArgumentNullException>(() => result.TapAsync(null!));
    }

    [Fact]
    public static async Task TapAsync_WithActionThrowingRegularException_CatchesAndReturnsUnhandledError()
    {
        var result = OperationResult.Success;

        var tapResult = await result.TapAsync(async (r) =>
        {
            await Task.Delay(0);
            throw new InvalidOperationException("Something went wrong");
        });

        Assert.True(tapResult.IsError());
        Assert.Equal("UnhandledException", tapResult.Error?.Code);
    }

    #endregion

    #region Tap<T> Tests - IOperationResult<T>

    [Fact]
    public static void TapGeneric_WithSuccessResult_ExecutesAction()
    {
        var result = OperationResult<int>.Success(42);
        var actionExecuted = false;
        IOperationResult<int>? receivedResult = null;

        var tapResult = result.Tap((r) =>
        {
            actionExecuted = true;
            receivedResult = r;
        });

        Assert.True(actionExecuted);
        Assert.NotNull(receivedResult);
        Assert.True(tapResult.IsSuccess());
        Assert.Equal(42, tapResult.Value);
    }

    [Fact]
    public static void TapGeneric_WithErrorResult_ExecutesAction()
    {
        var error = new OperationError("err.1", "test");
        var result = OperationResult<int>.Fail(error);
        var actionExecuted = false;

        var tapResult = result.Tap((r) =>
        {
            actionExecuted = true;
        });

        Assert.True(actionExecuted);
        Assert.True(tapResult.IsError());
    }

    [Fact]
    public static void TapGeneric_WithNullResult_ThrowsArgumentNullException()
    {
        IOperationResult<int> result = null!;

        Assert.Throws<ArgumentNullException>(() => result.Tap(r => { }));
    }

    [Fact]
    public static void TapGeneric_WithNullAction_ThrowsArgumentNullException()
    {
        var result = OperationResult<int>.Success(42);

        Assert.Throws<ArgumentNullException>(() => result.Tap((Action<IOperationResult<int>>)null!));
    }

    [Fact]
    public static void TapGeneric_WithActionThrowingRegularException_CatchesAndReturnsUnhandledError()
    {
        var result = OperationResult<int>.Success(42);

        var tapResult = result.Tap((r) => throw new InvalidOperationException("Something went wrong"));

        Assert.True(tapResult.IsError());
        Assert.Equal("UnhandledException", tapResult.Error?.Code);
    }

    #endregion

    #region TapAsync<T> Tests - IOperationResult<T>

    [Fact]
    public static async Task TapAsyncGeneric_WithSuccessResult_ExecutesAction()
    {
        var result = OperationResult<int>.Success(42);
        var actionExecuted = false;
        IOperationResult<int>? receivedResult = null;

        var tapResult = await result.TapAsync(async (r) =>
        {
            actionExecuted = true;
            receivedResult = r;
            await Task.Delay(0);
        });

        Assert.True(actionExecuted);
        Assert.NotNull(receivedResult);
        Assert.True(tapResult.IsSuccess());
        Assert.Equal(42, tapResult.Value);
    }

    [Fact]
    public static async Task TapAsyncGeneric_WithErrorResult_ExecutesAction()
    {
        var error = new OperationError("err.1", "test");
        var result = OperationResult<int>.Fail(error);
        var actionExecuted = false;

        var tapResult = await result.TapAsync(async (r) =>
        {
            actionExecuted = true;
            await Task.Delay(0);
        });

        Assert.True(actionExecuted);
        Assert.True(tapResult.IsError());
    }

    [Fact]
    public static async Task TapAsyncGeneric_WithNullResult_ThrowsArgumentNullException()
    {
        IOperationResult<int> result = null!;

        await Assert.ThrowsAsync<ArgumentNullException>(() => result.TapAsync(async r => await Task.Delay(0)));
    }

    [Fact]
    public static async Task TapAsyncGeneric_WithNullAction_ThrowsArgumentNullException()
    {
        var result = OperationResult<int>.Success(42);

        await Assert.ThrowsAsync<ArgumentNullException>(() => result.TapAsync((Func<IOperationResult<int>, Task>)null!));
    }

    [Fact]
    public static async Task TapAsyncGeneric_WithActionThrowingRegularException_CatchesAndReturnsUnhandledError()
    {
        var result = OperationResult<int>.Success(42);

        var tapResult = await result.TapAsync(async (r) =>
        {
            await Task.Delay(0);
            throw new InvalidOperationException("Something went wrong");
        });

        Assert.True(tapResult.IsError());
        Assert.Equal("UnhandledException", tapResult.Error?.Code);
    }

    #endregion

    #region Chaining Tests

    [Fact]
    public static void Bind_CanBeChained()
    {
        var result = OperationResult.Success
            .Bind(() => OperationResult<int>.Success(10))
            .Bind((value) => value > 5 ? OperationResult.Success : OperationResult.Fail("err.1", "test"));

        Assert.True(result.IsSuccess());
    }

    [Fact]
    public static void Tap_CanBeChained()
    {
        var tapExecutedCount = 0;
        var result = OperationResult.Success
            .Tap(r => tapExecutedCount++)
            .Tap(r => tapExecutedCount++);

        Assert.Equal(2, tapExecutedCount);
        Assert.True(result.IsSuccess());
    }

    [Fact]
    public static void BindAndTap_CanBeChained()
    {
        var bindExecuted = false;
        var tapExecuted = false;

        var result = OperationResult.Success
            .Bind(() =>
            {
                bindExecuted = true;
                return OperationResult<int>.Success(42);
            })
            .Tap(r => tapExecuted = true)
            .Bind(value => value > 0 ? OperationResult.Success : OperationResult.Fail("err.1", "test"));

        Assert.True(bindExecuted);
        Assert.True(tapExecuted);
        Assert.True(result.IsSuccess());
    }

    #endregion

    #region Edge Cases

    [Fact]
    public static void Bind_WithUnchangedResult_DoesNotExecuteFunction()
    {
        var result = OperationResult.NotChanged;
        var functionExecuted = false;

        var bindResult = result.Bind(() =>
        {
            functionExecuted = true;
            return OperationResult.Success;
        });

        Assert.False(functionExecuted);
        Assert.True(bindResult.IsUnchanged());
    }

    [Fact]
    public static void Tap_WithUnchangedResult_ExecutesAction()
    {
        IOperationResult result = OperationResult.NotChanged;
        var actionExecuted = false;

        var tapResult = result.Tap(r => actionExecuted = true);

        Assert.True(actionExecuted);
        Assert.True(tapResult.IsUnchanged());
    }

    [Fact]
    public static void TapGeneric_WithUnchangedResult_ExecutesAction()
    {
        var result = OperationResult<int>.NotChanged(33);
        var actionExecuted = false;
        int? receivedValue = null;

        var tapResult = result.Tap(r =>
        {
            actionExecuted = true;
            receivedValue = r.Value;
        });

        Assert.True(actionExecuted);
        Assert.Equal(33, receivedValue);
        Assert.True(tapResult.IsUnchanged());
    }

    [Fact]
    public static void Bind_HandlesNestedErrors()
    {
        var innerError = new OperationError("inner.err", "test", "Inner error");
        var outerError = new OperationError("outer.err", "test", "Outer error", innerError);
        var result = OperationResult.Fail(outerError);

        var bindResult = result.Bind(() => OperationResult.Success);

        Assert.True(bindResult.IsError());
        Assert.Equal("outer.err", bindResult.Error?.Code);
        Assert.NotNull(bindResult.Error?.InnerError);
        Assert.Equal("inner.err", bindResult.Error.InnerError.Code);
    }

    #endregion
}



























