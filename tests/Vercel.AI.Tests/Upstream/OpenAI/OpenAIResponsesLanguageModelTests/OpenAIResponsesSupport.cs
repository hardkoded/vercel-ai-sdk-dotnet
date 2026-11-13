// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Fixtures and helpers shared by the <c>openai-responses-language-model.test.ts</c> ports.</summary>
internal static class OpenAIResponsesSupport
{
    internal static string Fixture(string name) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));

    // Parses a Responses body with predictable source ids: id-0, id-1, and so on.
    internal static LanguageModelGenerateResult Generate(JsonNode body, IReadOnlyList<LanguageModelTool>? tools = null, bool jsonResponse = false)
    {
        var raw = body.ToJsonString();
        using var document = JsonDocument.Parse(raw);
        var next = 0;
        return OpenAIResponsesLanguageModel.Parse(
            document.RootElement,
            Array.Empty<OpenAICallWarning>(),
            raw,
            new Dictionary<string, string>(),
            () => "id-" + next++,
            OpenAIParallelToolCall.FunctionToolNames(tools),
            jsonResponse: jsonResponse);
    }

    internal static JsonNode FixtureBody(string name) => JsonNode.Parse(Fixture(name))!;

    internal static string Sse(IEnumerable<JsonNode> events) =>
        string.Concat(events.Select(@event => "data: " + @event.ToJsonString() + "\n\n"));

    // The TEST_TOOLS of the upstream test file.
    internal static readonly IReadOnlyList<LanguageModelTool> TestTools = new[]
    {
        new LanguageModelTool(
            "weather",
            null,
            OpenAIUpstream.Json("""{"type":"object","properties":{"location":{"type":"string"}},"required":["location"],"additionalProperties":false}""")),
        new LanguageModelTool(
            "cityAttractions",
            null,
            OpenAIUpstream.Json("""{"type":"object","properties":{"city":{"type":"string"}},"required":["city"],"additionalProperties":false}""")),
    };

    internal static async Task<List<LanguageModelStreamPart>> Stream(string serverSentEvents, IReadOnlyList<LanguageModelTool>? tools = null)
    {
        var capture = new OpenAICapture { ServerSentEvents = serverSentEvents };
        return await OpenAIUpstream.Read(
            (await OpenAIUpstream.Provider(capture).ResponsesModel("gpt-5-nano").DoStreamAsync(WithTools(tools), CancellationToken.None))).ConfigureAwait(false);
    }

    private static LanguageModelCallOptions WithTools(IReadOnlyList<LanguageModelTool>? tools)
    {
        var options = OpenAIUpstream.Hello();
        options.Tools = tools;
        return options;
    }

    internal static JsonNode CitationsJson(IReadOnlyList<Citation>? citations) =>
        CitationJson.ToJson(citations ?? Array.Empty<Citation>());

    /// <summary>Asserts a stream against an upstream <c>toMatchSnapshot()</c> entry, which the port keeps as JSON in <c>Fixtures</c>.</summary>
    internal static void AssertStreamSnapshot(IReadOnlyList<LanguageModelStreamPart> parts, string fixture)
    {
        var expected = JsonNode.Parse(Fixture(fixture + ".stream.snap.json"))!.AsArray();
        var actual = new JsonArray();
        foreach (var part in parts)
        {
            actual.Add(SnapshotOf(part));
        }

        for (var index = 0; index < Math.Min(expected.Count, actual.Count); index++)
        {
            Assert.True(
                JsonNode.DeepEquals(expected[index], actual[index]),
                "Part " + index + ": expected " + expected[index]!.ToJsonString() + " but got " + actual[index]!.ToJsonString());
        }

        Assert.Equal(expected.Count, actual.Count);
    }

    private static JsonNode SnapshotOf(LanguageModelStreamPart part)
    {
        var node = new JsonObject { ["type"] = part.Type };
        switch (part)
        {
            case StreamStartStreamPart start:
                node["warnings"] = new JsonArray(start.Warnings.Select(warning => (JsonNode?)JsonValue.Create(warning.Message)).ToArray());
                break;
            case ResponseMetadataStreamPart metadata:
                node["id"] = metadata.Id;
                node["modelId"] = metadata.ModelId;
                node["timestamp"] = metadata.Timestamp!.Value.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", System.Globalization.CultureInfo.InvariantCulture);
                break;
            case TextStartStreamPart textStart:
                node["id"] = textStart.Id;
                SetMetadata(node, textStart.ProviderMetadata);
                break;
            case TextDeltaStreamPart textDelta:
                node["id"] = textDelta.Id;
                node["delta"] = textDelta.Delta;
                SetMetadata(node, textDelta.ProviderMetadata);
                break;
            case TextEndStreamPart textEnd:
                node["id"] = textEnd.Id;
                SetMetadata(node, textEnd.ProviderMetadata);
                break;
            case ReasoningStartStreamPart reasoningStart:
                node["id"] = reasoningStart.Id;
                SetMetadata(node, reasoningStart.ProviderMetadata);
                break;
            case ReasoningDeltaStreamPart reasoningDelta:
                node["id"] = reasoningDelta.Id;
                node["delta"] = reasoningDelta.Delta;
                SetMetadata(node, reasoningDelta.ProviderMetadata);
                break;
            case ReasoningEndStreamPart reasoningEnd:
                node["id"] = reasoningEnd.Id;
                SetMetadata(node, reasoningEnd.ProviderMetadata);
                break;
            case ToolInputStartStreamPart inputStart:
                node["id"] = inputStart.Id;
                node["toolName"] = inputStart.ToolName;
                SetMetadata(node, inputStart.ProviderMetadata);
                break;
            case ToolInputDeltaStreamPart inputDelta:
                node["id"] = inputDelta.Id;
                node["delta"] = inputDelta.Delta;
                break;
            case ToolInputEndStreamPart inputEnd:
                node["id"] = inputEnd.Id;
                break;
            case ToolCallStreamPart call:
                node["toolCallId"] = call.ToolCallId;
                node["toolName"] = call.ToolName;
                node["input"] = call.ArgumentsJson;
                SetMetadata(node, call.ProviderMetadata);
                break;
            case FinishStreamPart finish:
                var finishReason = new JsonObject { ["unified"] = KebabCase(finish.FinishReason.ToString()) };
                if (finish.RawFinishReason != null)
                {
                    finishReason["raw"] = finish.RawFinishReason;
                }

                node["finishReason"] = finishReason;
                SetMetadata(node, finish.ProviderMetadata);
                node["usage"] = UsageSnapshot(finish.Usage);
                break;
            default:
                throw new InvalidOperationException("Unexpected stream part " + part.Type);
        }

        return node;
    }

    private static void SetMetadata(JsonObject node, JsonElement? metadata)
    {
        if (metadata != null)
        {
            node["providerMetadata"] = JsonNode.Parse(metadata.Value.GetRawText());
        }
    }

    private static JsonObject UsageSnapshot(LanguageModelUsage usage)
    {
        var input = new JsonObject();
        SetNumber(input, "cacheRead", usage.CacheReadTokens);
        SetNumber(input, "cacheWrite", usage.CacheWriteTokens);
        SetNumber(input, "noCache", usage.NoCacheInputTokens);
        SetNumber(input, "total", usage.InputTokens);
        var output = new JsonObject();
        SetNumber(output, "reasoning", usage.ReasoningTokens);
        SetNumber(output, "text", usage.TextTokens);
        SetNumber(output, "total", usage.OutputTokens);
        var node = new JsonObject { ["inputTokens"] = input, ["outputTokens"] = output };
        if (usage.Raw != null)
        {
            node["raw"] = JsonNode.Parse(usage.Raw.Value.GetRawText());
        }

        return node;
    }

    private static void SetNumber(JsonObject node, string name, int? value)
    {
        if (value != null)
        {
            node[name] = value;
        }
    }

    private static string KebabCase(string name) =>
        string.Concat(name.Select((c, index) => char.IsUpper(c) ? (index > 0 ? "-" : string.Empty) + char.ToLowerInvariant(c) : c.ToString()));
}
