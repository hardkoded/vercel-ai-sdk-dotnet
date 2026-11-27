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
    internal static string Fixture(string name) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));

    // Parses a Responses body with predictable source ids: id-0, id-1, and so on.
    internal static LanguageModelGenerateResult Generate(
        JsonNode body,
        IReadOnlyList<LanguageModelTool>? tools = null,
        bool jsonResponse = false
    )
    {
        var raw = body.ToJsonString();
        using var document = JsonDocument.Parse(raw);
        var next = 0;
        return OpenAIResponsesLanguageModel.Parse(
            OpenAIResponsesSchema.ParseResponse(document.RootElement)!,
            Array.Empty<OpenAICallWarning>(),
            raw,
            new Dictionary<string, string>(),
            () => "id-" + next++,
            OpenAIParallelToolCall.FunctionToolNames(tools),
            toolNameMapping: OpenAIResponsesLanguageModel.CreateToolNameMapping(tools),
            jsonResponse: jsonResponse,
            webSearchToolName: OpenAIResponsesLanguageModel.WebSearchToolName(tools)
        );
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
            OpenAIUpstream.Json(
                """{"type":"object","properties":{"location":{"type":"string"}},"required":["location"],"additionalProperties":false}"""
            )
        ),
        new LanguageModelTool(
            "cityAttractions",
            null,
            OpenAIUpstream.Json(
                """{"type":"object","properties":{"city":{"type":"string"}},"required":["city"],"additionalProperties":false}"""
            )
        ),
    };

    internal static async Task<List<LanguageModelStreamPart>> Stream(
        string serverSentEvents,
        IReadOnlyList<LanguageModelTool>? tools = null
    )
    {
        var capture = new OpenAICapture { ServerSentEvents = serverSentEvents };
        return await OpenAIUpstream
            .Read(
                (
                    await OpenAIUpstream
                        .Provider(capture)
                        .ResponsesModel("gpt-5-nano")
                        .DoStreamAsync(WithTools(tools), CancellationToken.None)
                )
            )
            .ConfigureAwait(false);
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
    internal static void AssertStreamSnapshot(
        IReadOnlyList<LanguageModelStreamPart> parts,
        string fixture,
        Func<string, string>? sourceId = null,
        bool includeCitations = false
    )
    {
        sourceId ??= static id => id;
        var expected = JsonNode.Parse(Fixture(fixture + ".stream.snap.json"))!.AsArray();
        var actual = new JsonArray();
        foreach (var part in parts)
        {
            actual.Add(SnapshotOf(part, sourceId, includeCitations));
        }

        for (var index = 0; index < Math.Min(expected.Count, actual.Count); index++)
        {
            Assert.True(
                JsonNode.DeepEquals(expected[index], actual[index]),
                "Part "
                    + index
                    + ": expected "
                    + expected[index]!.ToJsonString()
                    + " but got "
                    + actual[index]!.ToJsonString()
            );
        }

        Assert.Equal(expected.Count, actual.Count);
    }

    /// <summary>Asserts generated content against an upstream <c>toMatchSnapshot()</c> entry, which the port keeps as JSON in <c>Fixtures</c>.</summary>
    internal static void AssertContentSnapshot(
        IReadOnlyList<GeneratedContent> content,
        string fixture,
        bool includeCitations = false,
        Func<string, string>? sourceId = null
    )
    {
        AssertContent(
            content,
            JsonNode.Parse(Fixture(fixture + ".generate.snap.json"))!.AsArray(),
            includeCitations,
            sourceId
        );
    }

    /// <summary>Asserts generated content against an upstream inline snapshot, written as JSON.</summary>
    internal static void AssertContent(
        IReadOnlyList<GeneratedContent> content,
        JsonArray expected,
        bool includeCitations = false,
        Func<string, string>? sourceId = null
    )
    {
        sourceId ??= static id => id;
        var actual = new JsonArray();
        foreach (var part in content)
        {
            var node = new JsonObject { ["type"] = part.Type };
            switch (part)
            {
                case GeneratedText text:
                    node["text"] = text.Text;
                    SetMetadata(
                        node,
                        (text as OpenAIText)?.ProviderMetadata ?? text.ProviderMetadata
                    );
                    if (includeCitations && text.Citations is { Count: > 0 })
                    {
                        node["citations"] = CitationsJson(text.Citations);
                    }

                    break;
                case GeneratedSource source:
                    node["sourceType"] = "url";
                    node["id"] = sourceId(source.Id);
                    node["url"] = source.Url;
                    if (source.Title != null)
                    {
                        node["title"] = source.Title;
                    }

                    SetMetadata(node, source.ProviderMetadata);
                    break;
                case GeneratedDocumentSource documentSource:
                    node["sourceType"] = "document";
                    node["id"] = sourceId(documentSource.Id);
                    node["mediaType"] = documentSource.MediaType;
                    node["title"] = documentSource.Title;
                    if (documentSource.Filename != null)
                    {
                        node["filename"] = documentSource.Filename;
                    }

                    SetMetadata(node, documentSource.ProviderMetadata);
                    break;
                case GeneratedReasoning reasoning:
                    node["text"] = reasoning.Text;
                    SetMetadata(node, reasoning.ProviderMetadata);
                    break;
                case GeneratedToolCall call:
                    node["toolCallId"] = call.ToolCallId;
                    node["toolName"] = call.ToolName;
                    node["input"] = call.ArgumentsJson;
                    if (call.ProviderExecuted)
                    {
                        node["providerExecuted"] = true;
                    }

                    if (call.Dynamic)
                    {
                        node["dynamic"] = true;
                    }

                    SetMetadata(node, call.ProviderMetadata);
                    break;
                case GeneratedToolResult result:
                    node["toolCallId"] = result.ToolCallId;
                    node["toolName"] = result.ToolName;
                    node["result"] = JsonNode.Parse(result.ResultJson);
                    SetMetadata(node, result.ProviderMetadata);
                    break;
                case GeneratedCustom custom:
                    node["kind"] = custom.Kind;
                    SetMetadata(node, custom.ProviderMetadata);
                    break;
                case GeneratedToolApprovalRequest approvalRequest:
                    node["approvalId"] = approvalRequest.ApprovalId;
                    node["toolCallId"] = approvalRequest.ToolCallId;
                    break;
                default:
                    throw new InvalidOperationException("Unexpected content " + part.Type);
            }

            actual.Add(node);
        }

        Assert.True(
            JsonNode.DeepEquals(expected, actual),
            "Expected " + expected.ToJsonString() + " but got " + actual.ToJsonString()
        );
    }

    private static JsonNode SnapshotOf(
        LanguageModelStreamPart part,
        Func<string, string> sourceId,
        bool includeCitations
    )
    {
        var node = new JsonObject { ["type"] = part.Type };
        switch (part)
        {
            case SourceStreamPart source:
                node["sourceType"] = "url";
                node["id"] = sourceId(source.Id);
                node["url"] = source.Url;
                if (source.Title != null)
                {
                    node["title"] = source.Title;
                }

                SetMetadata(node, source.ProviderMetadata);
                break;
            case DocumentSourceStreamPart documentSource:
                node["sourceType"] = "document";
                node["id"] = sourceId(documentSource.Id);
                node["mediaType"] = documentSource.MediaType;
                node["title"] = documentSource.Title;
                if (documentSource.Filename != null)
                {
                    node["filename"] = documentSource.Filename;
                }

                SetMetadata(node, documentSource.ProviderMetadata);
                break;
            case StreamStartStreamPart start:
                node["warnings"] = new JsonArray(
                    start
                        .Warnings.Select(warning => (JsonNode?)JsonValue.Create(warning.Message))
                        .ToArray()
                );
                break;
            case ResponseMetadataStreamPart metadata:
                node["id"] = metadata.Id;
                node["modelId"] = metadata.ModelId;
                node["timestamp"] = metadata.Timestamp!.Value.UtcDateTime.ToString(
                    "yyyy-MM-dd'T'HH:mm:ss.fff'Z'",
                    System.Globalization.CultureInfo.InvariantCulture
                );
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
                if (includeCitations && textEnd.Citations is { Count: > 0 })
                {
                    node["citations"] = CitationsJson(textEnd.Citations);
                }

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
                if (inputStart.ProviderExecuted == true)
                {
                    node["providerExecuted"] = true;
                }

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
                if (call.ProviderExecuted)
                {
                    node["providerExecuted"] = true;
                }

                if (call.Dynamic)
                {
                    node["dynamic"] = true;
                }

                SetMetadata(node, call.ProviderMetadata);
                break;
            case ToolResultStreamPart toolResult:
                node["toolCallId"] = toolResult.ToolCallId;
                node["toolName"] = toolResult.ToolName;
                node["result"] = JsonNode.Parse(toolResult.Result.GetRawText());
                if (toolResult.Preliminary == true)
                {
                    node["preliminary"] = true;
                }

                SetMetadata(node, toolResult.ProviderMetadata);
                break;
            case ToolApprovalRequestStreamPart approvalRequest:
                node["approvalId"] = approvalRequest.ApprovalId;
                node["toolCallId"] = approvalRequest.ToolCallId;
                break;
            case CustomStreamPart custom:
                node["kind"] = custom.Kind;
                SetMetadata(node, custom.ProviderMetadata);
                break;
            case FinishStreamPart finish:
                var finishReason = new JsonObject
                {
                    ["unified"] = KebabCase(finish.FinishReason.ToString()),
                };
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
        string.Concat(
            name.Select(
                (c, index) =>
                    char.IsUpper(c)
                        ? (index > 0 ? "-" : string.Empty) + char.ToLowerInvariant(c)
                        : c.ToString()
            )
        );
}
