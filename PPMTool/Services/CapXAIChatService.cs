using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using Radzen;

namespace PPMTool.Services;

/// <summary>
/// Extends Radzen's AI chat service by automatically adding the
/// generated CapX database schema / rules prompt to every conversation as the system prompt.
/// </summary>
public sealed class CapXAIChatService : AIChatService, IAIChatService
{
    private readonly DatabaseSchemaPromptService schemaPromptService;

    /// <summary>
    /// Creates a new CapX AI chat service.
    /// </summary>
    /// <param name="serviceProvider">The application's service provider.</param>
    /// <param name="options">The configured Radzen AI chat options.</param>
    /// <param name="schemaPromptService">The service that generates the CapX database system prompt.</param>
    public CapXAIChatService(
        IServiceProvider serviceProvider,
        IOptions<AIChatServiceOptions> options,
        DatabaseSchemaPromptService schemaPromptService)
        : base(serviceProvider, options)
    {
        this.schemaPromptService = schemaPromptService;
    }

    /// <summary>
    /// Gets chat completions using Radzen's existing implementation,
    /// with the generated CapX database prompt supplied as the system
    /// prompt.
    /// Have to do this as a new method because the base class does not mark the method as virtual.
    /// </summary>
    public new async IAsyncEnumerable<string> GetCompletionsAsync(
        string userInput,
        string sessionId = null,
        [EnumeratorCancellation]
        CancellationToken cancellationToken = default,
        string model = null,
        string systemPrompt = null,
        double? temperature = null,
        int? maxTokens = null,
        string endpoint = null,
        string proxy = null,
        string apiKey = null,
        string apiKeyHeader = null)
    {
        // Get the generated CapX system prompt from the DatabaseSchemaPromptService.
        var capXSystemPrompt =
            await schemaPromptService.GetPromptAsync(
                cancellationToken);

        // Merge the CapX system prompt with any additional instructions supplied through the RadzenAIChat component.
        var effectiveSystemPrompt =
            MergeSystemPrompts(
                capXSystemPrompt,
                systemPrompt);

        // Call the base class's GetCompletionsAsync method with the effective system prompt.
        await foreach (var responseChunk in
            base.GetCompletionsAsync(
                userInput: userInput,
                sessionId: sessionId,
                cancellationToken: cancellationToken,
                model: model,
                systemPrompt: effectiveSystemPrompt,
                temperature: temperature,
                maxTokens: maxTokens,
                endpoint: endpoint,
                proxy: proxy,
                apiKey: apiKey,
                apiKeyHeader: apiKeyHeader))
        {
            yield return responseChunk;
        }
    }

    /// <summary>
    /// Combines the generated CapX system prompt with any additional
    /// instructions supplied through the RadzenAIChat component.
    /// </summary>
    private static string MergeSystemPrompts(
        string capXSystemPrompt,
        string additionalSystemPrompt)
    {
        if (string.IsNullOrWhiteSpace(additionalSystemPrompt))
        {
            return capXSystemPrompt;
        }

        return $"""
            {capXSystemPrompt}

            ADDITIONAL INSTRUCTIONS
            =======================

            {additionalSystemPrompt.Trim()}
            """;
    }
}