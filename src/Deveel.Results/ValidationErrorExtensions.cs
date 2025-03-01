namespace Deveel
{
	/// <summary>
	/// Extensions for the <see cref="IValidationError"/> contract.
	/// </summary>
	public static class ValidationErrorExtensions
    {
		/// <summary>
		/// Gets a dictionary of member names and the list of error messages
		/// </summary>
		/// <param name="error">
		/// The validation error to get the member errors from.
		/// </param>
		/// <returns>
		/// Returns a dictionary where the key is the member name and the value
		/// is the list of error messages for that member.
		/// </returns>
		public static IDictionary<string, string[]> GetMemberErrors(this IValidationError error)
		{
			ArgumentNullException.ThrowIfNull(error, nameof(error));

			var results = new Dictionary<string, List<string>>();

			foreach (var result in error.ValidationResults)
			{
				foreach (var memberName in result.MemberNames)
				{
					if (String.IsNullOrWhiteSpace(result.ErrorMessage))
						continue;

					if (!results.TryGetValue(memberName, out var messages))
					{
						messages = new List<string>();
						results[memberName] = messages;
					}

					messages.Add(result.ErrorMessage);
				}
			}

			return results.ToDictionary(x => x.Key, x => x.Value.ToArray());
		}
	}
}
