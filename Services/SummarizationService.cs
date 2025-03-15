using Assistant_API.Utilities;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Assistant_API.Services
{
    public class SummarizationService
    {
        public readonly IChatCompletionService _chatCompletionService;

        public SummarizationService(IChatCompletionService chatCompletionService)
        {
            _chatCompletionService = chatCompletionService;
        }

        /// <summary>
        /// Summarizes the provided text after ensuring JSON safety.
        /// </summary>
        /// <param name="text">The text to summarize.</param>
        /// <returns>A concise summary limited to 500 words.</returns>
        public async Task<string> SummarizeAsync(string text)
        {
            // Ensure the input text is JSON safe
            var sanitizedText = JsonHelper.EscapeForJson(text);

            var prompt = $@"
            Please summarize the following text into a concise and clear summary suitable for general use.
            The summary should not exceed 500 words. Focus on the key points and omit unnecessary details:

            {sanitizedText}

            Your response should only include the summarized content.";

            var response = await _chatCompletionService.GetChatMessageContentAsync(prompt);
            return response?.ToString().Trim() ?? "No summary available.";
        }
    }
}
