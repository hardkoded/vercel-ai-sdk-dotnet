// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.OpenResponses;

/// <summary>Open Responses settings. The default origin is the AI Gateway Open Responses route.</summary>
public sealed class OpenResponsesOptions
{
    /// <summary>API origin. Requests are posted to <c>{BaseUrl}/responses</c>.</summary>
    public string BaseUrl { get; set; } = "https://ai-gateway.vercel.sh/v1";

    /// <summary>Explicit key.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Environment variable.</summary>
    public string ApiKeyEnvironmentVariable { get; set; } = "AI_GATEWAY_API_KEY";

    /// <summary>Provider name. It is the key for provider options and provider metadata. Maps to <c>name</c>.</summary>
    public string Name { get; set; } = OpenResponsesProvider.ProviderName;

    /// <summary>
    /// Whether to serialize assistant history using the strict OpenAI Responses input schemas. Assistant messages
    /// without an item ID are sent as easy input messages. Maps to <c>strictResponseInput</c>.
    /// </summary>
    public bool StrictResponseInput { get; set; }

    /// <summary>Extra headers sent with every request.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Reads HTTP status and retry metadata from an endpoint-specific response <c>error</c> object.
    /// Applies to both generation and streaming errors. Maps to <c>getResponseErrorMetadata</c>.
    /// </summary>
    public Func<JsonElement, OpenResponsesErrorMetadata?>? GetResponseErrorMetadata { get; set; }

    /// <summary>Codecs for Open Responses extension tools, items, and streaming events. Experimental.</summary>
    public IReadOnlyList<OpenResponsesExtension>? Extensions { get; set; }
}

/// <summary>Status and retry metadata read from an endpoint-specific response error.</summary>
public sealed class OpenResponsesErrorMetadata
{
    /// <summary>HTTP status of the error.</summary>
    public int? StatusCode { get; set; }

    /// <summary>Whether the call can be retried.</summary>
    public bool? IsRetryable { get; set; }
}

/// <summary>Open Responses provider.</summary>
public sealed class OpenResponsesProvider : ProviderBase
{
    /// <summary>Default provider name.</summary>
    public const string ProviderName = "open-responses";

    /// <summary>Creates a provider.</summary>
    public OpenResponsesProvider(HttpClient httpClient, OpenResponsesOptions? options = null)
        : base((options ?? new OpenResponsesOptions()).Name ?? ProviderName)
    {
        Options = options ?? new OpenResponsesOptions();
        ExtensionRegistry = OpenResponsesExtensions.CreateRegistry(Options.Extensions);
        Http = new ProviderHttp(httpClient ?? throw new ArgumentNullException(nameof(httpClient)));
    }

    /// <summary>Options.</summary>
    public OpenResponsesOptions Options { get; }

    /// <summary>The registered extensions, indexed.</summary>
    public OpenResponsesExtensionRegistry ExtensionRegistry { get; }

    /// <summary>HTTP helper.</summary>
    public ProviderHttp Http { get; }

    /// <summary>Creates a provider.</summary>
    public static OpenResponsesProvider Create(OpenResponsesOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new OpenResponsesProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId) => new OpenResponsesLanguageModel(this, modelId);
}

/// <summary>Posts to <c>/responses</c>.</summary>
public sealed class OpenResponsesLanguageModel : ILanguageModel
{
    private static readonly Dictionary<string, string> ReasoningEffortMap = new Dictionary<string, string>
    {
        { "minimal", "low" },
        { "low", "low" },
        { "medium", "medium" },
        { "high", "high" },
        { "xhigh", "xhigh" },
        { "max", "max" },
    };

    private readonly OpenResponsesProvider _provider;

