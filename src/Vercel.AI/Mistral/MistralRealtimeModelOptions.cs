// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Mistral;

/// <summary>Speech recognition turn detection of the managed voice agent. Maps to <c>stt.turnDetection</c>.</summary>
public sealed class MistralRealtimeTurnDetection
{
    /// <summary>Upstream <c>startThreshold</c>, 0 to 1.</summary>
    public double? StartThreshold { get; init; }

    /// <summary>Upstream <c>eagerEndThreshold</c>, 0 to 1.</summary>
    public double? EagerEndThreshold { get; init; }

    /// <summary>Upstream <c>endThreshold</c>, 0 to 1.</summary>
    public double? EndThreshold { get; init; }

    /// <summary>Upstream <c>minSilenceMs</c>.</summary>
    public int? MinSilenceMs { get; init; }

    /// <summary>Upstream <c>endTimeoutMs</c>.</summary>
    public int? EndTimeoutMs { get; init; }
}

/// <summary>Speech recognition settings of the managed voice agent. Maps to <c>stt</c>.</summary>
public sealed class MistralRealtimeSttOptions
{
    /// <summary>Upstream <c>model</c>.</summary>
    public string? Model { get; init; }

    /// <summary>Upstream <c>language</c>.</summary>
    public string? Language { get; init; }

    /// <summary>Upstream <c>contextBias</c>.</summary>
    public IReadOnlyList<string>? ContextBias { get; init; }

    /// <summary>Upstream <c>targetStreamingDelayMs</c>, 80 to 1200.</summary>
    public int? TargetStreamingDelayMs { get; init; }

    /// <summary>Upstream <c>turnDetection</c>.</summary>
    public MistralRealtimeTurnDetection? TurnDetection { get; init; }
}

/// <summary>Speech synthesis settings. <c>sessionConfig.voice</c> overrides <see cref="VoiceId"/>. Maps to <c>tts</c>.</summary>
public sealed class MistralRealtimeTtsOptions
{
    /// <summary>Upstream <c>model</c>.</summary>
    public string? Model { get; init; }

    /// <summary>Upstream <c>voiceId</c>.</summary>
    public string? VoiceId { get; init; }

    /// <summary>Upstream <c>phonemeHints</c>.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>>? PhonemeHints { get; init; }
}

/// <summary>Reminders during periods of silence. Maps to <c>turn.inactivity</c>.</summary>
public sealed class MistralRealtimeInactivityOptions
{
    /// <summary>Upstream <c>timeoutMs</c>.</summary>
    public int? TimeoutMs { get; init; }

    /// <summary>Upstream <c>maxReminders</c>.</summary>
    public int? MaxReminders { get; init; }

    /// <summary>Upstream <c>messages</c>.</summary>
    public IReadOnlyList<string>? Messages { get; init; }
}

/// <summary>Interruptions and reminders. Maps to <c>turn</c>.</summary>
public sealed class MistralRealtimeTurnOptions
{
    /// <summary>Upstream <c>allowInterruptions</c>.</summary>
    public bool? AllowInterruptions { get; init; }

    /// <summary>Upstream <c>inactivity</c>.</summary>
    public MistralRealtimeInactivityOptions? Inactivity { get; init; }
}

/// <summary>Mistral voice agent options. Maps to <c>mistralRealtimeModelOptions</c>.</summary>
public sealed class MistralRealtimeModelOptions
{
    /// <summary>Speech recognition settings. Maps to <c>stt</c>.</summary>
    public MistralRealtimeSttOptions? Stt { get; init; }

    /// <summary>Speech synthesis settings. Maps to <c>tts</c>.</summary>
    public MistralRealtimeTtsOptions? Tts { get; init; }

    /// <summary>Interruptions and reminders. Maps to <c>turn</c>.</summary>
    public MistralRealtimeTurnOptions? Turn { get; init; }

