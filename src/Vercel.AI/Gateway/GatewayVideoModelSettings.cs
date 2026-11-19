// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Gateway;

/// <summary>Known AI Gateway model ids. Maps to <c>GatewayVideoModelId</c>; any other string is accepted too.</summary>
public static class GatewayVideoModelId
{
    /// <summary><c>alibaba/wan-v2.5-t2v-preview</c>.</summary>
    public const string AlibabaWanV25T2vPreview = "alibaba/wan-v2.5-t2v-preview";

    /// <summary><c>alibaba/wan-v2.6-i2v</c>.</summary>
    public const string AlibabaWanV26I2v = "alibaba/wan-v2.6-i2v";

    /// <summary><c>alibaba/wan-v2.6-i2v-flash</c>.</summary>
    public const string AlibabaWanV26I2vFlash = "alibaba/wan-v2.6-i2v-flash";

    /// <summary><c>alibaba/wan-v2.6-r2v</c>.</summary>
    public const string AlibabaWanV26R2v = "alibaba/wan-v2.6-r2v";

    /// <summary><c>alibaba/wan-v2.6-r2v-flash</c>.</summary>
    public const string AlibabaWanV26R2vFlash = "alibaba/wan-v2.6-r2v-flash";

    /// <summary><c>alibaba/wan-v2.6-t2v</c>.</summary>
    public const string AlibabaWanV26T2v = "alibaba/wan-v2.6-t2v";

    /// <summary><c>alibaba/wan-v2.7-r2v</c>.</summary>
    public const string AlibabaWanV27R2v = "alibaba/wan-v2.7-r2v";

    /// <summary><c>alibaba/wan-v2.7-t2v</c>.</summary>
    public const string AlibabaWanV27T2v = "alibaba/wan-v2.7-t2v";

    /// <summary><c>alibaba/wan-v3.0-video</c>.</summary>
    public const string AlibabaWanV30Video = "alibaba/wan-v3.0-video";

    /// <summary><c>alibaba/wan-v3.0-video-prime</c>.</summary>
    public const string AlibabaWanV30VideoPrime = "alibaba/wan-v3.0-video-prime";

    /// <summary><c>bfl/flux-3-video</c>.</summary>
    public const string BflFlux3Video = "bfl/flux-3-video";

    /// <summary><c>bytedance/seedance-2.0</c>.</summary>
    public const string BytedanceSeedance20 = "bytedance/seedance-2.0";

    /// <summary><c>bytedance/seedance-2.0-fast</c>.</summary>
    public const string BytedanceSeedance20Fast = "bytedance/seedance-2.0-fast";

    /// <summary><c>bytedance/seedance-2.0-mini</c>.</summary>
    public const string BytedanceSeedance20Mini = "bytedance/seedance-2.0-mini";

    /// <summary><c>bytedance/seedance-2.5</c>.</summary>
    public const string BytedanceSeedance25 = "bytedance/seedance-2.5";

    /// <summary><c>bytedance/seedance-2.5-flash</c>.</summary>
    public const string BytedanceSeedance25Flash = "bytedance/seedance-2.5-flash";

    /// <summary><c>bytedance/seedance-v1.0-pro</c>.</summary>
    public const string BytedanceSeedanceV10Pro = "bytedance/seedance-v1.0-pro";

    /// <summary><c>bytedance/seedance-v1.0-pro-fast</c>.</summary>
    public const string BytedanceSeedanceV10ProFast = "bytedance/seedance-v1.0-pro-fast";

    /// <summary><c>google/veo-3.1-fast-generate-001</c>.</summary>
    public const string GoogleVeo31FastGenerate001 = "google/veo-3.1-fast-generate-001";

    /// <summary><c>google/veo-3.1-generate-001</c>.</summary>
    public const string GoogleVeo31Generate001 = "google/veo-3.1-generate-001";

    /// <summary><c>google/veo-3.1-lite-generate-001</c>.</summary>
    public const string GoogleVeo31LiteGenerate001 = "google/veo-3.1-lite-generate-001";

    /// <summary><c>heygen/heygen-video-1</c>.</summary>
    public const string HeygenHeygenVideo1 = "heygen/heygen-video-1";

    /// <summary><c>klingai/kling-v2.5-turbo-i2v</c>.</summary>
    public const string KlingaiKlingV25TurboI2v = "klingai/kling-v2.5-turbo-i2v";

    /// <summary><c>klingai/kling-v2.5-turbo-t2v</c>.</summary>
    public const string KlingaiKlingV25TurboT2v = "klingai/kling-v2.5-turbo-t2v";

    /// <summary><c>klingai/kling-v2.6-i2v</c>.</summary>
    public const string KlingaiKlingV26I2v = "klingai/kling-v2.6-i2v";

    /// <summary><c>klingai/kling-v2.6-motion-control</c>.</summary>
    public const string KlingaiKlingV26MotionControl = "klingai/kling-v2.6-motion-control";

    /// <summary><c>klingai/kling-v2.6-t2v</c>.</summary>
    public const string KlingaiKlingV26T2v = "klingai/kling-v2.6-t2v";

    /// <summary><c>klingai/kling-v3.0-i2v</c>.</summary>
    public const string KlingaiKlingV30I2v = "klingai/kling-v3.0-i2v";

    /// <summary><c>klingai/kling-v3.0-motion-control</c>.</summary>
    public const string KlingaiKlingV30MotionControl = "klingai/kling-v3.0-motion-control";

    /// <summary><c>klingai/kling-v3.0-t2v</c>.</summary>
    public const string KlingaiKlingV30T2v = "klingai/kling-v3.0-t2v";

    /// <summary><c>minimax/minimax-h3</c>.</summary>
    public const string MinimaxMinimaxH3 = "minimax/minimax-h3";

    /// <summary><c>minimax/minimax-h3-max</c>.</summary>
    public const string MinimaxMinimaxH3Max = "minimax/minimax-h3-max";

    /// <summary><c>spacexai/grok-imagine-video</c>.</summary>
    public const string SpacexaiGrokImagineVideo = "spacexai/grok-imagine-video";

    /// <summary><c>spacexai/grok-imagine-video-1.5</c>.</summary>
    public const string SpacexaiGrokImagineVideo15 = "spacexai/grok-imagine-video-1.5";

    /// <summary><c>spacexai/grok-imagine-video-1.5-lite</c>.</summary>
    public const string SpacexaiGrokImagineVideo15Lite = "spacexai/grok-imagine-video-1.5-lite";

    /// <summary><c>topaz/proteus</c>.</summary>
    public const string TopazProteus = "topaz/proteus";

    /// <summary><c>topaz/starlight-precise-2.6</c>.</summary>
    public const string TopazStarlightPrecise26 = "topaz/starlight-precise-2.6";
}
