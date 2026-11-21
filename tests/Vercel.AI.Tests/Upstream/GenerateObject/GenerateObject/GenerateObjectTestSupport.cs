// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject;

/// <summary>A model with settable provider and model id. Maps to <c>MockLanguageModelV4</c> with a <c>doGenerate</c> function.</summary>
internal sealed class GenerateObjectTestModel : ILanguageModel
{
    public Action? OnGenerate { get; set; }

    public string SpecificationVersion => "V4";

    public string Provider { get; init; } = "mock-provider";

    public string ModelId { get; init; } = "mock-model-id";

    public IReadOnlyDictionary<string, string> SupportedUrls => new Dictionary<string, string>();

    public Task<LanguageModelGenerateResult> DoGenerateAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        OnGenerate?.Invoke();
        return Task.FromResult(
            new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedText("{ \"content\": \"Hello, world!\" }") },
                FinishReason.Stop,
                new LanguageModelUsage(10, 20, 30)
            )
        );
    }

    public Task<LanguageModelStreamResult> DoStreamAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    ) => throw new NotSupportedException();
}

internal static class GenerateObjectTestSupport
{
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