    /// <summary>Text spoken when the session connects. Maps to <c>welcomeMessage</c>.</summary>
    public string? WelcomeMessage { get; init; }

    /// <summary>Saved agent whose configuration is overridden for this session. Maps to <c>agentId</c>.</summary>
    public string? AgentId { get; init; }

    /// <summary>Version of the saved agent. Omit to use its latest version. Maps to <c>agentVersion</c>.</summary>
    public int? AgentVersion { get; init; }

    /// <summary>
    /// Parses <c>providerOptions.mistral</c> with a strict schema: unknown keys and out-of-range values are rejected.
    /// Maps to <c>parseProviderOptions</c> with <c>mistralRealtimeModelOptions</c>.
    /// </summary>
    /// <exception cref="InvalidArgumentException">An option has the wrong shape or is out of range.</exception>
    public static MistralRealtimeModelOptions? Parse(JsonElement providerOptions)
    {
        if (MistralOptionValues.Section(providerOptions) is not { } mistral)
        {
            return null;
        }

        Strict(mistral, "stt", "tts", "turn", "welcomeMessage", "agentId", "agentVersion");
        return new MistralRealtimeModelOptions
        {
            Stt = Object(mistral, "stt") is { } stt ? ParseStt(stt) : null,
            Tts = Object(mistral, "tts") is { } tts ? ParseTts(tts) : null,
            Turn = Object(mistral, "turn") is { } turn ? ParseTurn(turn) : null,
            WelcomeMessage = MistralOptionValues.Get(mistral, "welcomeMessage") is { } welcome
                ? Text(welcome, "welcomeMessage")
                : null,
            AgentId = MistralOptionValues.Get(mistral, "agentId") is { } agentId
                ? MistralOptionValues.NonEmptyString(agentId, "agentId")
                : null,
            AgentVersion = Integer(mistral, "agentVersion", 0, int.MaxValue),
        };
    }

