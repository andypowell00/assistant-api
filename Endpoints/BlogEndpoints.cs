using Microsoft.SemanticKernel;
using Assistant_API.Plugins;
using Assistant_API.Services;
using Assistant_API.Constants;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace Assistant_API.Endpoints
{
    public static class BlogEndpoints
    {
        public static void MapBlogEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/generate-blog", HandleGenerateBlogAsync);
            app.MapPost("/generate-blog-from-file", HandleGenerateBlogFromFileAsync);
        }

        private static async Task<IResult> HandleGenerateBlogAsync(
            BlogRequest blogRequest,
            BlogPostService blogPostService,
            IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            try
            {
                var logger = loggerFactory.CreateLogger("BlogEndpoints");
                logger.LogInformation("Received request to generate blog post");
                
                // Create the plugin with logger and configuration
                var plugin = new BlogPostPlugin(
                    loggerFactory.CreateLogger<BlogPostPlugin>(),
                    configuration);
                
                // Generate the blog post
                var markdown = await plugin.GenerateBlogPost(blogRequest.Notes, blogPostService);
                
                // Return the generated markdown
                return Results.Ok(new BlogResponse(markdown));
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    title: "Error generating blog post",
                    statusCode: 500);
            }
        }

        private static async Task<IResult> HandleGenerateBlogFromFileAsync(
            BlogFileRequest blogFileRequest,
            BlogPostService blogPostService,
            IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            try
            {
                var logger = loggerFactory.CreateLogger("BlogEndpoints");
                logger.LogInformation($"Received request to generate blog post from file: {blogFileRequest.FilePath}");
                
                // Validate file path
                if (!File.Exists(blogFileRequest.FilePath))
                {
                    logger.LogWarning($"File not found: {blogFileRequest.FilePath}");
                    return Results.Problem(
                        detail: $"File not found: {blogFileRequest.FilePath}",
                        title: "File Not Found",
                        statusCode: 404);
                }
                
                // Read the file content
                string fileContent;
                try
                {
                    fileContent = await File.ReadAllTextAsync(blogFileRequest.FilePath);
                    logger.LogInformation($"Successfully read file content ({fileContent.Length} characters)");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error reading file: {blogFileRequest.FilePath}");
                    return Results.Problem(
                        detail: $"Error reading file: {ex.Message}",
                        title: "File Read Error",
                        statusCode: 500);
                }
                
                // Create the plugin with logger and configuration
                var plugin = new BlogPostPlugin(
                    loggerFactory.CreateLogger<BlogPostPlugin>(),
                    configuration);
                
                // Generate the blog post from the file content
                var markdown = await plugin.GenerateBlogPost(fileContent, blogPostService);
                
                // Return the generated markdown
                return Results.Ok(new BlogResponse(markdown));
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    title: "Error generating blog post from file",
                    statusCode: 500);
            }
        }
        
    }

    public record BlogRequest(string Notes);
    public record BlogFileRequest(string FilePath);
    public record BlogResponse(string Markdown);
    
    public class BlogInfo
    {
        public string Filename { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }
}
