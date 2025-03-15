using Assistant_API.Constants;
using Assistant_API.Services;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Assistant_API.Plugins
{
    public class BlogPostPlugin
    {
        private readonly ILogger<BlogPostPlugin> _logger;
        private readonly IConfiguration _configuration;

        public BlogPostPlugin(ILogger<BlogPostPlugin> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [KernelFunction("generate_blog_post")]
        [Description("Generates a blog post with a Markdown header and content based on the provided project notes.")]
        [return: Description("A blog post in Markdown format.")]
        public async Task<string> GenerateBlogPost([Description("The project notes to summarize.")] string notes, BlogPostService blogPostService)
        {
            try
            {
                _logger.LogInformation("Generating blog post from notes");
                
                // Generate the complete blog post
                var blogPostResult = await blogPostService.GenerateBlogPostAsync(notes);
                
                // Generate Markdown content with proper Jekyll front matter
                var markdown = CreateMarkdownContent(blogPostResult);
                
                // Save the Markdown content to a file with the proper filename
                SaveMarkdownToFile(markdown, blogPostResult.Filename);
                
                _logger.LogInformation($"Blog post generated and saved as {blogPostResult.Filename}");
                
                return markdown;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating blog post");
                throw;
            }
        }

        private string CreateMarkdownContent(BlogPostResult blogPostResult)
        {
            // Format the date in Jekyll format: YYYY-MM-DD HH:MM:SS -0400
            var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss zzz");
            
            // Format categories and tags as comma-separated lists
            var categories = string.Join(" ", blogPostResult.Metadata.Categories);
            var tags = string.Join(" ", blogPostResult.Metadata.Tags);
            
            // Create Jekyll front matter with proper formatting (no indentation)
            var frontMatter = 
         $@"---
layout: post
title:  ""{blogPostResult.Metadata.Title}""
date:   {date}
categories: {categories}
tags: {tags}
---

";
            
            // Return the complete markdown with front matter and content
            return frontMatter + blogPostResult.Content;
        }

        private void SaveMarkdownToFile(string markdownContent, string filename)
        {
            // Get the directory from configuration
            var directoryPath = _configuration["FilePaths:GeneratedBlogsDirectory"];
            
            // Use default if not configured
            if (string.IsNullOrEmpty(directoryPath))
            {
                directoryPath = Path.Combine(AppContext.BaseDirectory, BlogConstants.GeneratedBlogsDirectory);
                _logger.LogWarning($"GeneratedBlogsDirectory not found in configuration, using default: {directoryPath}");
            }
            else
            {
                _logger.LogInformation($"Using blog posts directory from configuration: {directoryPath}");
            }

            if (!Directory.Exists(directoryPath))
            {
                _logger.LogInformation($"Creating directory: {directoryPath}");
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = Path.Combine(directoryPath, filename);
            
            _logger.LogInformation($"Saving blog post to: {filePath}");
            File.WriteAllText(filePath, markdownContent);
        }
    }
}
