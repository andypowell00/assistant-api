using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Assistant_API.Constants;

namespace Assistant_API.Services
{
    public class BlogPostService
    {
        private readonly IChatCompletionService _chatCompletionService;
        private readonly ILogger<BlogPostService> _logger;

        public BlogPostService(IChatCompletionService chatCompletionService, ILogger<BlogPostService> logger)
        {
            _chatCompletionService = chatCompletionService;
            _logger = logger;
        }

        /// <summary>
        /// Generates a comprehensive blog post from notes including summary, metadata, and formatted content
        /// </summary>
        public async Task<BlogPostResult> GenerateBlogPostAsync(string notes)
        {
            try
            {
                // Step 1: Generate summary and metadata
                var (summary, metadata) = await GenerateSummaryAndMetadataAsync(notes);
                
                // Step 2: Generate the full blog post content
                var blogContent = await GenerateBlogContentAsync(notes, summary);
                
                // Step 3: Create the filename
                var filename = GenerateFilename(metadata.Title);
                
                return new BlogPostResult
                {
                    Metadata = metadata,
                    Summary = summary,
                    Content = blogContent,
                    Filename = filename
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating blog post");
                throw;
            }
        }

        /// <summary>
        /// Generates a summary and metadata for a blog post based on notes
        /// </summary>
        public async Task<(string Summary, BlogMetadata Metadata)> GenerateSummaryAndMetadataAsync(string notes)
        {
            var prompt = $"{BlogConstants.SummaryAndMetadataPrompt}\n\nNotes:\n{notes}";

            _logger.LogInformation("Generating summary and metadata");
            var response = await _chatCompletionService.GetChatMessageContentAsync(prompt);

            if (string.IsNullOrWhiteSpace(response?.Content))
            {
                _logger.LogWarning("Empty response from chat completion service");
                return CreateDefaultSummaryAndMetadata(notes);
            }

            try
            {
                // Extract JSON from the response (in case the model included extra text)
                var jsonMatch = Regex.Match(response.Content, @"\{(?:[^{}]|(?<open>\{)|(?<-open>\}))+(?(open)(?!))\}");
                if (!jsonMatch.Success)
                {
                    _logger.LogWarning("Could not extract JSON from response");
                    return CreateDefaultSummaryAndMetadata(notes);
                }

                var jsonContent = jsonMatch.Value;
                var jsonResponse = JsonSerializer.Deserialize<CombinedResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (jsonResponse == null)
                {
                    _logger.LogWarning("Failed to deserialize JSON response");
                    return CreateDefaultSummaryAndMetadata(notes);
                }

                return (
                    jsonResponse.Summary ?? "No summary available.",
                    jsonResponse.Metadata ?? CreateDefaultMetadata(notes)
                );
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error parsing JSON response");
                return CreateDefaultSummaryAndMetadata(notes);
            }
        }

        /// <summary>
        /// Generates the full blog post content based on notes and summary
        /// </summary>
        private async Task<string> GenerateBlogContentAsync(string notes, string summary)
        {
            var prompt = $"{BlogConstants.BlogPostGenerationPrompt}\n\nSummary:\n{summary}\n\nOriginal Notes:\n{notes}";

            _logger.LogInformation("Generating blog content");
            var response = await _chatCompletionService.GetChatMessageContentAsync(prompt);
            
            return response?.Content?.Trim() ?? summary;
        }

        /// <summary>
        /// Generates a filename for the blog post based on the title
        /// </summary>
        private string GenerateFilename(string title)
        {
            // Format: YYYY-MM-DD-Title-With-Hyphens.md
            var date = DateTime.Now.ToString("yyyy-MM-dd");
            
            // Convert title to URL-friendly format
            var urlFriendlyTitle = Regex.Replace(title, @"[^a-zA-Z0-9\s-]", "");
            urlFriendlyTitle = Regex.Replace(urlFriendlyTitle, @"\s+", "-");
            urlFriendlyTitle = urlFriendlyTitle.Trim('-');
            
            return $"{date}-{urlFriendlyTitle}{BlogConstants.DefaultMarkdownFileExtension}";
        }

        /// <summary>
        /// Creates default summary and metadata if generation fails
        /// </summary>
        private (string Summary, BlogMetadata Metadata) CreateDefaultSummaryAndMetadata(string notes)
        {
            return (
                "This blog post contains technical notes and insights about a project or technology.",
                CreateDefaultMetadata(notes)
            );
        }

        /// <summary>
        /// Creates default metadata based on notes content
        /// </summary>
        private BlogMetadata CreateDefaultMetadata(string notes)
        {
            // Extract potential title from first line or use default
            var firstLine = notes.Split('\n').FirstOrDefault()?.Trim() ?? "";
            var title = firstLine.Length > 5 ? firstLine : "Technical Notes and Insights";
            
            // Extract potential categories from content
            var categories = new List<string> { "Technical", "Development" };
            
            // Extract potential tags from content
            var commonTechTerms = new[] { "AWS", "Azure", "Docker", "Kubernetes", "API", "React", "Angular", "Vue", 
                                         "JavaScript", "TypeScript", "Python", "C#", ".NET", "Java", "PHP", "SQL", 
                                         "NoSQL", "MongoDB", "Redis", "Git", "CI/CD", "DevOps" };
            
            var tags = commonTechTerms
                .Where(term => notes.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Take(5)
                .ToList();
            
            if (!tags.Any())
            {
                tags.Add("Development");
                tags.Add("Technology");
            }
            
            return new BlogMetadata
            {
                Title = title,
                Categories = categories,
                Tags = tags
            };
        }
    }

    public class CombinedResponse
    {
        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("metadata")]
        public BlogMetadata? Metadata { get; set; }
    }

    public class BlogMetadata
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; } = new();

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();
    }

    public class BlogPostResult
    {
        public BlogMetadata Metadata { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
    }
}
