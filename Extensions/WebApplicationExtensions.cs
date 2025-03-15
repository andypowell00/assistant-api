using Assistant_API.Endpoints;

namespace Assistant_API.Extensions
{
    public static class WebApplicationExtensions
    {
        public static void MapApplicationEndpoints(this WebApplication app)
        {
           
            app.MapBlogEndpoints();
            app.MapSummarizeEndpoints();

        }
    }
}