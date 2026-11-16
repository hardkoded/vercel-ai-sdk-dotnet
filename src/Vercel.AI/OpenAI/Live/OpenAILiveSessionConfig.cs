// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenAI;

/// <summary>Builds OpenAI Live session configurations. Maps to <c>openai-live-session-config.ts</c>.</summary>
internal static class OpenAILiveSessionConfig
{
    /// <summary>Transport name for a server WebSocket startup.</summary>
    internal const string WebSocketTransport = "websocket";

    /// <summary>Transport name for a WebRTC startup.</summary>
    internal const string WebRTCTransport = "webrtc";

    /// <summary>Builds the Live session configuration. Maps to <c>buildOpenAILiveSessionConfig</c>.</summary>
    public static JsonObject BuildOpenAILiveSessionConfig(
        RealtimeModelV4SessionConfig config,
        string modelId,
        string transport = WebSocketTransport
    )
    {
        foreach (
            var (key, present) in new (string, bool)[]
            {
                ("outputModalities", config.OutputModalities != null),
                ("inputAudioTranscription", config.InputAudioTranscription != null),
                ("outputAudioTranscription", config.OutputAudioTranscription != null),
                ("turnDetection", config.TurnDetection != null || config.TurnDetectionDisabled),
                ("tools", config.Tools != null),
            }
        )
        {
            if (present)
            {
                throw Unsupported("OpenAI Live session setting: " + key);
            }
        }

        var providerOptions =
            config.ProviderOptions != null
            && config.ProviderOptions.TryGetPropertyValue("openai", out var openai)
                ? openai
                : null;
        if (
            providerOptions is JsonObject providerObject
            && providerObject["delegation"] is JsonObject delegationObject
            && delegationObject["type"] is JsonValue delegationType
            && delegationType.TryGetValue<string>(out var delegationName)
            && delegationName == "responses"
        )
        {
            throw Unsupported(
                "OpenAI Live Responses delegation; only client delegation is supported"
            );
        }

        var options = OpenAIRealtimeModelLiveOptions.Parse(providerOptions);
        if (options.Client != null && transport != WebRTCTransport)
        {
            throw Unsupported("OpenAI Live client permissions outside WebRTC startup");
        }

        if (options.VoiceId != null && config.Voice != null)
        {
            throw new InvalidArgumentException(
                "voice",
                config.Voice,
                "Choose either voice or providerOptions.openai.voice."
            );
        }

        var hasInputFormat =
            config.InputAudioFormatType != null || config.InputAudioFormatRate != null;
        var hasOutputFormat =
            config.OutputAudioFormatType != null || config.OutputAudioFormatRate != null;
        if (transport == WebRTCTransport && (hasInputFormat || hasOutputFormat))
        {
            throw Unsupported(
                "Fixed audio formats for OpenAI Live WebRTC; audio is negotiated through SDP"
            );
        }

        var inputFormat = hasInputFormat
            ? ParseAudioFormat(
                "inputAudioFormat",
                config.InputAudioFormatType,
                config.InputAudioFormatRate
            )
            : ((string Type, int Rate)?)null;
        var outputFormat = hasOutputFormat
            ? ParseAudioFormat(
                "outputAudioFormat",
                config.OutputAudioFormatType,
                config.OutputAudioFormatRate
            )
            : ((string Type, int Rate)?)null;
        if (
            inputFormat != null
            && outputFormat != null
            && (
                inputFormat.Value.Type != outputFormat.Value.Type
                || inputFormat.Value.Rate != outputFormat.Value.Rate
            )
        )
        {
            throw new InvalidArgumentException(
                "outputAudioFormat",
                outputFormat.Value.Type,
                "OpenAI Live requires the same input and output audio format."
            );
        }

        var session = new JsonObject { ["model"] = modelId };
        if (config.Instructions != null)
        {
            session["instructions"] = config.Instructions;
        }

        var audio = new JsonObject();
        if (transport == WebSocketTransport)
        {
            var format = inputFormat ?? outputFormat ?? (Type: "audio/pcm", Rate: 24000);
            audio["format"] = new JsonObject { ["type"] = format.Type, ["rate"] = format.Rate };
        }

        audio["output"] = new JsonObject
        {
            ["voice"] =
                options.VoiceId != null
                    ? new JsonObject { ["id"] = options.VoiceId }
                    : (JsonNode?)(config.Voice ?? "marin"),
        };
        session["audio"] = audio;

        if (options.ClientDelegation)
        {
            session["delegation"] = new JsonObject { ["type"] = "client" };
        }
        else if (options.DisableDelegation)
        {
            session["delegation"] = null;
        }

        if (options.Client != null)
        {
            var channel = new JsonObject();
            if (options.Client.AllowAllClientEvents)
            {
                channel["allowed_client_events"] = "all";
            }
            else if (options.Client.AllowedClientEvents != null)
            {
                channel["allowed_client_events"] = new JsonArray(
                    options
                        .Client.AllowedClientEvents.Select(value =>
                            (JsonNode?)JsonValue.Create(value)
                        )
                        .ToArray()
                );
            }

            if (options.Client.AllowAllServerEvents)
            {
                channel["allowed_server_events"] = "all";
            }
            else if (options.Client.AllowedServerEvents != null)
            {
                var selectors = new JsonArray();
                foreach (var selector in options.Client.AllowedServerEvents)
                {
                    var node = new JsonObject { ["type"] = selector.Type };
                    if (selector.ResponseEvent != null)
                    {
                        node["response_event"] = selector.ResponseEvent;
                    }

                    selectors.Add(node);
                }

                channel["allowed_server_events"] = selectors;
            }

            session["client"] = new JsonObject { ["data_channel"] = channel };
        }

        if (options.Input != null)
        {
            session["input"] = new JsonArray(
                options.Input.Select(item => (JsonNode?)item.DeepClone()).ToArray()
            );
        }

        if (options.Store != null)
        {
            session["store"] = options.Store;
        }

        return session;
    }

    private static (string Type, int Rate) ParseAudioFormat(
        string argument,
        string? type,
        int? rate
    )
    {
        var valid =
            (type == "audio/pcm" && (rate == 16000 || rate == 24000))
            || ((type == "audio/pcma" || type == "audio/pcmu") && rate == 8000);
        if (!valid)
        {
            throw new InvalidArgumentException(
                argument,
                type,
                "OpenAI Live does not support this audio format."
            );
        }

        return (type!, rate!.Value);
    }

    private static UnsupportedFunctionalityException Unsupported(string functionality)
    {
        return new UnsupportedFunctionalityException(
            functionality,
            "'" + functionality + "' functionality not supported."
        );
    }
}
