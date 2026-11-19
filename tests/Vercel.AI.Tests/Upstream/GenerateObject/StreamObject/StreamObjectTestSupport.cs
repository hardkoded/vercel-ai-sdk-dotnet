// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>A model whose stream is built by a function. Maps to <c>MockLanguageModelV4</c> with a <c>doStream</c> function.</summary>
internal sealed class StreamObjectTestModel : ILanguageModel
{
    public Func<LanguageModelCallOptions, Task<LanguageModelStreamResult>> OnStream { get; set; } =
        _ => throw new InvalidOperationException("doStream is not set.");

    public Func<IReadOnlyDictionary<string, string>>? OnSupportedUrls { get; set; }

    public List<LanguageModelCallOptions> Calls { get; } = [];

    public string SpecificationVersion => "V4";

    public string Provider { get; init; } = "mock-provider";

    public string ModelId { get; init; } = "mock-model-id";

    public IReadOnlyDictionary<string, string> SupportedUrls =>
        OnSupportedUrls?.Invoke() ?? new Dictionary<string, string>();

    public Task<LanguageModelGenerateResult> DoGenerateAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    ) => throw new NotSupportedException();

    public Task<LanguageModelStreamResult> DoStreamAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        Calls.Add(options);
        return OnStream(options);
    }
}

/// <summary>Helpers that map to <c>createTestModel</c> and <c>testUsage</c> of <c>stream-object.test.ts</c>.</summary>
internal static class StreamObjectTestSupport
{
    public const string Prefix =
        "packages/ai/src/generate-object/stream-object.test.ts::streamObject > ";

    public const string ContentSchema =
        "{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"type\":\"object\",\"properties\":{\"content\":{\"type\":\"string\"}},\"required\":[\"content\"],\"additionalProperties\":false}";

    public static readonly LanguageModelUsage TestUsage = new(
        3,
        10,
        13,
        noCacheInputTokens: 3,
        textTokens: 10
    );

    public static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public sealed class ContentResult
    {
        public string Content { get; set; } = string.Empty;
    }

    public static AiClient Client() =>
        new(
            Vercel.AI.Gateway.GatewayProvider.Create(
                new Vercel.AI.Gateway.GatewayOptions { ApiKey = "test" }
            )
        );

    public static LanguageModelStreamPart Delta(string delta) =>
        new TextDeltaStreamPart("1", delta);

    public static LanguageModelStreamPart Finish(JsonElement? providerMetadata = null) =>
        new FinishStreamPart(FinishReason.Stop, TestUsage, "stop", providerMetadata);

    public static LanguageModelStreamPart ResponseMetadata() =>
        new ResponseMetadataStreamPart(
            "id-0",
            "mock-model-id",
            DateTimeOffset.FromUnixTimeMilliseconds(0)
        );

    // A text block with the given deltas, followed by a finish part.
    public static List<LanguageModelStreamPart> TextStream(params string[] deltas)
    {
        var parts = new List<LanguageModelStreamPart> { new TextStartStreamPart("1") };
        parts.AddRange(deltas.Select(Delta));
        parts.Add(new TextEndStreamPart("1"));
        parts.Add(Finish());
        return parts;
    }

    public static LanguageModelStreamResult Result(
        IEnumerable<LanguageModelStreamPart> parts,
        LanguageModelStreamRequest? request = null,
        LanguageModelStreamResponse? response = null
    ) => new(ToAsync(parts), request, response);

    public static StreamObjectTestModel Model(
        IEnumerable<LanguageModelStreamPart> parts,
        LanguageModelStreamRequest? request = null,
        LanguageModelStreamResponse? response = null
    )
    {
        var list = parts.ToList();
        return new StreamObjectTestModel
        {
            OnStream = _ => Task.FromResult(Result(list, request, response)),
        };
    }

    // Maps to createTestModel.
    public static StreamObjectTestModel CreateTestModel(
        IEnumerable<LanguageModelStreamPart>? stream = null,
        IReadOnlyList<CallWarning>? warnings = null,
        LanguageModelStreamRequest? request = null,
        LanguageModelStreamResponse? response = null
    )
    {
        var parts = stream
            ?? new LanguageModelStreamPart[]
            {
                new StreamStartStreamPart(warnings ?? []),
                ResponseMetadata(),
                new TextStartStreamPart("1"),
                Delta("{ "),
                Delta("\"content\": "),
                Delta("\"Hello, "),
                Delta("world"),
                Delta("!\""),
                Delta(" }"),
                new TextEndStreamPart("1"),
                Finish(
                    JsonSerializer.SerializeToElement(
                        new { testProvider = new { testKey = "testValue" } }
                    )
                ),
            };
        return Model(parts, request, response);
    }

    public static StreamObjectTestModel ThrowingModel(Exception error) =>
        new() { OnStream = _ => Task.FromException<LanguageModelStreamResult>(error) };

    // Compares JSON values by content, not by property order.
    public static void AssertJsonEqual(string expected, JsonElement actual) =>
        Assert.True(
            System.Text.Json.Nodes.JsonNode.DeepEquals(
                System.Text.Json.Nodes.JsonNode.Parse(expected),
                System.Text.Json.Nodes.JsonNode.Parse(actual.GetRawText())
            ),
            actual.GetRawText()
        );

    public static async Task<List<T>> Collect<T>(IAsyncEnumerable<T> source)
    {
        var result = new List<T>();
        await foreach (var item in source)
        {
            result.Add(item);
        }

        return result;
    }

    public static async Task<string[]> CollectRaw(IAsyncEnumerable<JsonElement> source) =>
        (await Collect(source)).Select(e => e.GetRawText()).ToArray();

    public static StreamObjectOptions<ContentResult> ObjectOptions(ILanguageModel model) =>
        new()
        {
            Model = model,
            Schema = ContentSchema,
            Prompt = "prompt",
            SerializerOptions = Json,
        };

    private static async IAsyncEnumerable<LanguageModelStreamPart> ToAsync(
        IEnumerable<LanguageModelStreamPart> parts
    )
    {
        foreach (var part in parts)
        {
            yield return part;
            await Task.Yield();
        }
    }

    // A stream that fails after the given parts.
    public static async IAsyncEnumerable<LanguageModelStreamPart> Failing(Exception error)
    {
        await Task.Yield();
        throw error;
#pragma warning disable CS0162
        yield break;
#pragma warning restore CS0162
    }

    // Runs body with a recording warning logger. Maps to the logWarnings spy.
    public static async Task<List<LogWarningsOptions>> WithLogWarnings(Func<Task> body)
    {
        var logged = new List<LogWarningsOptions>();
        var previous = LogWarnings.Logger;
        LogWarnings.Logger = new Action<LogWarningsOptions>(options =>
        {
            lock (logged)
            {
                logged.Add(options);
            }
        });
        try
        {
            await body().ConfigureAwait(false);
        }
        finally
        {
            LogWarnings.Logger = previous;
        }

        return logged;
    }
}