    /// <summary>Creates a model.</summary>
    public OpenResponsesLanguageModel(OpenResponsesProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => _provider.Name + ".responses";

    /// <inheritdoc />
    public string ModelId { get; }

    private OpenResponsesExtensionRegistry Registry => _provider.ExtensionRegistry;

    /// <summary>
    /// Serializes the model id and config for a workflow boundary. Maps to <c>[WORKFLOW_SERIALIZE]</c>.
    /// Throws a <see cref="SerializationError"/> when extensions are registered, because their codecs cannot cross the boundary.
    /// </summary>
    public SerializedModelOptions Serialize()
    {
        if (Registry.ByExtensionId.Count > 0)
        {
            throw new SerializationError(
                "Open Responses models with registered extensions cannot be serialized across workflow boundaries. Recreate the provider with its extension codecs inside the workflow step.");
        }

        var config = new Dictionary<string, object?>
        {
            ["provider"] = Provider,
            ["providerOptionsName"] = _provider.Name,
            ["url"] = RequestUri().ToString(),
        };
        return ModelOptionsSerialization.SerializeModelOptions(ModelId, config);
    }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var (body, warnings) = Prepare(options);
        var uri = RequestUri();
        var http = await _provider.Http.SendJsonStringAsync(HttpMethod.Post, uri, body.ToJsonString(), Headers(options), cancellationToken).ConfigureAwait(false);
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(string.IsNullOrWhiteSpace(http.Body) ? "null" : http.Body);
        }
        catch (JsonException exception)
        {
            throw new ApiException("Invalid JSON response", 200, http.Body, exception);
        }

