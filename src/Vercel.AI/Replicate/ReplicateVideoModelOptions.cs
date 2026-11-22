// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Replicate;

/// <summary>
/// Replicate video provider options, read from <c>providerOptions.replicate</c>.
/// A null property is the same as an omitted one. Options this class does not model are sent as they are.
/// </summary>
public sealed class ReplicateVideoModelOptions
{
    /// <summary>Poll interval in milliseconds. Must be positive.</summary>
    public double? PollIntervalMs { get; set; }

    /// <summary>Poll timeout in milliseconds. Must be positive.</summary>
    public double? PollTimeoutMs { get; set; }

    /// <summary>Maximum wait in seconds. Must be positive.</summary>
    public double? MaxWaitTimeInSeconds { get; set; }

    /// <summary>Sent as <c>guidance_scale</c>.</summary>
    public double? GuidanceScale { get; set; }

    /// <summary>Sent as <c>num_inference_steps</c>.</summary>
    public double? NumInferenceSteps { get; set; }

    /// <summary>Stable Video Diffusion. Sent as <c>motion_bucket_id</c>.</summary>
    public double? MotionBucketId { get; set; }

    /// <summary>Stable Video Diffusion. Sent as <c>cond_aug</c>.</summary>
    public double? CondAug { get; set; }

    /// <summary>Stable Video Diffusion. Sent as <c>decoding_t</c>.</summary>
    public double? DecodingT { get; set; }

    /// <summary>Stable Video Diffusion. Sent as <c>video_length</c>.</summary>
    public string? VideoLength { get; set; }

    /// <summary>Stable Video Diffusion. Sent as <c>sizing_strategy</c>.</summary>
    public string? SizingStrategy { get; set; }

    /// <summary>Stable Video Diffusion. Sent as <c>frames_per_second</c>.</summary>
    public double? FramesPerSecond { get; set; }

    /// <summary>MiniMax. Sent as <c>prompt_optimizer</c>.</summary>
    public bool? PromptOptimizer { get; set; }

    /// <summary>Options this class does not model, in order. They are sent to the API as they are. Upstream passthrough.</summary>
    public IReadOnlyDictionary<string, JsonElement> AdditionalProperties { get; set; } =
        new Dictionary<string, JsonElement>();

    /// <summary>
    /// Reads <c>providerOptions.replicate</c>. Returns null when absent. An invalid value throws <see cref="InvalidArgumentException"/>.
    /// </summary>
    public static ReplicateVideoModelOptions? Parse(JsonElement providerOptions)
    {
        if (
            providerOptions.ValueKind != JsonValueKind.Object
            || !providerOptions.TryGetProperty(ReplicateProvider.ProviderId, out var raw)
            || raw.ValueKind == JsonValueKind.Null
        )
        {
            return null;
        }

        if (raw.ValueKind != JsonValueKind.Object)
        {
            throw Invalid("expected an object.");
        }

        var options = new ReplicateVideoModelOptions();
        var additional = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var property in raw.EnumerateObject())
        {
            switch (property.Name)
            {
                case "pollIntervalMs":
                    options.PollIntervalMs = Number(property, positive: true);
                    break;
                case "pollTimeoutMs":
                    options.PollTimeoutMs = Number(property, positive: true);
                    break;
                case "maxWaitTimeInSeconds":
                    options.MaxWaitTimeInSeconds = Number(property, positive: true);
                    break;
                case "guidance_scale":
                    options.GuidanceScale = Number(property);
                    break;
                case "num_inference_steps":
                    options.NumInferenceSteps = Number(property);
                    break;
                case "motion_bucket_id":
                    options.MotionBucketId = Number(property);
                    break;
                case "cond_aug":
                    options.CondAug = Number(property);
                    break;
                case "decoding_t":
                    options.DecodingT = Number(property);
                    break;
                case "video_length":
                    options.VideoLength = Text(property);
                    break;
                case "sizing_strategy":
                    options.SizingStrategy = Text(property);
                    break;
                case "frames_per_second":
                    options.FramesPerSecond = Number(property);
                    break;
                case "prompt_optimizer":
                    options.PromptOptimizer = Boolean(property);
                    break;
                default:
                    additional[property.Name] = property.Value.Clone();
                    break;
            }
        }

        options.AdditionalProperties = additional;
        return options;
    }

    private static InvalidArgumentException Invalid(string message)
    {
        return new InvalidArgumentException(
            "providerOptions.replicate",
            null,
            "Invalid Replicate video provider options: " + message
        );
    }

    private static double? Number(JsonProperty property, bool positive = false)
    {
        if (property.Value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (property.Value.ValueKind != JsonValueKind.Number)
        {
            throw Invalid(property.Name + " must be a number.");
        }

        var value = property.Value.GetDouble();
        if (positive && value <= 0)
        {
            throw Invalid(property.Name + " must be positive.");
        }

        return value;
    }

    private static string? Text(JsonProperty property)
    {
        return property.Value.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.String => property.Value.GetString(),
            _ => throw Invalid(property.Name + " must be a string."),
        };
    }

    private static bool? Boolean(JsonProperty property)
    {
        return property.Value.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => throw Invalid(property.Name + " must be a boolean."),
        };
    }
}
