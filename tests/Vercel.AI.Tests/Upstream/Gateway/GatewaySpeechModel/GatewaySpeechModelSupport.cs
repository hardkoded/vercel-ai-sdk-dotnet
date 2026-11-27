// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewaySpeechModel;

/// <summary>Helpers of <c>gateway-speech-model.test.ts</c>.</summary>
internal static class GatewaySpeechModelSupport
{
    public static Vercel.AI.Gateway.GatewaySpeechModel CreateTestModel(
        HttpMessageHandler handler,
        IReadOnlyDictionary<string, string>? o11yHeaders = null
    )
    {
        var provider = GatewayProvider.Create(
            new GatewayOptions
            {
                ApiKey = "test-token",
                BaseUrl = "https://api.test.com",
                O11yHeaders = o11yHeaders ?? new Dictionary<string, string>(),
            },
            handler
        );
        return (Vercel.AI.Gateway.GatewaySpeechModel)provider.SpeechModel("openai/tts-1");
    }

    /// <summary>Maps to <c>prepareJsonResponse</c>.</summary>
    public static UpstreamRecordingHandler PrepareJsonResponse()
    {
        return new UpstreamRecordingHandler("{\"audio\":\"base64-audio\"}");
    }

    public static Task<SpeechModelResult> DoGenerate(
        Vercel.AI.Gateway.GatewaySpeechModel model,
        string text = "Hello world",
        string? voice = null,
        string? outputFormat = null,
        string? instructions = null,
        double? speed = null,
        string? language = null,
        string? providerOptions = null,
        IReadOnlyDictionary<string, string>? headers = null
    )
    {
        var options = providerOptions is null
            ? default
            : JsonDocument.Parse(providerOptions).RootElement.Clone();
        return model.DoGenerateAsync(
            new SpeechModelCall(
                text,
                voice,
                outputFormat,
                instructions,
                speed,
                language,
                options,
                headers ?? new Dictionary<string, string>(),
                CancellationToken.None
            ),
            CancellationToken.None
        );
    }
}
