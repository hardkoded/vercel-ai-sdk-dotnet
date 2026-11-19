// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Gateway;

/// <summary>Known AI Gateway model ids. Maps to <c>GatewayImageModelId</c>; any other string is accepted too.</summary>
public static class GatewayImageModelId
{
    /// <summary><c>bfl/flux-2-flex</c>.</summary>
    public const string BflFlux2Flex = "bfl/flux-2-flex";

    /// <summary><c>bfl/flux-2-klein-4b</c>.</summary>
    public const string BflFlux2Klein4b = "bfl/flux-2-klein-4b";

    /// <summary><c>bfl/flux-2-klein-9b</c>.</summary>
    public const string BflFlux2Klein9b = "bfl/flux-2-klein-9b";

    /// <summary><c>bfl/flux-2-max</c>.</summary>
    public const string BflFlux2Max = "bfl/flux-2-max";

    /// <summary><c>bfl/flux-2-pro</c>.</summary>
    public const string BflFlux2Pro = "bfl/flux-2-pro";

    /// <summary><c>bfl/flux-3-image</c>.</summary>
    public const string BflFlux3Image = "bfl/flux-3-image";

    /// <summary><c>bfl/flux-3-image-klein</c>.</summary>
    public const string BflFlux3ImageKlein = "bfl/flux-3-image-klein";

    /// <summary><c>bfl/flux-kontext-max</c>.</summary>
    public const string BflFluxKontextMax = "bfl/flux-kontext-max";

    /// <summary><c>bfl/flux-kontext-pro</c>.</summary>
    public const string BflFluxKontextPro = "bfl/flux-kontext-pro";

    /// <summary><c>bfl/flux-pro-1.0-fill</c>.</summary>
    public const string BflFluxPro10Fill = "bfl/flux-pro-1.0-fill";

    /// <summary><c>bfl/flux-pro-1.1</c>.</summary>
    public const string BflFluxPro11 = "bfl/flux-pro-1.1";

    /// <summary><c>bfl/flux-pro-1.1-ultra</c>.</summary>
    public const string BflFluxPro11Ultra = "bfl/flux-pro-1.1-ultra";

    /// <summary><c>bytedance/seedream-4.0</c>.</summary>
    public const string BytedanceSeedream40 = "bytedance/seedream-4.0";

    /// <summary><c>bytedance/seedream-4.5</c>.</summary>
    public const string BytedanceSeedream45 = "bytedance/seedream-4.5";

    /// <summary><c>bytedance/seedream-5.0-lite</c>.</summary>
    public const string BytedanceSeedream50Lite = "bytedance/seedream-5.0-lite";

    /// <summary><c>bytedance/seedream-5.0-pro</c>.</summary>
    public const string BytedanceSeedream50Pro = "bytedance/seedream-5.0-pro";

    /// <summary><c>bytedance/seedream-5.5</c>.</summary>
    public const string BytedanceSeedream55 = "bytedance/seedream-5.5";

    /// <summary><c>meta/muse-image-1.0</c>.</summary>
    public const string MetaMuseImage10 = "meta/muse-image-1.0";

    /// <summary><c>microsoft/mai-image-2.6</c>.</summary>
    public const string MicrosoftMaiImage26 = "microsoft/mai-image-2.6";

    /// <summary><c>microsoft/mai-image-2.6-flash</c>.</summary>
    public const string MicrosoftMaiImage26Flash = "microsoft/mai-image-2.6-flash";

    /// <summary><c>microsoft/mai-image-2.7</c>.</summary>
    public const string MicrosoftMaiImage27 = "microsoft/mai-image-2.7";

    /// <summary><c>microsoft/mai-image-2.7-flash</c>.</summary>
    public const string MicrosoftMaiImage27Flash = "microsoft/mai-image-2.7-flash";

    /// <summary><c>minimax/minimax-h3-image</c>.</summary>
    public const string MinimaxMinimaxH3Image = "minimax/minimax-h3-image";

    /// <summary><c>minimax/minimax-h3-image-highspeed</c>.</summary>
    public const string MinimaxMinimaxH3ImageHighspeed = "minimax/minimax-h3-image-highspeed";

    /// <summary><c>openai/gpt-image-1-mini</c>.</summary>
    public const string OpenaiGptImage1Mini = "openai/gpt-image-1-mini";

    /// <summary><c>openai/gpt-image-1.5</c>.</summary>
    public const string OpenaiGptImage15 = "openai/gpt-image-1.5";

    /// <summary><c>openai/gpt-image-2</c>.</summary>
    public const string OpenaiGptImage2 = "openai/gpt-image-2";

    /// <summary><c>openai/gpt-image-2.5-flare</c>.</summary>
    public const string OpenaiGptImage25Flare = "openai/gpt-image-2.5-flare";

    /// <summary><c>openai/gpt-image-2.5-sunburst</c>.</summary>
    public const string OpenaiGptImage25Sunburst = "openai/gpt-image-2.5-sunburst";

    /// <summary><c>openai/gpt-image-2.6-flare</c>.</summary>
    public const string OpenaiGptImage26Flare = "openai/gpt-image-2.6-flare";

    /// <summary><c>openai/gpt-image-2.6-sunburst</c>.</summary>
    public const string OpenaiGptImage26Sunburst = "openai/gpt-image-2.6-sunburst";

    /// <summary><c>prodia/flux-fast-schnell</c>.</summary>
    public const string ProdiaFluxFastSchnell = "prodia/flux-fast-schnell";

    /// <summary><c>recraft/recraft-v2</c>.</summary>
    public const string RecraftRecraftV2 = "recraft/recraft-v2";

    /// <summary><c>recraft/recraft-v3</c>.</summary>
    public const string RecraftRecraftV3 = "recraft/recraft-v3";

    /// <summary><c>recraft/recraft-v4</c>.</summary>
    public const string RecraftRecraftV4 = "recraft/recraft-v4";

    /// <summary><c>recraft/recraft-v4-pro</c>.</summary>
    public const string RecraftRecraftV4Pro = "recraft/recraft-v4-pro";

    /// <summary><c>recraft/recraft-v4.1</c>.</summary>
    public const string RecraftRecraftV41 = "recraft/recraft-v4.1";

    /// <summary><c>recraft/recraft-v4.1-flash</c>.</summary>
    public const string RecraftRecraftV41Flash = "recraft/recraft-v4.1-flash";

    /// <summary><c>recraft/recraft-v4.1-pro</c>.</summary>
    public const string RecraftRecraftV41Pro = "recraft/recraft-v4.1-pro";

    /// <summary><c>recraft/recraft-v4.1-utility</c>.</summary>
    public const string RecraftRecraftV41Utility = "recraft/recraft-v4.1-utility";

    /// <summary><c>recraft/recraft-v4.1-utility-pro</c>.</summary>
    public const string RecraftRecraftV41UtilityPro = "recraft/recraft-v4.1-utility-pro";

    /// <summary><c>spacexai/grok-imagine-image</c>.</summary>
    public const string SpacexaiGrokImagineImage = "spacexai/grok-imagine-image";

    /// <summary><c>spacexai/grok-imagine-image-2.0</c>.</summary>
    public const string SpacexaiGrokImagineImage20 = "spacexai/grok-imagine-image-2.0";

    /// <summary><c>topaz/wonder-3.5</c>.</summary>
    public const string TopazWonder35 = "topaz/wonder-3.5";
}
