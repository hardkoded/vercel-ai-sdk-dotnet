// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using SteeringFactory = Vercel.AI.GenerateText.Steering;

namespace Vercel.AI.Tests.Upstream.GenerateTextSteering;

/// <summary>
/// Shared helpers of the steering tests. Maps to the <c>createModel</c>, <c>lastUserText</c>, <c>textResponse</c>, <c>toolCallResponse</c>,
/// <c>textStream</c>, <c>toolCallStream</c>, and <c>weather</c> helpers of <c>generate-text-steering.test.ts</c> and <c>stream-text-steering.test.ts</c>.
/// </summary>
internal static class SteeringTestSupport
{
    public const string WeatherSchema =
        "{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"],\"additionalProperties\":false}";

    public static readonly LanguageModelUsage Usage = new(3, 10, 13);

    public static LanguageModelGenerateResult TextResponse(
        string text,
        FinishReason finishReason = FinishReason.Stop,
        string raw = "stop"
    )
    {
        return new LanguageModelGenerateResult(
            new GeneratedContent[] { new GeneratedText(text) },
            finishReason,
            Usage,
            raw
        );
    }

    public static LanguageModelGenerateResult ToolCallResponse()
    {
        return new LanguageModelGenerateResult(
            new GeneratedContent[]
            {
                new GeneratedToolCall("call-1", "weather", "{ \"city\": \"Paris\" }"),
            },
            FinishReason.ToolCalls,
            Usage,
            "tool_calls"
        );
    }

    public static IReadOnlyList<LanguageModelStreamPart> TextStream(
        string text,
        FinishReason finishReason = FinishReason.Stop,
        string raw = "stop"
    )
    {
        return new LanguageModelStreamPart[]
        {
            new StreamStartStreamPart(Array.Empty<CallWarning>()),
            new TextStartStreamPart("1"),
            new TextDeltaStreamPart("1", text),
            new TextEndStreamPart("1"),
            new FinishStreamPart(finishReason, Usage, raw),
        };
    }

    public static IReadOnlyList<LanguageModelStreamPart> ToolCallStream()
    {
        return new LanguageModelStreamPart[]
        {
            new StreamStartStreamPart(Array.Empty<CallWarning>()),
            new ToolCallStreamPart("call-1", "weather", "{ \"city\": \"Paris\" }"),
            new FinishStreamPart(FinishReason.ToolCalls, Usage, "tool_calls"),
        };
    }

    public static Tool Weather(Func<string> execute) =>
        Tool.Function("weather", null, WeatherSchema, (_, _) => Task.FromResult(execute()));

    public static string? LastUserText(LanguageModelCallOptions options)
    {
        var message = options.Prompt[options.Prompt.Count - 1];
        return message is UserModelMessage { Content: [TextContentPart text, ..] }
            ? text.Text
            : null;
    }

    public static void Ignore(Task task) { }

    public static ISteering CreateSteering() => SteeringFactory.ExperimentalCreateSteering();

    public static AiClient Client() =>
        new(
            Vercel.AI.Gateway.GatewayProvider.Create(
                new Vercel.AI.Gateway.GatewayOptions { ApiKey = "test" }
            )
        );

    public static string Text(UserModelMessage message) =>
        Assert.IsType<TextContentPart>(Assert.Single(message.Content)).Text;
}

/// <summary>Maps to <c>MockLanguageModelV4</c> with a function per call for <c>doGenerate</c> and <c>doStream</c>.</summary>
internal sealed class SteeringTestModel : ILanguageModel
{
    private readonly Func<LanguageModelCallOptions, LanguageModelGenerateResult>[] _generate;
    private readonly Func<
        LanguageModelCallOptions,
        IAsyncEnumerable<LanguageModelStreamPart>
    >[] _stream;
    private int _generateCall;
    private int _streamCall;

    public SteeringTestModel(
        Func<LanguageModelCallOptions, LanguageModelGenerateResult>[]? generate = null,
        Func<LanguageModelCallOptions, IAsyncEnumerable<LanguageModelStreamPart>>[]? stream = null
    )
    {
        _generate = generate ?? [];
        _stream = stream ?? [];
    }

    public static SteeringTestModel Generate(
        params Func<LanguageModelCallOptions, LanguageModelGenerateResult>[] responses
    ) => new(generate: responses);

    public static SteeringTestModel Stream(
        params Func<IReadOnlyList<LanguageModelStreamPart>>[] responses
    )
    {
        return new SteeringTestModel(
            stream: responses
                .Select<
                    Func<IReadOnlyList<LanguageModelStreamPart>>,
                    Func<LanguageModelCallOptions, IAsyncEnumerable<LanguageModelStreamPart>>
                >(response => _ => ToAsync(response()))
                .ToArray()
        );
    }

    public List<LanguageModelCallOptions> DoGenerateCalls { get; } = [];

    public List<LanguageModelCallOptions> DoStreamCalls { get; } = [];

    public IReadOnlyDictionary<string, string> SupportedUrls =>
        Vercel.AI.ProviderUtils.UrlSupport.None;

    public string SpecificationVersion => "V4";

    public string Provider => "mock-provider";

    public string ModelId => "mock-model-id";

    public static async IAsyncEnumerable<LanguageModelStreamPart> ToAsync(
        IEnumerable<LanguageModelStreamPart> parts,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        foreach (var part in parts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return part;
            await Task.Yield();
        }
    }

    public Task<LanguageModelGenerateResult> DoGenerateAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        DoGenerateCalls.Add(options);
        return Task.FromResult(_generate[_generateCall++](options));
    }

    public Task<LanguageModelStreamResult> DoStreamAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        DoStreamCalls.Add(options);
        return Task.FromResult(
            new LanguageModelStreamResult(_stream[_streamCall++](options), null, null)
        );
    }
}