    /// <summary>
    /// Builds the REST configuration, or with <paramref name="update"/> the subset accepted by <c>session.update</c>.
    /// Maps to <c>buildMistralRealtimeSessionConfig</c>.
    /// </summary>
    /// <exception cref="UnsupportedFunctionalityException">A setting the managed voice agent cannot honor.</exception>
    /// <exception cref="InvalidArgumentException">A provider option is invalid.</exception>
    public static JsonObject BuildMistralRealtimeSessionConfig(
        RealtimeModelV4SessionConfig config,
        string? modelId = null,
        bool update = false
    )
    {
        config = config ?? throw new ArgumentNullException(nameof(config));
        if (config.InputAudioFormatType != null || config.InputAudioFormatRate != null)
        {
            Unsupported("inputAudioFormat", update);
        }

        if (config.OutputAudioFormatType != null || config.OutputAudioFormatRate != null)
        {
            Unsupported("outputAudioFormat", update);
        }

        if (config.TurnDetection != null || config.TurnDetectionDisabled)
        {
            Unsupported("turnDetection", update);
        }

        if (update)
        {
            if (config.Voice != null)
            {
                Unsupported("voice", true);
            }

            if (config.InputAudioTranscription != null)
            {
                Unsupported("inputAudioTranscription", true);
            }

            if (config.OutputAudioTranscription != null)
            {
                Unsupported("outputAudioTranscription", true);
            }

            if (config.OutputModalities != null)
            {
                Unsupported("outputModalities", true);
            }
        }

        if (config.InputAudioTranscription?.Prompt != null)
        {
            Unsupported("inputAudioTranscription.prompt", update);
        }

        if (
            config.OutputAudioTranscription is { } output
            && (output.Model != null || output.Language != null || output.Prompt != null)
        )
        {
            Unsupported("outputAudioTranscription settings", update);
        }

        if (
            config.OutputModalities is { } modalities
            && (
                !modalities.Contains("audio")
                || modalities.Any(value => value != "audio" && value != "text")
            )
        )
        {
            Unsupported("outputModalities without audio", update);
        }

        var options =
            config.ProviderOptions == null
                ? null
                : Parse(JsonSerializer.SerializeToElement(config.ProviderOptions));
        if (update && options != null)
        {
            if (options.Stt != null)
            {
                Unsupported("providerOptions.mistral.stt", true);
            }

            if (options.Tts != null)
            {
                Unsupported("providerOptions.mistral.tts", true);
            }

            if (options.WelcomeMessage != null)
            {
                Unsupported("providerOptions.mistral.welcomeMessage", true);
            }

            if (options.AgentId != null)
            {
                Unsupported("providerOptions.mistral.agentId", true);
            }

            if (options.AgentVersion != null)
            {
                Unsupported("providerOptions.mistral.agentVersion", true);
            }
        }

        if (options?.AgentVersion != null && options.AgentId == null)
        {
            throw new InvalidArgumentException(
                "providerOptions.mistral.agentVersion",
                null,
                "Mistral agentVersion requires agentId."
            );
        }

        var body = new JsonObject();
        if (modelId != null)
        {
            body["llm"] = new JsonObject { ["model"] = modelId };
        }

        if (config.Instructions != null)
        {
            body["instructions"] = config.Instructions;
        }

        if (config.Tools != null)
        {
            var tools = new JsonArray();
            foreach (var tool in config.Tools)
            {
                var function = new JsonObject { ["name"] = tool.Name };
                if (tool.Description != null)
                {
                    function["description"] = tool.Description;
                }

                function["parameters"] =
                    tool.Parameters.ValueKind == JsonValueKind.Undefined
                        ? new JsonObject()
                        : JsonNode.Parse(tool.Parameters.GetRawText());
                tools.Add(new JsonObject { ["type"] = "function", ["function"] = function });
            }

            body["tools"] = tools;
        }

        var stt = options?.Stt;
        if (stt != null || config.InputAudioTranscription != null)
        {
            var node = new JsonObject();
            Set(node, "model", config.InputAudioTranscription?.Model ?? stt?.Model);
            Set(node, "language", config.InputAudioTranscription?.Language ?? stt?.Language);
            if (stt?.ContextBias != null)
            {
                node["context_bias"] = Strings(stt.ContextBias);
            }

            Set(node, "target_streaming_delay_ms", stt?.TargetStreamingDelayMs);
            if (stt?.TurnDetection is { } detection)
            {
                var turnDetection = new JsonObject();
                Set(turnDetection, "start_threshold", detection.StartThreshold);
                Set(turnDetection, "eager_end_threshold", detection.EagerEndThreshold);
                Set(turnDetection, "end_threshold", detection.EndThreshold);
                Set(turnDetection, "min_silence_ms", detection.MinSilenceMs);
                Set(turnDetection, "end_timeout_ms", detection.EndTimeoutMs);
                node["turn_detection"] = turnDetection;
            }

            body["stt"] = node;
        }

        var tts = options?.Tts;
        if (tts != null || config.Voice != null)
        {
            var node = new JsonObject();
            Set(node, "model", tts?.Model);
            Set(node, "voice_id", config.Voice ?? tts?.VoiceId);
            if (tts?.PhonemeHints != null)
            {
                var hints = new JsonObject();
                foreach (var pair in tts.PhonemeHints)
                {
                    hints[pair.Key] = Strings(pair.Value);
                }

                node["phoneme_hints"] = hints;
            }

            body["tts"] = node;
        }

        if (options?.Turn is { } turn)
        {
            var node = new JsonObject();
            Set(node, "allow_interruptions", turn.AllowInterruptions);
            if (turn.Inactivity is { } inactivity)
            {
                var inactivityNode = new JsonObject();
                Set(inactivityNode, "timeout_ms", inactivity.TimeoutMs);
                Set(inactivityNode, "max_reminders", inactivity.MaxReminders);
                if (inactivity.Messages != null)
                {
                    inactivityNode["messages"] = Strings(inactivity.Messages);
                }

                node["inactivity"] = inactivityNode;
            }

            body["turn"] = node;
        }

        Set(body, "welcome_message", options?.WelcomeMessage);
        Set(body, "agent_id", options?.AgentId);
        Set(body, "agent_version", options?.AgentVersion);
        return body;
    }

