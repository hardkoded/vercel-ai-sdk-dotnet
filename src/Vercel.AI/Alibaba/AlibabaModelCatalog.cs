// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.RegularExpressions;

namespace Vercel.AI.Alibaba;

/// <summary>
/// One row of the video model matrix. The protocol and the mode together decide which inputs and
/// provider options a model accepts. Maps to <c>AlibabaVideoModelRow</c>.
/// </summary>
/// <param name="Protocol">
/// Request protocol: <c>legacy</c> (wan2.6 and earlier: <c>parameters.size</c>, <c>input.img_url</c>, and
/// <c>input.reference_urls</c>), <c>wan27</c> (resolution tiers and <c>ratio</c>, <c>input.media</c>, no
/// <c>shot_type</c>, audio always on), or <c>wan3</c> (like <c>wan27</c>, plus a real <c>last_frame</c> slot, an
/// <c>audio</c> toggle, a 480P tier, and one model ID covering every mode).
/// </param>
/// <param name="Mode">
/// <c>t2v</c>, <c>i2v</c>, or <c>r2v</c>: the mode a model ID is fixed to. A <c>wan3</c> model takes its mode
/// from the media of the request instead, and its row uses <c>t2v</c>.
/// </param>
/// <param name="AudioMode">Documented audio generation. Null: not documented.</param>
/// <param name="ResolutionTiers">Documented resolution tiers. Null: not documented.</param>
/// <param name="DurationRange">Documented duration range. Null: not documented.</param>
internal sealed record AlibabaVideoModelRow(
    string Protocol,
    string Mode,
    string? AudioMode,
    IReadOnlyList<string>? ResolutionTiers,
    string? DurationRange
);

/// <summary>The <c>requestConfig.video</c> of a video model entry.</summary>
internal sealed record AlibabaVideoRequestConfig(string Mode, string Protocol);

/// <summary>The entry <c>createVideoModelEntry</c> builds. Only <c>requestConfig</c> is read by the model.</summary>
internal sealed record AlibabaVideoModelEntry(AlibabaVideoRequestConfig RequestConfig);

/// <summary>Maps to <c>alibaba-model-catalog.ts</c> (video models).</summary>
internal static class AlibabaModelCatalog
{
    private static readonly Regex VersionPattern = new(
        @"^wanx?(\d+)(?:\.(\d+))?(?=[.-]|\z)",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1)
    );

    private static readonly Dictionary<string, AlibabaVideoModelRow> VideoModelDefinitions = new(
        StringComparer.Ordinal
    )
    {
        ["wan2.6-t2v"] = new("legacy", "t2v", "Yes", ["720P", "1080P"], "2-15s"),
        ["wan2.5-t2v-preview"] = new("legacy", "t2v", "Yes", ["480P", "720P", "1080P"], "5s, 10s"),
        ["wan2.7-t2v"] = new("wan27", "t2v", "Yes", ["720P", "1080P"], "2-15s"),
        ["wan2.7-t2v-2026-06-12"] = new("wan27", "t2v", "Yes", ["720P", "1080P"], "2-15s"),
        ["wan2.6-i2v"] = new("legacy", "i2v", "Yes", ["720P", "1080P"], "2-15s"),
        ["wan2.6-i2v-flash"] = new("legacy", "i2v", "Optional", ["720P", "1080P"], "2-15s"),
        ["wan2.6-r2v"] = new("legacy", "r2v", "Yes", ["720P", "1080P"], "2-10s"),
        ["wan2.6-r2v-flash"] = new("legacy", "r2v", "Optional", ["720P", "1080P"], "2-10s"),
        ["wan2.7-r2v"] = new(
            "wan27",
            "r2v",
            "Yes",
            ["720P", "1080P"],
            "2-10s (with video ref), 2-15s"
        ),
        ["wan2.7-r2v-2026-06-12"] = new(
            "wan27",
            "r2v",
            "Yes",
            ["720P", "1080P"],
            "2-10s (with video ref), 2-15s"
        ),
        ["wan3.0-video"] = new("wan3", "t2v", null, null, null),
    };

    /// <summary>Maps to <c>getAlibabaVideoModelEntry</c>.</summary>
    public static AlibabaVideoModelEntry GetAlibabaVideoModelEntry(string modelId)
    {
        return CreateVideoModelEntry(
            VideoModelDefinitions.TryGetValue(modelId, out var row)
                ? row
                : GetUnlistedVideoModelRow(modelId)
        );
    }

    // Maps a matrix row to its entry. Nothing here inspects the model ID.
    private static AlibabaVideoModelEntry CreateVideoModelEntry(AlibabaVideoModelRow row)
    {
        return new AlibabaVideoModelEntry(new AlibabaVideoRequestConfig(row.Mode, row.Protocol));
    }

    // Resolves a video model ID that is not listed in the matrix to a row, from the mode and the
    // version that the model ID names.
    private static AlibabaVideoModelRow GetUnlistedVideoModelRow(string modelId)
    {
        return new AlibabaVideoModelRow(
            GetUnlistedVideoProtocol(modelId),
            modelId.Contains("-i2v") ? "i2v"
                : modelId.Contains("-r2v") ? "r2v"
                : "t2v",
            null,
            null,
            null
        );
    }

    private static string GetUnlistedVideoProtocol(string modelId)
    {
        // `wanx` is the older id prefix (e.g. `wanx2.1-t2v-turbo`).
        var version = VersionPattern.Match(modelId);
        if (version.Success)
        {
            var major = double.Parse(version.Groups[1].Value, CultureInfo.InvariantCulture);
            var minor = version.Groups[2].Success
                ? double.Parse(version.Groups[2].Value, CultureInfo.InvariantCulture)
                : 0;
            if (major < 2 || (major == 2 && minor < 7))
            {
                return "legacy";
            }

            if (major == 2)
            {
                return "wan27";
            }
        }

        // Unrecognized IDs inherit the latest all-in-one media protocol.
        return "wan3";
    }
}