        using (document)
        {
            return Parse(document.RootElement, body, warnings, uri, http);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var (body, warnings) = Prepare(options);
        body["stream"] = true;
        var session = new OpenResponsesStream(_provider.Name, Registry, _provider.Options.GetResponseErrorMetadata);
        var enumerator = _provider.Http.SendSseAsync(RequestUri(), body.ToJsonString(), Headers(options), cancellationToken)
            .GetAsyncEnumerator(cancellationToken);
        try
        {
            var more = await enumerator.MoveNextAsync().ConfigureAwait(false);
            yield return new StreamStartStreamPart(warnings);
            while (more)
            {
                foreach (var part in session.Accept(enumerator.Current, options.IncludeRawChunks))
                {
                    yield return part;
                }

                more = await enumerator.MoveNextAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }

        foreach (var part in session.Flush())
        {
            yield return part;
        }
    }

    private (JsonObject Body, List<CallWarning> Warnings) Prepare(LanguageModelCallOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var warnings = new List<CallWarning>();
        if (options.StopSequences != null)
        {
            warnings.Add(new CallWarning("unsupported", "stopSequences"));
        }

        if (options.TopK.HasValue)
        {
            warnings.Add(new CallWarning("unsupported", "topK"));
        }

        if (options.Seed.HasValue)
        {
            warnings.Add(new CallWarning("unsupported", "seed"));
        }

        var providerToolsByName = new Dictionary<string, LanguageModelTool>(StringComparer.Ordinal);
        foreach (var tool in options.Tools ?? Array.Empty<LanguageModelTool>())
        {
            if (tool.IsProviderDefined)
            {
                providerToolsByName[tool.Name] = tool;
            }
        }

        var converted = OpenResponsesInput.Convert(options.Prompt, _provider.Name, _provider.Options.StrictResponseInput, Registry, providerToolsByName);
        warnings.AddRange(converted.Warnings);
        var encodedProviderTools = new Dictionary<string, OpenResponsesExtension>(StringComparer.Ordinal);

        var body = new JsonObject { ["model"] = ModelId, ["input"] = converted.Input };
        OpenAIJson.Set(body, "instructions", converted.Instructions);
        OpenAIJson.Set(body, "max_output_tokens", options.MaxOutputTokens);
        OpenAIJson.Set(body, "temperature", options.Temperature);
        OpenAIJson.Set(body, "top_p", options.TopP);
        OpenAIJson.Set(body, "presence_penalty", options.PresencePenalty);
        OpenAIJson.Set(body, "frequency_penalty", options.FrequencyPenalty);
        OpenAIJson.Set(body, "reasoning", Reasoning(options, warnings));
        OpenAIJson.Set(body, "tools", Tools(options.Tools, encodedProviderTools, warnings));
        OpenAIJson.Set(body, "tool_choice", ToolChoice(options.Tools, options.ToolChoice, encodedProviderTools, warnings));
        if (options.JsonSchema is { ValueKind: not JsonValueKind.Undefined and not JsonValueKind.Null } schema)
        {
            var format = new JsonObject
            {
                ["type"] = "json_schema",
                ["name"] = options.JsonSchemaName ?? "response",
            };
            OpenAIJson.Set(format, "description", options.JsonSchemaDescription);
            format["schema"] = JsonNode.Parse(schema.GetRawText());
            format["strict"] = true;
            body["text"] = new JsonObject { ["format"] = format };
        }
        else if (options.JsonResponse)
        {
            body["text"] = new JsonObject { ["format"] = new JsonObject { ["type"] = "json_object" } };
        }

        return (body, warnings);
    }

    private JsonObject? Reasoning(LanguageModelCallOptions options, List<CallWarning> warnings)
    {
        string? providerEffort = null;
        string? providerSummary = null;
        if (options.ProviderOptions != null
            && options.ProviderOptions.TryGetValue(_provider.Name, out var configured)
            && configured.ValueKind == JsonValueKind.Object)
        {
            providerEffort = OpenAIJson.String(configured, "reasoningEffort");
            providerSummary = OpenAIJson.String(configured, "reasoningSummary");
        }

        var effort = providerEffort;
        if (effort == null && ReasoningMap.IsCustomReasoning(options.Reasoning))
        {
            if (options.Reasoning == "none")
            {
                effort = "none";
            }
            else
            {
                var mappingWarnings = new List<ModelWarning>();
                effort = ReasoningMap.MapReasoningToProviderEffort(options.Reasoning!, ReasoningEffortMap, mappingWarnings);
                foreach (var warning in mappingWarnings)
                {
                    warnings.Add(new CallWarning(warning.Type, (warning as CompatibilityWarning)?.Details ?? (warning as UnsupportedWarning)?.Details ?? string.Empty));
                }
            }
        }

        if (effort == null && providerSummary == null)
        {
            return null;
        }

        var reasoning = new JsonObject();
        OpenAIJson.Set(reasoning, "effort", effort);
        OpenAIJson.Set(reasoning, "summary", providerSummary);
        return reasoning;
    }

    /// <summary>
    /// Converts function tools, and provider-defined tools through their extension. A provider tool that no extension
    /// encodes is dropped with a warning. <paramref name="encodedProviderTools"/> receives the encoded provider tools by name.
    /// </summary>
    private JsonArray? Tools(
        IReadOnlyList<LanguageModelTool>? tools,
        Dictionary<string, OpenResponsesExtension> encodedProviderTools,
        List<CallWarning> warnings)
    {
        if (tools == null || tools.Count == 0)
        {
            return null;
        }

        var converted = new JsonArray();
        foreach (var tool in tools)
        {
            if (tool.IsProviderDefined)
            {
                JsonObject? encoded = null;
                if (Registry.ByProviderToolId.TryGetValue(tool.ProviderToolId!, out var extension))
                {
                    try
                    {
                        if (extension.EncodeTool!(new OpenResponsesToolContext(tool.Name, tool.Args)) is { } fields)
                        {
                            encoded = WithType(fields, extension.ToolType!);
                        }
                    }
                    catch (Exception)
                    {
                        // Encoding failures are reported as unsupported below.
                    }
                }

                if (encoded == null)
                {
                    warnings.Add(new CallWarning("unsupported", "provider-defined tool " + tool.ProviderToolId));
                }
                else
                {
                    converted.Add(encoded);
                    encodedProviderTools[tool.Name] = extension!;
                }

                continue;
            }

            var function = new JsonObject { ["type"] = "function", ["name"] = tool.Name };
            OpenAIJson.Set(function, "description", tool.Description);
            function["parameters"] = OpenAIJson.Node(tool.InputSchema);
            if (tool.Strict is { } strict)
            {
                function["strict"] = strict;
            }

            converted.Add(function);
        }

        return converted.Count > 0 ? converted : null;
    }

    /// <summary>
    /// Converts the tool choice. A named provider tool is encoded through its extension, and omitted when that tool was not
    /// encoded or its extension cannot encode the choice.
    /// </summary>
    private static JsonNode? ToolChoice(
        IReadOnlyList<LanguageModelTool>? tools,
        ToolChoice? toolChoice,
        Dictionary<string, OpenResponsesExtension> encodedProviderTools,
        List<CallWarning> warnings)
    {
        if (toolChoice is not ToolChoice.NamedChoice named)
        {
            return toolChoice?.Type;
        }

        var tool = tools?.FirstOrDefault(candidate => candidate.IsProviderDefined && candidate.Name == named.ToolName);
        if (tool == null)
        {
            return new JsonObject { ["type"] = "function", ["name"] = named.ToolName };
        }

        if (!encodedProviderTools.TryGetValue(named.ToolName, out var extension))
        {
            return null;
        }

        JsonObject? fields = new JsonObject();
        if (extension.EncodeToolChoice != null)
        {
            try
            {
                fields = extension.EncodeToolChoice(new OpenResponsesToolContext(tool.Name, tool.Args));
            }
            catch (Exception)
            {
                fields = null;
            }

            if (fields == null)
            {
                warnings.Add(new CallWarning("unsupported", "tool choice for provider-defined tool " + tool.ProviderToolId));
                return null;
            }
        }

        return WithType(fields!, extension.ToolType!);
    }

    private static JsonObject WithType(JsonObject fields, string type)
    {
        var result = new JsonObject { ["type"] = type };
        foreach (var pair in fields)
        {
            if (pair.Key != "type")
            {
                result[pair.Key] = pair.Value?.DeepClone();
            }
        }

        return result;
    }

    private LanguageModelGenerateResult Parse(
        JsonElement response,
        JsonObject body,
        List<CallWarning> warnings,
        Uri uri,
        ProviderTextResponse http)
    {
        if (OpenAIJson.Child(response, "error") is { } error)
        {
            var errorMetadata = _provider.Options.GetResponseErrorMetadata?.Invoke(error);
            var statusCode = errorMetadata?.StatusCode ?? 400;
            throw new ApiException(OpenAIJson.String(error, "message") ?? string.Empty, statusCode, http.Body)
            {
                Url = uri.ToString(),
                RequestBodyValues = body,
                ResponseHeaders = http.Headers,
                IsRetryable = errorMetadata?.IsRetryable ?? (statusCode == 408 || statusCode == 409 || statusCode == 429 || statusCode >= 500),
            };
        }

        if (OpenAIJson.Child(response, "output") is not { ValueKind: JsonValueKind.Array } output)
        {
            var detail = OpenAIJson.String(OpenAIJson.Child(response, "incomplete_details"), "reason") ?? OpenAIJson.String(response, "status");
            throw new ApiException(
                detail != null ? "Responses API returned no output (" + detail + ")" : "Responses API returned no output",
                500,
                http.Body)
            {
                Url = uri.ToString(),
                RequestBodyValues = body,
                ResponseHeaders = http.Headers,
                IsRetryable = false,
            };
        }

        var content = new List<GeneratedContent>();
        var hasToolCalls = false;
        foreach (var part in output.EnumerateArray())
        {
            switch (OpenAIJson.String(part, "type"))
            {
                case "reasoning":
                    if (OpenAIJson.Child(part, "content") is { ValueKind: JsonValueKind.Array } reasoningContent && reasoningContent.GetArrayLength() > 0)
                    {
                        foreach (var contentPart in reasoningContent.EnumerateArray())
                        {
                            content.Add(new GeneratedReasoning(
                                OpenAIJson.String(contentPart, "text") ?? string.Empty,
                                OpenResponsesResults.ReasoningMetadata(_provider.Name, part, new[] { contentPart })));
                        }
                    }
                    else
                    {
                        var summary = new List<string>();
                        if (OpenAIJson.Child(part, "summary") is { ValueKind: JsonValueKind.Array } summaryParts)
                        {
                            foreach (var summaryPart in summaryParts.EnumerateArray())
                            {
                                summary.Add(OpenAIJson.String(summaryPart, "text") ?? string.Empty);
                            }
                        }

                        content.Add(new GeneratedReasoning(string.Concat(summary), OpenResponsesResults.ReasoningMetadata(_provider.Name, part)));
                    }

                    break;
                case "message":
                    if (OpenAIJson.Child(part, "content") is { ValueKind: JsonValueKind.Array } messageContent)
                    {
                        foreach (var contentPart in messageContent.EnumerateArray())
                        {
                            content.Add(new GeneratedText(
                                OpenAIJson.String(contentPart, "text") ?? string.Empty,
                                providerMetadata: OpenResponsesResults.MessageMetadata(_provider.Name, OpenAIJson.String(part, "id"), OpenResponsesResults.Annotations(contentPart))));
                        }
                    }

                    break;
                case "function_call":
                    hasToolCalls = true;
                    content.Add(new GeneratedToolCall(
                        OpenAIJson.String(part, "call_id") ?? string.Empty,
                        OpenAIJson.String(part, "name") ?? string.Empty,
                        OpenAIJson.String(part, "arguments") ?? string.Empty,
                        OpenResponsesResults.ItemMetadata(_provider.Name, OpenAIJson.String(part, "id"))));
                    break;
                default:
                    if (OpenAIJson.Node(part) is JsonObject item
                        && OpenResponsesJson.IsExtensionItem(item)
                        && OpenResponsesExtensionItems.Decode(Registry, _provider.Name, item, OpenResponsesItemMode.Generate) is { } decoded)
                    {
                        content.AddRange(decoded);
                        hasToolCalls |= decoded.Any(decodedPart => decodedPart is GeneratedToolCall);
                    }

                    break;
            }
        }

        var rawFinishReason = OpenAIJson.String(OpenAIJson.Child(response, "incomplete_details"), "reason");
        return new LanguageModelGenerateResult(
            content,
            OpenResponsesFinishReason.Map(rawFinishReason, hasToolCalls),
            OpenAIJson.Child(response, "usage") is { ValueKind: JsonValueKind.Object } usage ? OpenResponsesResults.Usage(usage) : LanguageModelUsage.Null,
            rawFinishReason,
            warnings,
            OpenAIJson.String(response, "id"),
            rawResponse: http.Body,
            responseModelId: OpenAIJson.String(response, "model"),
            responseTimestamp: OpenAIJson.UnixSeconds(OpenAIJson.Unix(response, "created_at")),
            responseHeaders: http.Headers);
    }

    private Uri RequestUri()
    {
        return ApiKeys.Combine(_provider.Options.BaseUrl, "responses");
    }

    private Dictionary<string, string?> Headers(LanguageModelCallOptions options)
    {
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["Authorization"] = "Bearer " + ApiKeys.Require(_provider.Options.ApiKey, _provider.Options.ApiKeyEnvironmentVariable),
        };
        foreach (var pair in _provider.Options.Headers ?? new Dictionary<string, string>())
        {
            headers[pair.Key] = pair.Value;
        }

        if (options.Headers != null)
        {
            foreach (var pair in options.Headers)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return headers;
    }
}

/// <summary>Registers Open Responses.</summary>
public static class OpenResponsesServiceCollectionExtensions
{
    /// <summary>Adds <see cref="OpenResponsesProvider"/>.</summary>
    public static IServiceCollection AddOpenResponses(this IServiceCollection services, Action<OpenResponsesOptions>? configure = null)
    {
        services.AddHttpClient(OpenResponsesProvider.ProviderName);
        services.AddSingleton(sp =>
        {
            var options = new OpenResponsesOptions();
            configure?.Invoke(options);
            return new OpenResponsesProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(OpenResponsesProvider.ProviderName), options);
        });
        return services;
    }
}
