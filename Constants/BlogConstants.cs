namespace Assistant_API.Constants
{
    /// <summary>
    /// Constants related to blog post generation
    /// </summary>
    public static class BlogConstants
    {
        /// <summary>
        /// Directory where generated blog posts are stored
        /// </summary>
        public const string GeneratedBlogsDirectory = "generated_blogs";

        /// <summary>
        /// Default markdown file extension
        /// </summary>
        public const string DefaultMarkdownFileExtension = ".md";

        /// <summary>
        /// System message for blog post generation
        /// </summary>
        public const string BlogPostGenerationPrompt = @"
        You are a professional technical blog writer. Based on the following project notes and summary, create a well-structured, engaging blog post.

        Write a complete blog post that:
        1. Expands on the summary with more details from the notes
        2. Uses proper markdown formatting with headings, code blocks, and links
        3. Has a clear introduction, body, and conclusion
        4. Includes code examples where relevant
        5. Explains technical concepts clearly
        6. Uses a professional but conversational tone

        Your response should ONLY include the blog post content in markdown format.";

        /// <summary>
        /// System message for summary and metadata generation
        /// </summary>
        public const string SummaryAndMetadataPrompt = @"
            You are a professional technical blog writer. Based on the following project notes, perform these tasks:

            1. Create a concise, clear, and engaging summary that captures the key points. This summary should be suitable for a technical blog post introduction.
            2. Generate appropriate metadata including a descriptive title, relevant categories, and specific tags based on the content.

            The metadata should be in JSON format with this structure:
            {
                ""summary"": ""<A concise, engaging summary of the notes, focusing on key technical points and insights>"",
                ""metadata"": {
                    ""title"": ""<A clear, descriptive title that captures the main topic>"",
                    ""categories"": [""<primary category>"", ""<secondary category>"", ...],
                    ""tags"": [""<specific tag1>"", ""<specific tag2>"", ...]
                }
            }

            Your response should ONLY include the JSON with no additional text.";
    }
}
