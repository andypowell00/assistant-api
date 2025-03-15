using Assistant_API.DataAccess.Data;
using Assistant_API.Extensions;
using Assistant_API.Services;
using FluentValidation;
using Assistant_API.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using OllamaSharp.Models.Chat;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices(); // Custom DI extensions
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Register our custom services
builder.Services.AddSingleton<BlogPostService>();
builder.Services.AddSingleton<SummarizationService>();

// Register IChatCompletionService
builder.Services.AddSingleton<IChatCompletionService>(sp => 
{
    // Use the standard constructor with improved parameters
    return new OllamaChatCompletionService(
        modelId: ApiConstants.OllamaModelName, 
        endpoint: new Uri(ApiConstants.MainOllamaEndpoint));
});

// Add database context
//builder.Services.AddDbContext<AiAssistantDbContext>(options =>
// options.UseInMemoryDatabase("AiAssistantDb"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapApplicationEndpoints(); // Custom extension to map all endpoints

app.UseHttpsRedirection();

app.Run();