    private static void Unsupported(string setting, bool update)
    {
        var functionality =
            "Mistral voice agent " + (update ? "live update of " : "setting ") + setting;
        throw new UnsupportedFunctionalityException(
            functionality,
            "'" + functionality + "' functionality not supported."
        );
    }

    private static void Set(JsonObject node, string name, string? value)
    {
        if (value != null)
        {
            node[name] = value;
        }
    }

    private static void Set(JsonObject node, string name, int? value)
    {
        if (value != null)
        {
            node[name] = value;
        }
    }

    private static void Set(JsonObject node, string name, double? value)
    {
        if (value != null)
        {
            node[name] = value;
        }
    }

    private static void Set(JsonObject node, string name, bool? value)
    {
        if (value != null)
        {
            node[name] = value;
        }
    }

    private static JsonArray Strings(IEnumerable<string> values)
    {
        var array = new JsonArray();
        foreach (var value in values)
        {
            array.Add(value);
        }

        return array;
    }

    private static MistralRealtimeSttOptions ParseStt(JsonElement stt)
    {
        Strict(stt, "model", "language", "contextBias", "targetStreamingDelayMs", "turnDetection");
        return new MistralRealtimeSttOptions
        {
            Model = NonEmpty(stt, "model"),
            Language = NonEmpty(stt, "language"),
            ContextBias = NonEmptyStrings(stt, "contextBias"),
            TargetStreamingDelayMs = Integer(stt, "targetStreamingDelayMs", 80, 1200),
            TurnDetection = Object(stt, "turnDetection") is { } detection
                ? ParseTurnDetection(detection)
                : null,
        };
    }

    private static MistralRealtimeTurnDetection ParseTurnDetection(JsonElement detection)
    {
        Strict(
            detection,
            "startThreshold",
            "eagerEndThreshold",
            "endThreshold",
            "minSilenceMs",
            "endTimeoutMs"
        );
        return new MistralRealtimeTurnDetection
        {
            StartThreshold = Number(detection, "startThreshold", 0, 1),
            EagerEndThreshold = Number(detection, "eagerEndThreshold", 0, 1),
            EndThreshold = Number(detection, "endThreshold", 0, 1),
            MinSilenceMs = Integer(detection, "minSilenceMs", 0, int.MaxValue),
            EndTimeoutMs = Integer(detection, "endTimeoutMs", 0, int.MaxValue),
        };
    }

    private static MistralRealtimeTtsOptions ParseTts(JsonElement tts)
    {
        Strict(tts, "model", "voiceId", "phonemeHints");
        Dictionary<string, IReadOnlyList<string>>? hints = null;
        if (Object(tts, "phonemeHints") is { } hintsValue)
        {
            hints = [];
            foreach (var property in hintsValue.EnumerateObject())
            {
                var values = property.Value;
                if (values.ValueKind != JsonValueKind.Array || values.GetArrayLength() == 0)
                {
                    throw MistralOptionValues.Invalid(
                        values,
                        "phonemeHints entries must be non-empty arrays"
                    );
                }

                hints[property.Name] = values
                    .EnumerateArray()
                    .Select(item => MistralOptionValues.NonEmptyString(item, "phonemeHints"))
                    .ToList();
            }
        }

        return new MistralRealtimeTtsOptions
        {
            Model = NonEmpty(tts, "model"),
            VoiceId = NonEmpty(tts, "voiceId"),
            PhonemeHints = hints,
        };
    }

