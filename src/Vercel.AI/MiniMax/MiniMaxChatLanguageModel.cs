// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.MiniMax;

/// <summary>MiniMax chat language model. Reuses the Anthropic Messages adapter and adds the M3.1 and later capabilities.</summary>
public sealed class MiniMaxChatLanguageModel : AnthropicLanguageModel
{
    // Compare version components as integers: M3.123 is newer than M3.1.
    // Assume later M-series models retain the M3.1 capabilities.
    private static readonly Regex VersionPattern = new Regex(
        @"^minimax-m(\d+)(?:\.(\d+))?(?:\.\d+)*(?:-|\z)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    private static readonly IReadOnlyDictionary<string, string> NativeUrls = new Dictionary<string, string>
    {
        ["image/*"] = "^https?://",
        ["video/*"] = "^https?://",
    };

    private readonly AnthropicProvider _messages;
    private readonly bool _supportsM31Features;

    /// <summary>Creates a model.</summary>
    public MiniMaxChatLanguageModel(AnthropicProvider provider, string modelId)
        : this(provider, modelId, SupportsM31(modelId))
    {
    }

    private MiniMaxChatLanguageModel(AnthropicProvider provider, string modelId, bool supportsM31Features)
        : base(provider, modelId, supportsM31Features ? Settings : null)
    {
        _messages = provider;
        _supportsM31Features = supportsM31Features;
    }

    private static AnthropicModelSettings Settings { get; } = new AnthropicModelSettings
    {
        ConvertMediaPart = MiniMaxMediaPart.Convert,
        MaxTemperature = 2,
        SupportsTemperatureAndTopPWithThinking = true,
        CacheControlProviderOptionsName = MiniMaxProvider.ProviderId,
    };

    /// <summary>
    /// Serializes the model id and config for a workflow boundary. Maps to <c>[WORKFLOW_SERIALIZE]</c>.
    /// The API key is not part of the serialized config. Pass it to <see cref="Deserialize"/>.
    /// </summary>
    public SerializedModelOptions Serialize()
    {
        var headers = new Dictionary<string, object?>();
        foreach (var pair in _messages.Options.Headers)
        {
            if (!IsSensitiveHeader(pair.Key))
            {
                headers[pair.Key] = pair.Value;
            }
        }

        headers["anthropic-version"] = _messages.Options.Version;
        var config = new Dictionary<string, object?>
        {
            ["provider"] = Provider,
            ["baseURL"] = _messages.NormalizedBase(),
            ["headers"] = headers,
        };
        return ModelOptionsSerialization.SerializeModelOptions(ModelId, config);
    }

    /// <summary>Restores a model from <see cref="Serialize"/>. Maps to <c>[WORKFLOW_DESERIALIZE]</c>.</summary>
    /// <param name="serialized">The serialized model id and config.</param>
    /// <param name="apiKey">API key. Defaults to the <c>MINIMAX_API_KEY</c> environment variable.</param>
    /// <param name="handler">HTTP handler. Pass one from tests.</param>
    public static MiniMaxChatLanguageModel Deserialize(SerializedModelOptions serialized, string? apiKey = null, HttpMessageHandler? handler = null)
    {
        if (serialized is null)
        {
            throw new ArgumentNullException(nameof(serialized));
        }

        var options = new MiniMaxOptions { ApiKey = apiKey };
        if (serialized.Config.TryGetValue("baseURL", out var baseUrl) && baseUrl is string url)
        {
            options.BaseUrl = url;
        }

        if (serialized.Config.TryGetValue("headers", out var headers) && headers is System.Collections.IDictionary map)
        {
            foreach (System.Collections.DictionaryEntry entry in map)
            {
                options.Headers[(string)entry.Key] = entry.Value as string;
            }
        }

        return MiniMaxProvider.Create(options, handler).Chat(serialized.ModelId);
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> SupportedUrls => _supportsM31Features ? NativeUrls : base.SupportedUrls;

    /// <inheritdoc />
    protected override LanguageModelCallOptions PrepareCallOptions(LanguageModelCallOptions options, IList<AnthropicWarning> warnings)
    {
        var reasoning = options.Reasoning;
        if (!_supportsM31Features
            || !AnthropicReasoning.IsCustom(reasoning)
            || HasEffort(options, MiniMaxProvider.ProviderId)
            || HasEffort(options, "anthropic"))
        {
            return options;
        }

        string? effort;
        if (reasoning == "none")
        {
            warnings.Add(new AnthropicWarning(
                "compatibility",
                "reasoning",
                "reasoning 'none' is not supported by " + ModelId + "; it always reasons. Using effort 'low' to minimize thinking instead."));
            effort = "low";
        }
        else
        {
            effort = MapEffort(reasoning!, warnings);
        }

        var minimax = new JsonObject();
        if (options.ProviderOptions != null && options.ProviderOptions.TryGetValue(MiniMaxProvider.ProviderId, out var existing)
            && existing.ValueKind == JsonValueKind.Object && JsonNode.Parse(existing.GetRawText()) is JsonObject existingObject)
        {
            minimax = existingObject;
        }

        if (effort != null)
        {
            minimax["effort"] = effort;
        }

        var providerOptions = options.ProviderOptions == null
            ? new Dictionary<string, JsonElement>()
            : new Dictionary<string, JsonElement>(options.ProviderOptions.ToDictionary(pair => pair.Key, pair => pair.Value));
        using (var document = JsonDocument.Parse(minimax.ToJsonString()))
        {
            providerOptions[MiniMaxProvider.ProviderId] = document.RootElement.Clone();
        }

        var prepared = options.Copy();
        prepared.ProviderOptions = providerOptions;
        // M3.1 and later use effort instead of Anthropic thinking budgets.
        prepared.Reasoning = null;
        return prepared;
    }

    private static bool IsSensitiveHeader(string name)
    {
        foreach (var marker in new[] { "authorization", "key", "token", "secret", "cookie" })
        {
            if (name.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static bool SupportsM31(string modelId)
    {
        var version = VersionPattern.Match(modelId);
        if (!version.Success)
        {
            return false;
        }

        var major = double.Parse(version.Groups[1].Value, CultureInfo.InvariantCulture);
        var minor = version.Groups[2].Success ? double.Parse(version.Groups[2].Value, CultureInfo.InvariantCulture) : 0;
        return major > 3 || (major == 3 && minor >= 1);
    }

    private static bool HasEffort(LanguageModelCallOptions options, string provider)
    {
        return options.ProviderOptions != null
            && options.ProviderOptions.TryGetValue(provider, out var value)
            && value.ValueKind == JsonValueKind.Object
            && value.TryGetProperty("effort", out var effort)
            && effort.ValueKind != JsonValueKind.Null;
    }

    private static string? MapEffort(string reasoning, IList<AnthropicWarning> warnings)
    {
        string mapped;
        switch (reasoning)
        {
            case "minimal":
            case "low":
                mapped = "low";
                break;
            case "medium":
            case "high":
            case "xhigh":
            case "max":
                mapped = reasoning;
                break;
            default:
                warnings.Add(new AnthropicWarning("unsupported", "reasoning", "reasoning \"" + reasoning + "\" is not supported by this model."));
                return null;
        }

        if (mapped != reasoning)
        {
            warnings.Add(new AnthropicWarning(
                "compatibility",
                "reasoning",
                "reasoning \"" + reasoning + "\" is not directly supported by this model. mapped to effort \"" + mapped + "\"."));
        }

        return mapped;
    }
}
