// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Azure;
using Vercel.AI.Provider;
using Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Helpers of the <c>azure-openai-provider.test.ts</c> &gt; <c>responses</c> &gt; <c>doGenerate</c> tool tests.</summary>
internal static class AzureResponsesToolSupport
{
    internal const string Url = "https://test-resource.openai.azure.com/openai/v1/responses";

    internal static string Fixture(string name) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));

    internal static JsonElement Json(string json) => OpenAIUpstream.Json(json);

    internal static AzureOpenAIProvider Provider(OpenAICapture capture) =>
        AzureOpenAIProvider.Create(
            new AzureOpenAIOptions { ApiKey = "test-api-key", ResourceName = "test-resource" },
            capture
        );

    internal static LanguageModelCallOptions Call(
        IReadOnlyList<LanguageModelTool>? tools = null,
        IReadOnlyList<ModelMessage>? extra = null,
        string? azureOptions = null,
        string optionsKey = "azure"
    )
    {
        var prompt = new List<ModelMessage> { new UserModelMessage("Hello") };
        if (extra != null)
        {
            prompt.AddRange(extra);
        }

        return new LanguageModelCallOptions
        {
            Prompt = prompt,
            Tools = tools,
            ProviderOptions =
                azureOptions == null
                    ? null
                    : new Dictionary<string, JsonElement> { [optionsKey] = Json(azureOptions) },
        };
    }

    internal static async Task<LanguageModelGenerateResult> Generate(
        OpenAICapture capture,
        LanguageModelCallOptions options
    ) =>
        await Provider(capture)
            .ResponsesModel("test-deployment")
            .DoGenerateAsync(options, CancellationToken.None);

    internal static async Task<List<LanguageModelStreamPart>> Stream(
        OpenAICapture capture,
        LanguageModelCallOptions options
    ) =>
        await OpenAIUpstream.Read(
            await Provider(capture)
                .ResponsesModel("test-deployment")
                .DoStreamAsync(options, CancellationToken.None)
        );

    internal static JsonNode RequestBody(OpenAICapture capture) => JsonNode.Parse(capture.Body)!;

    internal static void AssertTools(OpenAICapture capture, string expected) =>
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expected), RequestBody(capture)["tools"]),
            RequestBody(capture)["tools"]!.ToJsonString()
        );

    internal static string Sse(IEnumerable<JsonNode> events) =>
        string.Concat(events.Select(@event => "data: " + @event.ToJsonString() + "\n\n"));

    internal static readonly IReadOnlyDictionary<string, string> ProviderAndRequestHeaders =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["api-key"] = "test-api-key",
            ["Custom-Provider-Header"] = "provider-header-value",
            ["Custom-Request-Header"] = "request-header-value",
        };

    /// <summary>
    /// The <c>requestHeaders</c> snapshot of the upstream tests, plus the <c>requestUserAgent</c> check. The test server of
    /// upstream hides the user agent and the content length from the snapshot, so they are not compared here.
    /// </summary>
    internal static void AssertRequestHeaders(
        IReadOnlyDictionary<string, string> actual,
        IReadOnlyDictionary<string, string> expected
    )
    {
        var hidden = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "user-agent",
            "content-length",
            "content-type",
        };
        foreach (var pair in expected)
        {
            Assert.Equal(pair.Value, actual[pair.Key]);
        }

        Assert.StartsWith("application/json", actual["content-type"]);
        foreach (var key in actual.Keys)
        {
            Assert.True(
                expected.ContainsKey(key) || hidden.Contains(key),
                "Unexpected request header " + key
            );
        }

        Assert.Contains(
            "ai-sdk-azure/" + Vercel.AI.Operations.AiSdkVersion.Version,
            actual["user-agent"]
        );
    }

    internal static IReadOnlyList<LanguageModelTool> ProviderTool(
        string id,
        string name,
        string args = "{}"
    ) => new[] { LanguageModelTool.ProviderDefined(id, name, Json(args)) };

    /// <summary>Port of <c>prepareJsonFixtureResponse</c>.</summary>
    internal static OpenAICapture JsonFixture(
        string name,
        IReadOnlyDictionary<string, string>? headers = null
    )
    {
        // The test server of upstream serves the parsed fixture, so the body is compact JSON.
        var capture = new OpenAICapture
        {
            ResponseJson = JsonNode
                .Parse(Fixture(name + ".json"))!
                .ToJsonString(
                    new JsonSerializerOptions
                    {
                        Encoder = System
                            .Text
                            .Encodings
                            .Web
                            .JavaScriptEncoder
                            .UnsafeRelaxedJsonEscaping,
                    }
                ),
        };
        foreach (var pair in headers ?? new Dictionary<string, string>())
        {
            capture.ResponseHeaders[pair.Key] = pair.Value;
        }

        return capture;
    }

    /// <summary>Port of <c>prepareChunksFixtureResponse</c>.</summary>
    internal static OpenAICapture ChunksFixture(
        string name,
        IReadOnlyDictionary<string, string>? headers = null
    )
    {
        var lines = Fixture(name + ".chunks.txt")
            .Split('\n')
            .Where(line => line.Trim().Length > 0)
            .Select(line => "data: " + line + "\n\n");
        var capture = new OpenAICapture
        {
            ServerSentEvents = string.Concat(lines) + "data: [DONE]\n\n",
        };
        foreach (var pair in headers ?? new Dictionary<string, string>())
        {
            capture.ResponseHeaders[pair.Key] = pair.Value;
        }

        return capture;
    }

    /// <summary>Source ids are random in the port. Upstream's <c>mockId</c> numbers them id-0, id-1, and so on.</summary>
    internal static Func<string, string> SequentialIds()
    {
        var ids = new Dictionary<string, string>();
        return id =>
        {
            if (!ids.TryGetValue(id, out var mapped))
            {
                mapped = "id-" + ids.Count;
                ids[id] = mapped;
            }

            return mapped;
        };
    }

    internal static string KebabCase(string name) =>
        string.Concat(
            name.Select(
                (c, index) =>
                    char.IsUpper(c)
                        ? (index > 0 ? "-" : string.Empty) + char.ToLowerInvariant(c)
                        : c.ToString()
            )
        );

    /// <summary>Asserts a whole <c>doGenerate</c> result against an upstream <c>toMatchSnapshot()</c> entry kept as JSON in <c>Fixtures</c>.</summary>
    internal static void AssertResultSnapshot(
        LanguageModelGenerateResult result,
        OpenAICapture capture,
        string snapshot,
        string fixture
    )
    {
        var expected = JsonNode.Parse(Fixture(snapshot + ".generate.snap.json"))!;
        OpenAIResponsesSupport.AssertContent(
            result.Content,
            expected["content"]!.AsArray(),
            includeCitations: true,
            SequentialIds()
        );
        Assert.Equal(
            expected["finishReason"]!["unified"]!.GetValue<string>(),
            KebabCase(result.FinishReason.ToString())
        );
        Assert.Equal(expected["finishReason"]!["raw"]?.GetValue<string>(), result.RawFinishReason);
        JsonAssert.Equal(
            result.ProviderMetadata!.Value,
            expected["providerMetadata"]!.ToJsonString()
        );
        var body = RequestBody(capture);
        Assert.True(
            JsonNode.DeepEquals(expected["request"]!["body"], body),
            "Request body " + body.ToJsonString()
        );
        var response = expected["response"]!;
        Assert.Equal(response["id"]!.GetValue<string>(), result.ResponseId);
        Assert.Equal(response["modelId"]!.GetValue<string>(), result.ResponseModelId);
        Assert.Equal(
            DateTimeOffset.Parse(
                response["timestamp"]!.GetValue<string>(),
                System.Globalization.CultureInfo.InvariantCulture
            ),
            result.ResponseTimestamp
        );
        Assert.True(
            JsonNode.DeepEquals(response["body"], JsonNode.Parse(result.RawResponse!)),
            "Response body"
        );
        Assert.Equal(
            response["headers"]!["content-type"]!.GetValue<string>(),
            result.ResponseHeaders["content-type"].Split(';')[0]
        );
        Assert.Equal(
            response["headers"]!["content-length"]!.GetValue<string>(),
            result.ResponseHeaders["content-length"]
        );
        var usage = result.Usage;
        var expectedUsage = expected["usage"]!;
        Assert.Equal(
            expectedUsage["inputTokens"]!["noCache"]!.GetValue<int>(),
            usage.NoCacheInputTokens
        );
        Assert.Equal(expectedUsage["inputTokens"]!["total"]!.GetValue<int>(), usage.InputTokens);
        Assert.Equal(
            expectedUsage["inputTokens"]!["cacheRead"]!.GetValue<int>(),
            usage.CacheReadTokens
        );
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(
            expectedUsage["outputTokens"]!["reasoning"]!.GetValue<int>(),
            usage.ReasoningTokens
        );
        Assert.Equal(expectedUsage["outputTokens"]!["text"]!.GetValue<int>(), usage.TextTokens);
        Assert.Equal(expectedUsage["outputTokens"]!["total"]!.GetValue<int>(), usage.OutputTokens);
        Assert.True(
            JsonNode.DeepEquals(expectedUsage["raw"], JsonNode.Parse(usage.Raw!.Value.GetRawText()))
        );
        Assert.Equal(expected["warnings"]!.AsArray().Count, result.Warnings.Count);
        Assert.Equal(
            KebabCase(result.FinishReason.ToString()),
            expected["finishReason"]!["unified"]!.GetValue<string>()
        );
    }
}
