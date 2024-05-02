using System.Text.RegularExpressions;

namespace NewDotnet.Code
{
    public static class Extensions
    {
        /// <summary>
        /// Replace all occurrences of keys enclosed in braces with the corresponding values from the provided dictionary.
        /// </summary>
        /// <param name="s">Source string containing keys enclosed in braces.</param>
        /// <param name="replacements">Dictionary with keys and their corresponding replacement values.</param>
        /// <returns>A string with all keys replaced by corresponding values from the dictionary.</returns>
        public static string BuildUsingDictionary(this string s, Dictionary<string, string> replacements)
        {
            return Regex.Replace(s, @"\{([^}]+)\}", match =>
            {
                // Attempt to replace matched key with value from the dictionary.
                if (replacements.TryGetValue(match.Groups[1].Value, out var replacement))
                {
                    return replacement;
                }
                return match.Value; // If no replacement found, return the match itself.
            });
        }

        /// <summary>
        /// Removes consecutive duplicate entries from a list.
        /// </summary>
        /// <param name="inputList">The input list from which to remove duplicates.</param>
        /// <returns>A new list with consecutive duplicates removed.</returns>
        public static List<T> ExcludeConsecutiveDuplicates<T>(this List<T> inputList)
        {
            if (inputList == null || inputList.Count <= 1)
                return inputList ?? new List<T>();

            var result = new List<T> { inputList[0] };

            for (int i = 1; i < inputList.Count; i++)
            {
                if (!Equals(inputList[i], inputList[i - 1]))
                {
                    result.Add(inputList[i]);
                }
            }

            return result;
        }
    }
}
