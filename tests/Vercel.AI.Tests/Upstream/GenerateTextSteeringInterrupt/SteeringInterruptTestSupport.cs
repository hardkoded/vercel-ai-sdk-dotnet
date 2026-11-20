// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Channels;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.GenerateTextSteeringInterrupt;

/// <summary>
/// Shared helpers of the steering interrupt tests. Maps to the <c>usage</c>, <c>textResponse</c>, <c>toolCallResponse</c>, <c>textStream</c>,
/// <c>lastUserText</c>, <c>waitForAbort</c>, <c>createHangingFirstCall</c>, <c>hangingStream</c>, and <c>createModel</c> helpers of
/// <c>generate-text-steering-interrupt.test.ts</c> and <c>stream-text-steering-interrupt.test.ts</c>.
/// </summary>
internal static class SteeringInterruptTestSupport
{
    public const string PromptText = "What is the weather in Paris?";
    public const string WeatherSchema =
        "{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"],\"additionalProperties\":false}";

    public static ISteering CreateSteering() =>
        GenerateTextSteering.SteeringTestSupport.CreateSteering();

    public static AiClient Client() => GenerateTextSteering.SteeringTestSupport.Client();

    public static LanguageModelUsage UsageOf(int output) => new(3, output, 3 + output);

    public static LanguageModelGenerateResult TextResponse(string text) =>
        new(
            new GeneratedContent[] { new GeneratedText(text) },
            FinishReason.Stop,
            UsageOf(10),
            "stop"
        );

    public static LanguageModelGenerateResult ToolCallResponse() =>
        new(
            new GeneratedContent[]
            {
                new GeneratedToolCall("call-1", "weather", "{ \"city\": \"Paris\" }"),
            },
            FinishReason.ToolCalls,
            UsageOf(5),
            "tool_calls"
        );

    public static IReadOnlyList<LanguageModelStreamPart> TextStream(string text) =>
        [
            new StreamStartStreamPart([]),
            new TextStartStreamPart("2"),
            new TextDeltaStreamPart("2", text),
            new TextEndStreamPart("2"),
            new FinishStreamPart(FinishReason.Stop, UsageOf(10), "stop"),
        ];

    public static string? LastUserText(LanguageModelCallOptions options) =>
        options.Prompt[options.Prompt.Count - 1]
            is UserModelMessage { Content: [TextContentPart text, ..] }
            ? text.Text
            : null;

    public static string Text(UserModelMessage message) =>
        Assert.IsType<TextContentPart>(Assert.Single(message.Content)).Text;

    public static Tool Weather(Func<Task<string>> execute) =>
        Tool.Function("weather", null, WeatherSchema, (_, _) => execute());

    /// <summary>A weather tool with a context schema for the unit. Maps to the <c>weather</c> tool with <c>contextSchema: z.object({ unit: z.string() })</c>.</summary>
    public static Tool UnitWeather()
    {
        var tool = Tool.Function("weather", null, WeatherSchema, (_, _) =>
            Task.FromResult("{\"temperature\":72}")
        );
        tool.ContextSchema = value => value;
        return tool;
    }

    /// <summary>Maps to <c>toolsContext: { weather: { unit } }</c>.</summary>
    public static IReadOnlyDictionary<string, System.Text.Json.JsonElement> UnitContext(
        string unit
    ) =>
        new Dictionary<string, System.Text.Json.JsonElement>
        {
            ["weather"] = System
                .Text.Json.JsonDocument.Parse($"{{\"unit\":\"{unit}\"}}")
                .RootElement.Clone(),
        };

    /// <summary>Maps to <c>waitForAbort</c>: never returns, and fails when the model call is cancelled.</summary>
    public static Task<T> WaitForAbort<T>(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        var source = new TaskCompletionSource<T>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        void Cancel() => source.TrySetException(new OperationCanceledException());
        options.AbortSignal?.AddAbortHandler(Cancel);
        cancellationToken.Register(Cancel);
        return source.Task;
    }

    /// <summary>
    /// Maps to <c>hangingStream</c>: a stream that emits <paramref name="parts"/>, then waits. It fails when the model call is cancelled,
    /// and it emits <paramref name="release"/> and ends when that completes.
    /// </summary>
    public static LanguageModelStreamResult HangingStream(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken,
        IReadOnlyList<LanguageModelStreamPart> parts,
        Task<IReadOnlyList<LanguageModelStreamPart>>? release = null,
        LanguageModelStreamRequest? request = null
    )
    {
        var channel = Channel.CreateUnbounded<LanguageModelStreamPart>();
        foreach (var part in parts)
        {
            channel.Writer.TryWrite(part);
        }

        void Cancel() => channel.Writer.TryComplete(new OperationCanceledException());
        options.AbortSignal?.AddAbortHandler(Cancel);
        cancellationToken.Register(Cancel);
        _ = release?.ContinueWith(
            rest =>
            {
                foreach (var part in rest.Result)
                {
                    channel.Writer.TryWrite(part);
                }

                channel.Writer.TryComplete();
            },
            TaskScheduler.Default
        );
        return new LanguageModelStreamResult(channel.Reader.ReadAllAsync(), request);
    }

    public static LanguageModelStreamResult ToResult(
        IReadOnlyList<LanguageModelStreamPart> parts,
        LanguageModelStreamRequest? request = null
    ) => new(Read(parts), request);

    private static async IAsyncEnumerable<LanguageModelStreamPart> Read(
        IReadOnlyList<LanguageModelStreamPart> parts
    )
    {
        foreach (var part in parts)
        {
            yield return part;
            await Task.Yield();
        }
    }
}

/// <summary>Maps to <c>MockLanguageModelV4</c> with an async <c>doGenerate</c> and <c>doStream</c>.</summary>
internal sealed class InterruptTestModel : ILanguageModel
{
    public Func<
        LanguageModelCallOptions,
        CancellationToken,
        Task<LanguageModelGenerateResult>
    >? Generate { get; init; }

    public Func<
        LanguageModelCallOptions,
        CancellationToken,
        Task<LanguageModelStreamResult>
    >? Stream { get; init; }

    public List<LanguageModelCallOptions> DoGenerateCalls { get; } = [];

    public List<LanguageModelCallOptions> DoStreamCalls { get; } = [];

    public IReadOnlyDictionary<string, string> SupportedUrls =>
        Vercel.AI.ProviderUtils.UrlSupport.None;

    public string SpecificationVersion => "V4";

    public string Provider => "mock-provider";

    public string ModelId => "mock-model-id";

    /// <summary>Maps to <c>createHangingFirstCall</c>: the first call runs <paramref name="onFirstCall"/> and waits for the abort.</summary>
    public static InterruptTestModel HangingFirstCall(
        Action<LanguageModelCallOptions> onFirstCall,
        params Func<LanguageModelCallOptions, LanguageModelGenerateResult>[] responses
    )
    {
        InterruptTestModel? model = null;
        model = new InterruptTestModel
        {
            Generate = (options, token) =>
            {
                if (model!.DoGenerateCalls.Count == 1)
                {
                    onFirstCall(options);
                    return SteeringInterruptTestSupport.WaitForAbort<LanguageModelGenerateResult>(
                        options,
                        token
                    );
                }

                return Task.FromResult(responses[model.DoGenerateCalls.Count - 2](options));
            },
        };
        return model;
    }

    public Task<LanguageModelGenerateResult> DoGenerateAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        DoGenerateCalls.Add(options);
        return Generate!(options, cancellationToken);
    }

    public Task<LanguageModelStreamResult> DoStreamAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        DoStreamCalls.Add(options);
        return Stream!(options, cancellationToken);
    }
}
