using Assistant_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Assistant_API.Endpoints
{
    public static class SummarizeEndpoints
    {
        public static void MapSummarizeEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/summarize", HandleSummarizationAsync);
        }

        private static async Task<IResult> HandleSummarizationAsync(
            SummarizationService summarizationService,
            [FromBody] SummarizeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return Results.BadRequest("Input text cannot be empty.");

            var summary = await summarizationService.SummarizeAsync(request.Text);
            return Results.Ok(new { Summary = summary });
        }
    }

    public record SummarizeRequest(string Text);
}
