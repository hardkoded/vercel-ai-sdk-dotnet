// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Google;

/// <summary>Maps an Interactions API status onto a finish reason.</summary>
public static class GoogleInteractionsFinish
{
    /// <summary>
    /// <c>completed</c> is a tool call when the response contains a client function call.
    /// <c>requires_action</c> is always a tool call.
    /// </summary>
    public static FinishReason Map(string? status, bool hasFunctionCall)
    {
        switch (status)
        {
            case "completed":
                return hasFunctionCall ? FinishReason.ToolCalls : FinishReason.Stop;
            case "requires_action":
                return FinishReason.ToolCalls;
            case "failed":
                return FinishReason.Error;
            case "incomplete":
                return FinishReason.Length;
            default:
                return FinishReason.Other;
        }
    }
}

/// <summary>Converts a prompt into an Interactions <c>input</c> array.</summary>
public static class GoogleInteractionsInput
{
    /// <summary>Converts shared messages. System text is joined into <c>systemInstruction</c>.</summary>
    public static JsonObject Convert(IReadOnlyList<ModelMessage> prompt)
    {
        var system = new List<string>();
        var input = new JsonArray();
        if (prompt != null)
        {
            foreach (var message in prompt)
            {
                if (message is SystemModelMessage systemMessage)
                {
                    system.Add(systemMessage.Content);
                }
                else if (message is UserModelMessage user)
                {
                    var content = new JsonArray();
                    foreach (var part in user.Content)
                    {
                        if (part is TextContentPart text)
                        {
                            content.Add(new JsonObject { ["type"] = "text", ["text"] = text.Text });
                        }
                    }

                    input.Add(new JsonObject { ["type"] = "user_input", ["content"] = content });
                }
                else if (message is AssistantModelMessage assistant)
                {
                    if (!string.IsNullOrEmpty(assistant.Text))
                    {
                        input.Add(new JsonObject
                        {
                            ["type"] = "model_output",
                            ["content"] = new JsonArray { new JsonObject { ["type"] = "text", ["text"] = assistant.Text } },
                        });
                    }

                    foreach (var call in assistant.ToolCalls)
                    {
                        input.Add(new JsonObject
                        {
                            ["type"] = "function_call",
                            ["name"] = call.ToolName,
                            ["id"] = call.ToolCallId,
                            ["arguments"] = GoogleJson.ParseObject(call.ArgumentsJson),
                        });
                    }
                }
                else if (message is ToolModelMessage tool)
                {
                    input.Add(new JsonObject
                    {
                        ["type"] = "user_input",
                        ["content"] = new JsonArray
                        {
                            new JsonObject
                            {
                                ["type"] = "function_result",
                                ["name"] = tool.ToolName,
                                ["call_id"] = tool.ToolCallId,
                                ["result"] = GoogleJson.ParseObject(tool.OutputJson),
                            },
                        },
                    });
                }
            }
        }

        var body = new JsonObject { ["input"] = input };
        if (system.Count > 0)
        {
            body["systemInstruction"] = string.Join("\n\n", system);
        }

        return body;
    }
}

/// <summary>Reads text and function calls from an Interactions response.</summary>
public static class GoogleInteractionsOutput
{
    /// <summary>Parses <c>steps</c> into generated content and reports whether a client function call is present.</summary>
    public static (IReadOnlyList<GeneratedContent> Content, bool HasFunctionCall) Parse(JsonObject response)
    {
        var content = new List<GeneratedContent>();
        var function = false;
        if (response["steps"] is JsonArray steps)
        {
            foreach (var stepNode in steps)
            {
                if (stepNode is not JsonObject step)
                {
                    continue;
                }

                var type = (string?)step["type"];
                if (type == "function_call")
                {
                    function = true;
                    content.Add(new GeneratedToolCall((string?)step["id"] ?? "call", (string?)step["name"] ?? string.Empty, step["arguments"]?.ToJsonString() ?? "{}"));
                }

                if (step["content"] is JsonArray parts)
                {
                    foreach (var partNode in parts)
                    {
                        if (partNode is not JsonObject part)
                        {
                            continue;
                        }

                        if ((string?)part["type"] == "text")
                        {
                            content.Add(new GeneratedText((string?)part["text"] ?? string.Empty));
                        }
                        else if ((string?)part["type"] == "function_call")
                        {
                            function = true;
                            content.Add(new GeneratedToolCall((string?)part["id"] ?? "call", (string?)part["name"] ?? string.Empty, part["arguments"]?.ToJsonString() ?? "{}"));
                        }
                    }
                }
            }
        }

        return (content, function);
    }
}

/// <summary>Language model that posts to the Gemini Interactions API.</summary>
public sealed class GoogleInteractionsModel : ILanguageModel
{
    private readonly GoogleProvider _provider;

