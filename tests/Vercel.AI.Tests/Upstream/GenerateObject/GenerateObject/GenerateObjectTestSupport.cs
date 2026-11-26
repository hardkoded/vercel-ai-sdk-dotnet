// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject;

/// <summary>A model with settable provider and model id. Maps to <c>MockLanguageModelV4</c> with a <c>doGenerate</c> function.</summary>
internal class GenerateObjectTestModel : ILanguageModel
{
    public Action? OnGenerate { get; set; }

    /// <summary>When set, replaces the default result. Maps to a <c>doGenerate</c> function.</summary>
    public Func<LanguageModelCallOptions, LanguageModelGenerateResult>? Generate { get; set; }

    /// <summary>Calls the model observed. Maps to <c>doGenerateCalls</c>.</summary>
    public List<LanguageModelCallOptions> Calls { get; } = [];

    public string SpecificationVersion => "V4";

    public string Provider { get; init; } = "mock-provider";

    public string ModelId { get; init; } = "mock-model-id";

    public virtual IReadOnlyDictionary<string, string> SupportedUrls =>
        new Dictionary<string, string>();

    public Task<LanguageModelGenerateResult> DoGenerateAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        Calls.Add(options);
        OnGenerate?.Invoke();
        return Task.FromResult(Generate?.Invoke(options) ?? GenerateObjectTestSupport.Result());
    }

    public Task<LanguageModelStreamResult> DoStreamAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    ) => throw new NotSupportedException();
}

internal static class GenerateObjectTestSupport
{
    /// <summary>Maps to <c>dummyResponseValues</c> with text content.</summary>
    public static LanguageModelGenerateResult Result(
        string text = "{ \"content\": \"Hello, world!\" }",
        params GeneratedContent[] before
    ) =>
        new(
            [.. before, new GeneratedText(text)],
            FinishReason.Stop,
            new LanguageModelUsage(10, 20, 30),
            "stop",
            responseId: "id-1",
            responseModelId: "m-1",
            responseTimestamp: DateTimeOffset.FromUnixTimeMilliseconds(123)
        );

    public const string ContentSchema =
        "{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"type\":\"object\",\"properties\":{\"content\":{\"type\":\"string\"}},\"required\":[\"content\"],\"additionalProperties\":false}";

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

    public static GenerateObjectOptions<ContentResult> ObjectOptions(ILanguageModel model) =>
        new()
        {
            Model = model,
            Schema = ContentSchema,
            Prompt = "prompt",
            SerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
        };
}
