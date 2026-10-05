// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>Completion request body and warnings.</summary>
public sealed class OpenAICompletionPreparedRequest
{
    internal OpenAICompletionPreparedRequest(JsonObject body, IReadOnlyList<OpenAICallWarning> warnings)
    {
        Body = body;
        Warnings = warnings;
    }

    /// <summary>JSON body sent to <c>/completions</c>.</summary>
    public JsonObject Body { get; }

    /// <summary>Warnings produced while preparing the request.</summary>
    public IReadOnlyList<OpenAICallWarning> Warnings { get; }
}

/// <summary>OpenAI legacy completions language model.</summary>
public sealed class OpenAICompletionLanguageModel : ILanguageModel
{
    private readonly OpenAIProvider _provider;

    /// <summary>Creates a completion model.</summary>
    public OpenAICompletionLanguageModel(OpenAIProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => _provider.Name + ".completion";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>Builds the completions body. <paramref name="stream"/> adds streaming fields.</summary>
    public static OpenAICompletionPreparedRequest Prepare(string modelId, LanguageModelCallOptions options, bool stream)
    {
        options ??= new LanguageModelCallOptions();
        var warnings = new List<OpenAICallWarning>();
        var openai = OpenAIJson.Provider(options.ProviderOptions);
        if (options.TopK != null)
        {
            warnings.Add(new OpenAICallWarning("unsupported", "topK", null));
        }

        if (options.Tools is { Count: > 0 })
        {
            warnings.Add(new OpenAICallWarning("unsupported", "tools", null));
        }

        if (options.ToolChoice != null)
        {
            warnings.Add(new OpenAICallWarning("unsupported", "toolChoice", null));
        }

        if (options.JsonSchema != null)
        {
            warnings.Add(new OpenAICallWarning("unsupported", "responseFormat", "JSON response format is not supported."));
        }

        var prompt = ConvertPrompt(options.Prompt);
        var body = new JsonObject
        {
            ["model"] = modelId,
            ["prompt"] = prompt.Prompt,
        };
        if (OpenAIJson.Bool(openai, "echo") is { } echo)
        {
            body["echo"] = echo;
        }

        var logitBias = OpenAIJson.Node(OpenAIJson.Child(openai, "logitBias"));
        if (logitBias != null)
        {
            body["logit_bias"] = logitBias;
        }

        var logprobs = OpenAIJson.Child(openai, "logprobs");
        if (logprobs != null && logprobs.Value.ValueKind == JsonValueKind.True)
        {
            body["logprobs"] = 0;
        }
        else if (logprobs != null && logprobs.Value.ValueKind == JsonValueKind.Number)
        {
            body["logprobs"] = logprobs.Value.GetInt32();
        }

        var suffix = OpenAIJson.String(openai, "suffix");
        if (suffix != null)
        {
            body["suffix"] = suffix;
        }

        var user = OpenAIJson.String(openai, "user");
        if (user != null)
        {
            body["user"] = user;
        }

        if (options.MaxOutputTokens is { } max)
        {
            body["max_tokens"] = max;
        }

        if (options.Temperature is { } temperature)
        {
            body["temperature"] = temperature;
        }

        if (options.TopP is { } topP)
        {
            body["top_p"] = topP;
        }

        if (options.FrequencyPenalty is { } frequency)
        {
            body["frequency_penalty"] = frequency;
        }

        if (options.PresencePenalty is { } presence)
        {
            body["presence_penalty"] = presence;
        }

        if (options.Seed is { } seed)
        {
            body["seed"] = seed;
        }

        var stop = new JsonArray();
        foreach (var sequence in prompt.Stop)
        {
            stop.Add(sequence);
        }

        if (options.StopSequences != null)
        {
            foreach (var sequence in options.StopSequences)
            {
                stop.Add(sequence);
            }
        }

        if (stop.Count > 0)
        {
            body["stop"] = stop;
        }

        if (stream)
        {
            body["stream"] = true;
            body["stream_options"] = new JsonObject { ["include_usage"] = true };
        }

        return new OpenAICompletionPreparedRequest(body, warnings);
    }

    /// <summary>Converts messages into a completion prompt and the default stop sequence.</summary>
    public static OpenAICompletionPrompt ConvertPrompt(IReadOnlyList<ModelMessage> prompt)
    {
        var text = new StringBuilder();
        var index = 0;
        if (prompt.Count > 0 && prompt[0] is SystemModelMessage first)
        {
            text.Append(first.Content);
            text.Append("\n\n");
            index = 1;
        }

        for (var i = index; i < prompt.Count; i++)
        {
            var message = prompt[i];
            switch (message)
            {
                case SystemModelMessage:
                    throw new AiSdkException("Unexpected system message in prompt: ${content}");
                case UserModelMessage user:
                    var userText = new StringBuilder();
                    foreach (var part in user.Content)
                    {
                        if (part is TextContentPart textPart && !string.IsNullOrEmpty(textPart.Text))
                        {
                            userText.Append(textPart.Text);
                        }
                    }

                    text.Append("user:\n");
                    text.Append(userText);
                    text.Append("\n\n");
                    break;
                case AssistantModelMessage assistant:
                    if (assistant.ToolCalls.Count > 0)
                    {
                        throw new AiSdkException("tool-call messages");
                    }

                    text.Append("assistant:\n");
                    text.Append(assistant.Text ?? string.Empty);
                    text.Append("\n\n");
                    break;
                case ToolModelMessage:
                    throw new AiSdkException("tool messages");
                default:
                    throw new AiSdkException("Unsupported role: " + message.Role);
            }
        }

        text.Append("assistant:\n");
        return new OpenAICompletionPrompt(text.ToString(), new[] { "\nuser:" });
    }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var prepared = Prepare(ModelId, options, false);
        var response = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "completions"),
            prepared.Body.ToJsonString(),
            _provider.CreateOpenAIHeaders(options.Headers),
            cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        var root = document.RootElement;
        var choice = root.GetProperty("choices")[0];
        var text = choice.TryGetProperty("text", out var textElement) ? textElement.GetString() ?? string.Empty : string.Empty;
        var rawReason = choice.TryGetProperty("finish_reason", out var finish) && finish.ValueKind == JsonValueKind.String ? finish.GetString() : null;
        var metadata = new JsonObject();
        if (choice.TryGetProperty("logprobs", out var logprobs) && logprobs.ValueKind != JsonValueKind.Null)
        {
            metadata["logprobs"] = JsonNode.Parse(logprobs.GetRawText());
        }