    private static MistralRealtimeTurnOptions ParseTurn(JsonElement turn)
    {
        Strict(turn, "allowInterruptions", "inactivity");
        bool? allow = null;
        if (MistralOptionValues.Get(turn, "allowInterruptions") is { } allowValue)
        {
            if (allowValue.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            {
                throw MistralOptionValues.Invalid(
                    allowValue,
                    "allowInterruptions must be a boolean"
                );
            }

            allow = allowValue.GetBoolean();
        }

        MistralRealtimeInactivityOptions? inactivity = null;
        if (Object(turn, "inactivity") is { } inactivityValue)
        {
            Strict(inactivityValue, "timeoutMs", "maxReminders", "messages");
            List<string>? messages = null;
            if (MistralOptionValues.Get(inactivityValue, "messages") is { } messagesValue)
            {
                if (messagesValue.ValueKind != JsonValueKind.Array)
                {
                    throw MistralOptionValues.Invalid(messagesValue, "messages must be an array");
                }

                messages = messagesValue
                    .EnumerateArray()
                    .Select(item => Text(item, "messages"))
                    .ToList();
            }

            inactivity = new MistralRealtimeInactivityOptions
            {
                TimeoutMs = Integer(inactivityValue, "timeoutMs", 0, int.MaxValue),
                MaxReminders = Integer(inactivityValue, "maxReminders", 0, int.MaxValue),
                Messages = messages,
            };
        }

        return new MistralRealtimeTurnOptions
        {
            AllowInterruptions = allow,
            Inactivity = inactivity,
        };
    }

    private static void Strict(JsonElement section, params string[] allowed)
    {
        foreach (var property in section.EnumerateObject())
        {
            if (!allowed.Contains(property.Name))
            {
                throw MistralOptionValues.Invalid(
                    property.Value,
                    "unrecognized key " + property.Name
                );
            }
        }
    }

    private static JsonElement? Object(JsonElement section, string name)
    {
        if (MistralOptionValues.Get(section, name) is not { } value)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.Object)
        {
            throw MistralOptionValues.Invalid(value, name + " must be an object");
        }

        return value;
    }

    private static string? NonEmpty(JsonElement section, string name)
    {
        return MistralOptionValues.Get(section, name) is { } value
            ? MistralOptionValues.NonEmptyString(value, name)
            : null;
    }

    private static string Text(JsonElement value, string name)
    {
        if (value.ValueKind != JsonValueKind.String)
        {
            throw MistralOptionValues.Invalid(value, name + " must be a string");
        }

        return value.GetString()!;
    }

    private static List<string>? NonEmptyStrings(JsonElement section, string name)
    {
        if (MistralOptionValues.Get(section, name) is not { } value)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.Array)
        {
            throw MistralOptionValues.Invalid(value, name + " must be an array");
        }

        return value
            .EnumerateArray()
            .Select(item => MistralOptionValues.NonEmptyString(item, name))
            .ToList();
    }

    private static int? Integer(JsonElement section, string name, int min, int max)
    {
        if (MistralOptionValues.Get(section, name) is not { } value)
        {
            return null;
        }

        if (
            value.ValueKind != JsonValueKind.Number
            || !value.TryGetInt32(out var number)
            || number < min
            || number > max
        )
        {
            throw MistralOptionValues.Invalid(
                value,
                name
                    + " must be an integer from "
                    + min
                    + (max == int.MaxValue ? " up" : " to " + max)
            );
        }

        return number;
    }

    private static double? Number(JsonElement section, string name, double min, double max)
    {
        if (MistralOptionValues.Get(section, name) is not { } value)
        {
            return null;
        }

        if (
            value.ValueKind != JsonValueKind.Number
            || value.GetDouble() < min
            || value.GetDouble() > max
        )
        {
            throw MistralOptionValues.Invalid(value, name + " must be a number from 0 to 1");
        }

        return value.GetDouble();
    }
}
