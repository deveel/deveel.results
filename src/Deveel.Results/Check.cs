namespace Deveel;

/// <summary>
/// Provides utility methods for argument validation.
/// </summary>
static class Check
{
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the specified value is <see langword="null"/>.
    /// </summary>
    /// <param name="value">The value to check for null.</param>
    /// <param name="paramName">The name of the parameter being checked.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static void ThrowIfNull(object? value, string paramName)
    {
        if (value is null)
            throw new ArgumentNullException(paramName);
    }
}