    /// <summary>Creates an interactions model.</summary>
    public GoogleInteractionsModel(GoogleProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion
    {
        get { return "V4"; }
    }

    /// <inheritdoc />
    public string Provider
    {
        get
        {
            return _provider.ModelProvider.IndexOf("vertex", StringComparison.OrdinalIgnoreCase) >= 0
                ? "google.vertex.interactions"
                : _provider.ModelProvider + ".interactions";
        }
    }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var input = GoogleInteractionsInput.Convert(options.Prompt);
        input["model"] = ModelId;
        var background = Background(options);
        if (background)
        {
            input["background"] = true;
        }

        if (options.MaxOutputTokens is { } maxOutputTokens)
        {
            input["generation_config"] = new JsonObject { ["max_output_tokens"] = maxOutputTokens };
        }

        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        var baseUrl = InteractionsBase(_provider.Options.BaseUrl);
        var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            ProviderUtils.ApiKeys.Combine(baseUrl, "interactions"),
            GoogleJson.Write(input),
            headers,
            cancellationToken).ConfigureAwait(false);

        // A background call (such as Gemini 4 long decode, where the server runs every hop) may return a non-terminal
        // status. Poll GET /interactions/{id} until it is terminal so the result matches a synchronous call.
        if (background)
        {
            document = await PollUntilTerminalAsync(document, baseUrl, headers, cancellationToken).ConfigureAwait(false);
        }

        using (document)
        {
            var response = JsonNode.Parse(document.RootElement.GetRawText()) as JsonObject ?? new JsonObject();
            var parsed = GoogleInteractionsOutput.Parse(response);
            var status = (string?)response["status"];
            return new LanguageModelGenerateResult(parsed.Content, GoogleInteractionsFinish.Map(status, parsed.HasFunctionCall), Usage(response["usage"] as JsonObject), status);
        }
    }

    private static bool Background(LanguageModelCallOptions options)
    {
        return options.ProviderOptions != null
            && options.ProviderOptions.TryGetValue("google", out var google)
            && google.ValueKind == JsonValueKind.Object
            && google.TryGetProperty("background", out var flag)
            && flag.ValueKind == JsonValueKind.True;
    }

    private static bool IsTerminal(string? status)
    {
        return status == "completed" || status == "failed" || status == "cancelled" || status == "incomplete";
    }

    private async Task<JsonDocument> PollUntilTerminalAsync(JsonDocument first, string baseUrl, IReadOnlyDictionary<string, string?> headers, CancellationToken cancellationToken)
    {
        var current = first;
        var delay = TimeSpan.FromSeconds(1);
        var deadline = DateTime.UtcNow + TimeSpan.FromMinutes(30);
        try
        {
            while (true)
            {
                var status = current.RootElement.TryGetProperty("status", out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
                if (IsTerminal(status))
                {
                    return current;
                }

                var id = current.RootElement.TryGetProperty("id", out var idValue) && idValue.ValueKind == JsonValueKind.String ? idValue.GetString() : null;
                if (string.IsNullOrEmpty(id))
                {
                    throw new InvalidOperationException("The Interactions response has no id to poll.");
                }

                if (DateTime.UtcNow + delay > deadline)
                {
                    throw new TimeoutException("Polling the Interactions response for '" + id + "' timed out.");
                }

                current.Dispose();
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 10));
                current = await _provider.Http.SendJsonAsync(
                    HttpMethod.Get,
                    ProviderUtils.ApiKeys.Combine(baseUrl, "interactions/" + Uri.EscapeDataString(id!)),
                    null,
                    headers,
                    cancellationToken).ConfigureAwait(false);
            }
        }
        catch
        {
            current.Dispose();
            throw;
        }
    }

    // total_output_tokens is the text count. Thought tokens add to the output total.
    private static LanguageModelUsage Usage(JsonObject? usage)
    {
        if (usage == null)
        {
            return LanguageModelUsage.Empty;
        }

        var input = Count(usage, "total_input_tokens");
        var output = Count(usage, "total_output_tokens");
        var thought = Count(usage, "total_thought_tokens");
        var cached = Count(usage, "total_cached_tokens");
        int? outputTotal = output == null && thought == null ? null : (output ?? 0) + (thought ?? 0);
        int? noCache = input == null ? null : input - (cached ?? 0);
        var raw = JsonSerializer.SerializeToElement(usage);
        return new LanguageModelUsage(input, outputTotal, input == null && outputTotal == null ? null : (input ?? 0) + (outputTotal ?? 0), cached, null, thought, raw, noCache, output);
    }

    private static int? Count(JsonObject usage, string name)
    {
        return usage[name] is JsonValue value && value.TryGetValue<int>(out var count) ? count : null;
    }

    /// <inheritdoc />
    public Task<LanguageModelStreamResult> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        return Task.FromResult(new LanguageModelStreamResult(StreamAsync(options, cancellationToken)));
    }

    private async IAsyncEnumerable<LanguageModelStreamPart> StreamAsync(LanguageModelCallOptions options, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var result = await DoGenerateAsync(options, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(result.Text))
        {
            yield return new TextDeltaStreamPart("text", result.Text);
        }

        yield return new FinishStreamPart(result.FinishReason, result.Usage, result.RawFinishReason);
    }

    private static string InteractionsBase(string? baseUrl)
    {
        var value = baseUrl ?? string.Empty;
        const string suffix = "/publishers/google";
        if (value.EndsWith(suffix, StringComparison.Ordinal))
        {
            return value.Substring(0, value.Length - suffix.Length);
        }

        return value;
    }
}
