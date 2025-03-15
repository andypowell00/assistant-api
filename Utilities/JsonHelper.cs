using Newtonsoft.Json;


namespace Assistant_API.Utilities
{
    public static class JsonHelper
    {
        /// <summary>
        /// Ensures the input string is JSON-safe by escaping special characters.
        /// </summary>
        /// <param name="rawText">The raw input text.</param>
        /// <returns>A JSON-safe string.</returns>
        public static string EscapeForJson(string rawText)
        {
            if (string.IsNullOrEmpty(rawText))
                return rawText;

            return JsonConvert.ToString(rawText).Trim('"');
        }
    }
}
