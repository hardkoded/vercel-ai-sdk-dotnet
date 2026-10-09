// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Shared fixtures for Anthropic upstream parity tests.</summary>
internal static class AnthropicParity
{
    public static LanguageModelCallOptions Hello(string text = "Hello")
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage(text) },
        };
    }

    public static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    public static LanguageModelCallOptions WithProvider(string json, LanguageModelCallOptions? options = null)
    {
        options = options ?? Hello();
        options.ProviderOptions = new Dictionary<string, JsonElement> { ["anthropic"] = Json(json) };
        return options;
    }

    public static AnthropicPreparedRequest Prepare(string model, LanguageModelCallOptions options, bool stream = false)
    {
        return AnthropicMessagesRequest.Prepare(model, options, stream, "anthropic.messages");
    }

    public static CaptureHandler Ok(string? body = null)
    {
        return new CaptureHandler
        {
            ResponseBody = body ?? "{\"id\":\"msg_123\",\"type\":\"message\",\"role\":\"assistant\",\"model\":\"claude-3-haiku-20240307\",\"content\":[{\"type\":\"text\",\"text\":\"Hi\"}],\"stop_reason\":\"end_turn\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":1,\"output_tokens\":1}}",
        };
    }

    public static AnthropicProvider Client(CaptureHandler handler, AnthropicOptions? options = null)
    {
        return AnthropicProvider.Create(options ?? new AnthropicOptions { ApiKey = "test-api-key" }, handler);
    }

    public static AnthropicAwsProvider Aws(CaptureHandler handler, AnthropicOptions options)
    {
        return AnthropicAwsProvider.Create(options, handler);
    }

    public static async Task<LanguageModelGenerateResult> Generate(AnthropicProvider provider, string model, LanguageModelCallOptions? options = null)
    {
        return await provider.LanguageModel(model).DoGenerateAsync(options ?? Hello(), CancellationToken.None).ConfigureAwait(false);
    }

    public static void AssertCapability(
        string model,
        int max,
        bool known,
        bool structured,
        bool adaptive,
        bool rejectSampling,
        bool xhigh,
        bool rejectAboveHigh,
        bool rejectDisabled,
        bool rejectBudget,
        bool rejectForced)
    {
        var caps = AnthropicModelCapabilities.Get(model);
        Assert.Equal(max, caps.MaxOutputTokens);
        Assert.Equal(known, caps.IsKnownModel);
        Assert.Equal(structured, caps.SupportsStructuredOutput);
        Assert.Equal(adaptive, caps.SupportsAdaptiveThinking);
        Assert.Equal(rejectSampling, caps.RejectsSamplingParameters);
        Assert.Equal(xhigh, caps.SupportsXhighEffort);
        Assert.Equal(rejectAboveHigh, caps.RejectsThinkingDisabledAboveHighEffort);
        Assert.Equal(rejectDisabled, caps.RejectsThinkingDisabled);
        Assert.Equal(rejectBudget, caps.RejectsBudgetThinking);
        Assert.Equal(rejectForced, caps.RejectsForcedToolUse);
        Assert.False(caps.SupportsBetweenToolsThinking);
    }

    public static LanguageModelUsage ConvertUsage(string usage, string? raw = null)
    {
        using var document = JsonDocument.Parse(usage);
        if (raw == null)
        {
            return AnthropicUsage.Convert(document.RootElement);
        }

        using var rawDocument = JsonDocument.Parse(raw);
        return AnthropicUsage.Convert(document.RootElement, rawDocument.RootElement);
    }
}

/// <summary>Restores environment variables changed by a test.</summary>
internal sealed class EnvScope : IDisposable
{
    private readonly Dictionary<string, string?> _previous = new Dictionary<string, string?>();

    public EnvScope Set(string name, string? value)
    {
        if (!_previous.ContainsKey(name))
        {
            _previous[name] = Environment.GetEnvironmentVariable(name);
        }

        Environment.SetEnvironmentVariable(name, value);
        return this;
    }

    public void Dispose()
    {
        foreach (var pair in _previous)
        {
            Environment.SetEnvironmentVariable(pair.Key, pair.Value);
        }
    }
}

/// <summary>Records one HTTP request and returns a scripted response.</summary>
internal sealed class CaptureHandler : HttpMessageHandler
{
    public int Calls { get; private set; }

    public string Uri { get; private set; } = string.Empty;

    public string Method { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public string ResponseBody { get; set; } = "{}";

    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

    public string ContentType { get; set; } = "application/json";

    public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public List<string> Uris { get; } = new List<string>();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        Method = request.Method.Method;
        Uri = request.RequestUri?.AbsoluteUri ?? string.Empty;
        Uris.Add(Uri);
        if (request.Content != null)
        {
            var text = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (text.Length > 0)
            {
                Body = text;
            }
        }
        Headers.Clear();
        foreach (var header in request.Headers)
        {
            Headers[header.Key] = string.Join(",", header.Value);
        }

        if (request.Headers.Authorization != null)
        {
            Headers["Authorization"] = request.Headers.Authorization.ToString();
        }

        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                Headers[header.Key] = string.Join(",", header.Value);
            }
        }

        return new HttpResponseMessage(Status)
        {
            Content = new StringContent(ResponseBody, Encoding.UTF8, ContentType),
        };
    }
}

/// <summary>Keeps Anthropic parity tests from racing on environment variables.</summary>
[CollectionDefinition("AnthropicParity", DisableParallelization = true)]
public sealed class AnthropicParityCollection
{
}

/// <summary>Dispatches one upstream id to its assertion.</summary>
internal static class AnthropicCases
{
    private static readonly Dictionary<string, Func<Task>> Map = new Dictionary<string, Func<Task>>();

    static AnthropicCases()
    {
        AnthropicCaseRegistration.Register();
    }

    public static void Add(string id, Action action)
    {
        Map[id] = () =>
        {
            action();
            return Task.CompletedTask;
        };
    }

    public static void Add(string id, Func<Task> action)
    {
        Map[id] = action;
    }

    public static Task Run(string id)
    {
        return Map[id]();
    }
}
