// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicMessageThreads;

/// <summary>What one generate or stream call produced, for the shared message thread assertions.</summary>
internal sealed class ThreadCallResult
{
    public string? ResponseId { get; init; }

    public IReadOnlyList<CallWarning> Warnings { get; init; } = Array.Empty<CallWarning>();

    public IReadOnlyList<string> PartTypes { get; init; } = Array.Empty<string>();

    public IReadOnlyList<ToolCallStreamPart> StreamToolCalls { get; init; } = Array.Empty<ToolCallStreamPart>();

    public IReadOnlyList<GeneratedContent> Content { get; init; } = Array.Empty<GeneratedContent>();
}

internal static class ThreadTestSupport
{
    public static readonly string[] Methods = { "generate", "stream" };

    public static IEnumerable<object[]> Each()
    {
        foreach (var method in Methods)
        {
            yield return new object[] { method };
        }
    }

    public static string Fixture(string name)
    {
        return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));
    }

    public static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    public static IReadOnlyDictionary<string, JsonElement> Opts(string json, string name = "anthropic")
    {
        return new Dictionary<string, JsonElement> { [name] = Json(json) };
    }

    public static LanguageModelCallOptions Options(IReadOnlyList<ModelMessage> prompt, string? providerOptions = null, bool tool = true)
    {
        var options = new LanguageModelCallOptions { Prompt = prompt };
        if (tool)
        {
            options.Tools = new[] { new LanguageModelTool("weather", null, Json("{\"type\":\"object\"}")) };
        }

        if (providerOptions != null)
        {
            options.ProviderOptions = Opts(providerOptions);
        }

        return options;
    }

    public static IReadOnlyList<ModelMessage> SystemAndHello()
    {
        return new ModelMessage[] { new SystemModelMessage("Be concise."), new UserModelMessage("Hello") };
    }

    public static ModelMessage Hello()
    {
        return new UserModelMessage("Hello");
    }

    public static LanguageModelCallOptions BaseOptions(string? providerOptions = null)
    {
        return Options(SystemAndHello(), providerOptions);
    }

    public static async Task<ThreadCallResult> Call(
        string method,
        CaptureHandler handler,
        LanguageModelCallOptions options,
        ILanguageModel? model = null,
        string fixture = "anthropic-text")
    {
        model ??= AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-key" }, handler).LanguageModel("claude-sonnet-4-6");
        if (method == "generate")
        {
            handler.ContentType = "application/json";
            handler.ResponseBody = Fixture(fixture + ".json");
            var result = await model.DoGenerateAsync(options, CancellationToken.None).ConfigureAwait(false);
            return new ThreadCallResult { ResponseId = result.ResponseId, Warnings = result.Warnings, Content = result.Content };
        }

        var chunks = new StringBuilder();
        foreach (var line in Fixture(fixture + ".chunks.txt").Split('\n'))
        {
            if (line.Length > 0)
            {
                chunks.Append("data: ").Append(line).Append("\n\n");
            }
        }

        handler.ContentType = "text/event-stream";
        handler.ResponseBody = chunks.ToString();
        var stream = await model.DoStreamAsync(options, CancellationToken.None).ConfigureAwait(false);
        var warnings = new List<CallWarning>();
        var types = new List<string>();
        var calls = new List<ToolCallStreamPart>();
        string? id = null;
        await foreach (var part in stream.Stream.ConfigureAwait(false))
        {
            types.Add(part.GetType().Name);
            if (part is StreamStartStreamPart start)
            {
                warnings.AddRange(start.Warnings);
            }
            else if (part is ResponseMetadataStreamPart metadata)
            {
                id = metadata.Id;
            }
            else if (part is ToolCallStreamPart call)
            {
                calls.Add(call);
            }
        }

        return new ThreadCallResult { ResponseId = id, Warnings = warnings, PartTypes = types, StreamToolCalls = calls };
    }

    public static JsonDocument Body(CaptureHandler handler)
    {
        return JsonDocument.Parse(handler.Body);
    }

    public static string[] Betas(CaptureHandler handler)
    {
        return handler.Headers.TryGetValue("anthropic-beta", out var value)
            ? value.Split(',').Select(beta => beta.Trim()).ToArray()
            : Array.Empty<string>();
    }
}
