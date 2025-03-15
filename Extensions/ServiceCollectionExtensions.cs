using Assistant_API.Endpoints;
using Assistant_API.Plugins;
using Assistant_API.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Assistant_API.Extensions

{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register Semantic Kernel
            services.AddSingleton(sp =>
            {
                var builder = Kernel.CreateBuilder();
                var kernel = builder.Build();

                // Add necessary plugins
                kernel.ImportPluginFromType<BlogPostPlugin>("BlogPost");
                // Add invocation filters
                kernel.AutoFunctionInvocationFilters.Add(new AutoInvocationFilter());

                return kernel;
            });

            services.AddHttpClient();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            // Add scoped services

            services.AddScoped<SummarizationService>(); 
            services.AddScoped<BlogPostService>();
            // Add logging
            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Information);
            });

            // Register our custom services
            services.AddSingleton<BlogPostService>();
            services.AddSingleton<SummarizationService>();

            return services;
        }

        private sealed class AutoInvocationFilter : IAutoFunctionInvocationFilter
        {
            public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
            {
                // Execute the function
                await next(context);

               
            }
        }
    }
}
