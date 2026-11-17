// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;

namespace Vercel.AI.Zai;

/// <summary>Reasoning effort. Maps to the <c>reasoningEffort</c> enum of <c>ZaiLanguageModelChatOptions</c>.</summary>
public enum ZaiReasoningEffort
{
    /// <summary><c>none</c>.</summary>
    None,

    /// <summary><c>minimal</c>.</summary>
    Minimal,

    /// <summary><c>low</c>.</summary>
    Low,

    /// <summary><c>medium</c>.</summary>
    Medium,

    /// <summary><c>high</c>.</summary>
    High,

    /// <summary><c>xhigh</c>.</summary>
    XHigh,

    /// <summary><c>max</c>.</summary>
    Max,
}

/// <summary>Thinking mode. Maps to the <c>thinking.type</c> enum of <c>ZaiLanguageModelChatOptions</c>.</summary>
public enum ZaiThinkingType
{
    /// <summary><c>enabled</c>.</summary>
    Enabled,

    /// <summary><c>disabled</c>.</summary>
    Disabled,
}

/// <summary>Controls model thinking. Maps to the <c>thinking</c> option of <c>ZaiLanguageModelChatOptions</c>.</summary>
public sealed class ZaiThinking
{
    /// <summary>Enables or disables thinking.</summary>
    public ZaiThinkingType? Type { get; set; }

    /// <summary>Whether reasoning from earlier turns is cleared.</summary>
    public bool? ClearThinking { get; set; }
}

/// <summary>The <c>zai</c> provider options of a chat call. Maps to <c>ZaiLanguageModelChatOptions</c>.</summary>
public sealed class ZaiLanguageModelChatOptions
{
    /// <summary>Enables or disables sampling. When disabled, temperature and topP do not take effect.</summary>
    public bool? DoSample { get; set; }

    /// <summary>Controls model thinking and whether reasoning from earlier turns is kept.</summary>
    public ZaiThinking? Thinking { get; set; }

    /// <summary>Controls reasoning effort for GLM-5.2 and later models.</summary>
    public ZaiReasoningEffort? ReasoningEffort { get; set; }

    /// <summary>Enables incremental function-call argument streaming on supported models.</summary>
    public bool? ToolStream { get; set; }

    /// <summary>A caller-provided request identifier between 6 and 64 characters.</summary>
    public string? RequestId { get; set; }

    /// <summary>A non-sensitive end-user identifier between 6 and 128 characters.</summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Validates the options and returns them as call provider options under <paramref name="providerOptionsName"/>
    /// (<c>zai</c> by default), ready for <c>LanguageModelCallOptions.ProviderOptions</c>.
    /// </summary>
    /// <exception cref="InvalidArgumentException">An option has the wrong value.</exception>
    public IReadOnlyDictionary<string, JsonElement> ToProviderOptions(
        string providerOptionsName = ZaiProvider.ProviderId
    )
    {
        Check(
            RequestId is null || (RequestId.Length >= 6 && RequestId.Length <= 64),
            "requestId",
            "must be between 6 and 64 characters."
        );
        Check(
            UserId is null || (UserId.Length >= 6 && UserId.Length <= 128),
            "userId",
            "must be between 6 and 128 characters."
        );

        var json = new JsonObject();
        Set(json, "doSample", DoSample);
        if (Thinking is not null)
        {
            var thinking = new JsonObject();
            Set(thinking, "type", Thinking.Type?.ToWireValue());
            Set(thinking, "clearThinking", Thinking.ClearThinking);
            json["thinking"] = thinking;
        }

        Set(json, "reasoningEffort", ReasoningEffort?.ToWireValue());
        Set(json, "toolStream", ToolStream);
        Set(json, "requestId", RequestId);
        Set(json, "userId", UserId);
        return new Dictionary<string, JsonElement>
        {
            [providerOptionsName] = JsonSerializer.SerializeToElement(json),
        };
    }

    private static void Check(bool valid, string name, string detail)
    {
        if (!valid)
        {
            throw new InvalidArgumentException("providerOptions", name, name + " " + detail);
        }
    }

    private static void Set<T>(JsonObject json, string name, T? value)
    {
        if (value is not null)
        {
            json[name] = JsonValue.Create(value);
        }
    }
}

internal static class ZaiOptionEnumExtensions
{
    internal static string ToWireValue(this ZaiReasoningEffort value) =>
        value.ToString().ToLowerInvariant();

    internal static string ToWireValue(this ZaiThinkingType value) =>
        value.ToString().ToLowerInvariant();
}
