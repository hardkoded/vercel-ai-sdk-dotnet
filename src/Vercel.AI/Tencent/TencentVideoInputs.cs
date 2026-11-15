// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tencent;

/// <summary>Shared video input helpers. Upstream <c>tencent-video-inputs.ts</c>.</summary>
internal static class TencentVideoInputs
{
    internal static string GetFileBase64(VideoModelFile file)
    {
        return Convert.ToBase64String(file.Data ?? Array.Empty<byte>());
    }

    internal static string ToUrlOrDataUri(VideoModelFile file)
    {
        return file.Type == "url" ? file.Url! : "data:" + file.MediaType + ";base64," + GetFileBase64(file);
    }

    internal static bool IsVideoFile(VideoModelFile file) => GetMediaKind(file) == "video";

    internal static bool IsAudioFile(VideoModelFile file) => GetMediaKind(file) == "audio";

    internal static VideoModelFile? ResolveFirstFrameImage(VideoModelCall options, List<OperationWarning> warnings)
    {
        var frame = options.FrameImages?.FirstOrDefault(entry => entry.FrameType == "first_frame")?.Image ?? options.Image;
        return RejectNonImageFrame(frame, warnings);
    }

    internal static VideoModelFile? ResolveLastFrameImage(VideoModelCall options, List<OperationWarning> warnings)
    {
        return RejectNonImageFrame(options.FrameImages?.FirstOrDefault(entry => entry.FrameType == "last_frame")?.Image, warnings);
    }

    internal static (List<VideoModelFile> Images, List<VideoModelFile> Videos, List<VideoModelFile> Audios) SplitInputReferences(VideoModelCall options)
    {
        var images = new List<VideoModelFile>();
        var videos = new List<VideoModelFile>();
        var audios = new List<VideoModelFile>();
        foreach (var reference in options.InputReferences ?? [])
        {
            if (IsVideoFile(reference))
            {
                videos.Add(reference);
            }
            else if (IsAudioFile(reference))
            {
                audios.Add(reference);
            }
            else
            {
                images.Add(reference);
            }
        }

        return (images, videos, audios);
    }

    /// <summary>Maps a <c>{width}x{height}</c> resolution to a provider tier keyed by the shorter side.</summary>
    internal static string? MapResolutionToTier(string resolution, IReadOnlyDictionary<int, string> tiers)
    {
        var parts = resolution.Split('x');
        if (parts.Length != 2
            || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var width)
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var height))
        {
            return null;
        }

        return tiers.TryGetValue(Math.Min(width, height), out var tier) ? tier : null;
    }

    internal static string? ResolveResolution(VideoModelCall options, TencentVideoModelOptions? tencentOptions, IReadOnlyDictionary<int, string> tiers, List<OperationWarning> warnings)
    {
        if (tencentOptions?.Resolution != null)
        {
            return tencentOptions.Resolution;
        }

        if (options.Resolution == null)
        {
            return null;
        }

        var tier = MapResolutionToTier(options.Resolution, tiers);
        if (tier == null)
        {
            warnings.Add(OperationWarning.Unsupported(
                "resolution",
                "Resolution " + options.Resolution + " does not map to a supported tier (" + string.Join(", ", tiers.Values) + "); the default resolution was used."));
        }

        return tier;
    }

    internal static void WarnIgnoredProviderOptions(TencentVideoModelOptions? tencentOptions, IReadOnlyList<string> keys, string details, List<OperationWarning> warnings)
    {
        if (tencentOptions == null)
        {
            return;
        }

        foreach (var key in keys)
        {
            if (tencentOptions.IsSet(key))
            {
                warnings.Add(OperationWarning.Unsupported("providerOptions.tencent." + key, details));
            }
        }
    }

    internal static void WarnUnsupportedCommonOptions(VideoModelCall options, bool supportsSeed, List<OperationWarning> warnings)
    {
        if (options.Fps != null)
        {
            warnings.Add(OperationWarning.Unsupported("fps", "Tencent video models do not support custom FPS."));
        }

        if (options.N > 1)
        {
            warnings.Add(OperationWarning.Unsupported("n", "Tencent video models generate one video per call. Only 1 video will be generated."));
        }

        if (!supportsSeed && options.Seed != null)
        {
            warnings.Add(OperationWarning.Unsupported("seed", "TokenHub Kling models do not support a seed."));
        }

        if (options.WebhookUrl != null)
        {
            warnings.Add(OperationWarning.Unsupported("webhookUrl", "Tencent video tasks do not accept a per-request callback URL; the result is retrieved by polling."));
        }
    }

    private static string? GetMediaKind(VideoModelFile file)
    {
        return file.MediaType != null ? MediaTypes.GetTopLevelMediaType(file.MediaType) : null;
    }

    private static VideoModelFile? RejectNonImageFrame(VideoModelFile? file, List<OperationWarning> warnings)
    {
        if (file != null && (IsVideoFile(file) || IsAudioFile(file)))
        {
            warnings.Add(OperationWarning.Unsupported("frameImages", "Frame images must be images; the frame was ignored."));
            return null;
        }

        return file;
    }
}