        return new LanguageModelGenerateResult(
            new GeneratedContent[] { new GeneratedText(text) },
            FinishReasons.Parse(rawReason),
            OpenAIJson.CompletionUsage(root.TryGetProperty("usage", out var usage) ? usage : (JsonElement?)null),
            rawReason,
            ToCallWarnings(prepared.Warnings),
            StringOrNull(root, "id"),
            OpenAIJson.ProviderMetadata(metadata),
            response.Body,
            StringOrNull(root, "model"),
            OpenAIJson.UnixSeconds(OpenAIJson.Unix(root, "created")),
            response.Headers);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var prepared = Prepare(ModelId, options, true);
        yield return new StreamStartStreamPart(ToCallWarnings(prepared.Warnings));
        var started = false;
        var finish = FinishReason.Other;
        string? raw = null;
        LanguageModelUsage? usage = null;
        await foreach (var data in _provider.Http.SendSseAsync(
            ApiKeys.Combine(_provider.Options.BaseUrl, "completions"),
            prepared.Body.ToJsonString(),
            _provider.CreateOpenAIHeaders(options.Headers),
            cancellationToken).ConfigureAwait(false))
        {
            JsonDocument? document = null;
            var invalidJson = false;
            try
            {
                document = JsonDocument.Parse(data);
            }
            catch (JsonException)
            {
                invalidJson = true;
            }

            if (invalidJson || document == null)
            {
                yield return new ErrorStreamPart("JSON parsing failed: Text: " + data + ".");
                continue;
            }

            using (document)
            {
                var root = document.RootElement;
                if (root.TryGetProperty("usage", out var usageElement) && usageElement.ValueKind == JsonValueKind.Object)
                {
                    usage = OpenAIJson.CompletionUsage(usageElement);
                }

                if (!root.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
                {
                    continue;
                }

                var choice = choices[0];
                if (choice.TryGetProperty("text", out var textElement) && textElement.ValueKind == JsonValueKind.String)
                {
                    if (!started)
                    {
                        started = true;
                        yield return new TextStartStreamPart("0");
                    }

                    yield return new TextDeltaStreamPart("0", textElement.GetString() ?? string.Empty);
                }

                if (choice.TryGetProperty("finish_reason", out var finishElement) && finishElement.ValueKind == JsonValueKind.String)
                {
                    raw = finishElement.GetString();
                    finish = FinishReasons.Parse(raw);
                }
            }
        }

        if (started)
        {
            yield return new TextEndStreamPart("0");
        }

        yield return new FinishStreamPart(finish, usage ?? new LanguageModelUsage(null, null, null), raw);
    }

    private static string? StringOrNull(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var text = value.GetString();
        return string.IsNullOrEmpty(text) ? null : text;
    }

    private static List<CallWarning> ToCallWarnings(IReadOnlyList<OpenAICallWarning> warnings)
    {
        var result = new List<CallWarning>(warnings.Count);
        foreach (var warning in warnings)
        {
            result.Add(warning.ToCallWarning());
        }

        return result;
    }
}

/// <summary>A completion prompt and its default stop sequences.</summary>
public sealed class OpenAICompletionPrompt
{
    internal OpenAICompletionPrompt(string prompt, IReadOnlyList<string> stop)
    {
        Prompt = prompt;
        Stop = stop;
    }

    /// <summary>Prompt text.</summary>
    public string Prompt { get; }

    /// <summary>Stop sequences implied by the prompt.</summary>
    public IReadOnlyList<string> Stop { get; }
}
