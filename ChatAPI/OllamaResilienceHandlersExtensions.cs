namespace ChatAPI;

public static class OllamaResilienceHandlersExtensions
{
    public static IServiceCollection AddOllamaResilienceHandlers(this IServiceCollection services)
    {
        services.ConfigureHttpClientDefaults(httpClientBuilder =>
        {
#pragma warning disable EXTEXP0001
            httpClientBuilder.RemoveAllResilienceHandlers();
#pragma warning restore EXTEXP0001

            httpClientBuilder.AddStandardResilienceHandler(config =>
            {
                config.AttemptTimeout.Timeout= TimeSpan.FromMinutes(5);
                config.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(10);
                config.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(10);
            });
        });
        
        return services;
    }
    
}