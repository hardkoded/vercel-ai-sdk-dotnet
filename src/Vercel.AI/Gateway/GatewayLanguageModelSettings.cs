// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Gateway;

/// <summary>Known AI Gateway model ids. Maps to <c>GatewayModelId</c>; any other string is accepted too.</summary>
public static class GatewayModelId
{
    /// <summary><c>alibaba/qwen-3-14b</c>.</summary>
    public const string AlibabaQwen314b = "alibaba/qwen-3-14b";

    /// <summary><c>alibaba/qwen-3-30b</c>.</summary>
    public const string AlibabaQwen330b = "alibaba/qwen-3-30b";

    /// <summary><c>alibaba/qwen-3-32b</c>.</summary>
    public const string AlibabaQwen332b = "alibaba/qwen-3-32b";

    /// <summary><c>alibaba/qwen3-coder-30b-a3b</c>.</summary>
    public const string AlibabaQwen3Coder30bA3b = "alibaba/qwen3-coder-30b-a3b";

    /// <summary><c>alibaba/qwen3-coder-next</c>.</summary>
    public const string AlibabaQwen3CoderNext = "alibaba/qwen3-coder-next";

    /// <summary><c>alibaba/qwen3-max-thinking</c>.</summary>
    public const string AlibabaQwen3MaxThinking = "alibaba/qwen3-max-thinking";

    /// <summary><c>alibaba/qwen3-next-80b-a3b-instruct</c>.</summary>
    public const string AlibabaQwen3Next80bA3bInstruct = "alibaba/qwen3-next-80b-a3b-instruct";

    /// <summary><c>alibaba/qwen3-vl-instruct</c>.</summary>
    public const string AlibabaQwen3VlInstruct = "alibaba/qwen3-vl-instruct";

    /// <summary><c>alibaba/qwen3.5-flash</c>.</summary>
    public const string AlibabaQwen35Flash = "alibaba/qwen3.5-flash";

    /// <summary><c>alibaba/qwen3.5-plus</c>.</summary>
    public const string AlibabaQwen35Plus = "alibaba/qwen3.5-plus";

    /// <summary><c>alibaba/qwen3.6-27b</c>.</summary>
    public const string AlibabaQwen3627b = "alibaba/qwen3.6-27b";

    /// <summary><c>alibaba/qwen3.6-35b-a3b</c>.</summary>
    public const string AlibabaQwen3635bA3b = "alibaba/qwen3.6-35b-a3b";

    /// <summary><c>alibaba/qwen3.6-plus</c>.</summary>
    public const string AlibabaQwen36Plus = "alibaba/qwen3.6-plus";

    /// <summary><c>alibaba/qwen3.7-flash</c>.</summary>
    public const string AlibabaQwen37Flash = "alibaba/qwen3.7-flash";

    /// <summary><c>alibaba/qwen3.7-max</c>.</summary>
    public const string AlibabaQwen37Max = "alibaba/qwen3.7-max";

    /// <summary><c>alibaba/qwen3.7-plus</c>.</summary>
    public const string AlibabaQwen37Plus = "alibaba/qwen3.7-plus";

    /// <summary><c>alibaba/qwen3.8-2.4t-a95b</c>.</summary>
    public const string AlibabaQwen3824tA95b = "alibaba/qwen3.8-2.4t-a95b";

    /// <summary><c>alibaba/qwen3.8-27b</c>.</summary>
    public const string AlibabaQwen3827b = "alibaba/qwen3.8-27b";

    /// <summary><c>alibaba/qwen3.8-flash</c>.</summary>
    public const string AlibabaQwen38Flash = "alibaba/qwen3.8-flash";

    /// <summary><c>alibaba/qwen3.8-max</c>.</summary>
    public const string AlibabaQwen38Max = "alibaba/qwen3.8-max";

    /// <summary><c>alibaba/qwen3.8-max-0902</c>.</summary>
    public const string AlibabaQwen38Max0902 = "alibaba/qwen3.8-max-0902";

    /// <summary><c>alibaba/qwen3.8-max-prime</c>.</summary>
    public const string AlibabaQwen38MaxPrime = "alibaba/qwen3.8-max-prime";

    /// <summary><c>alibaba/qwen3.8-omni-flash</c>.</summary>
    public const string AlibabaQwen38OmniFlash = "alibaba/qwen3.8-omni-flash";

    /// <summary><c>alibaba/qwen4-max</c>.</summary>
    public const string AlibabaQwen4Max = "alibaba/qwen4-max";

    /// <summary><c>amazon/nova-2-lite</c>.</summary>
    public const string AmazonNova2Lite = "amazon/nova-2-lite";

    /// <summary><c>amazon/nova-lite</c>.</summary>
    public const string AmazonNovaLite = "amazon/nova-lite";

    /// <summary><c>amazon/nova-micro</c>.</summary>
    public const string AmazonNovaMicro = "amazon/nova-micro";

    /// <summary><c>amazon/nova-pro</c>.</summary>
    public const string AmazonNovaPro = "amazon/nova-pro";

    /// <summary><c>anthropic/claude-fable-5</c>.</summary>
    public const string AnthropicClaudeFable5 = "anthropic/claude-fable-5";

    /// <summary><c>anthropic/claude-fable-5-batch</c>.</summary>
    public const string AnthropicClaudeFable5Batch = "anthropic/claude-fable-5-batch";

    /// <summary><c>anthropic/claude-fable-5.1</c>.</summary>
    public const string AnthropicClaudeFable51 = "anthropic/claude-fable-5.1";

    /// <summary><c>anthropic/claude-fable-5.1-batch</c>.</summary>
    public const string AnthropicClaudeFable51Batch = "anthropic/claude-fable-5.1-batch";

    /// <summary><c>anthropic/claude-fable-5.2</c>.</summary>
    public const string AnthropicClaudeFable52 = "anthropic/claude-fable-5.2";

    /// <summary><c>anthropic/claude-fable-5.2-batch</c>.</summary>
    public const string AnthropicClaudeFable52Batch = "anthropic/claude-fable-5.2-batch";

    /// <summary><c>anthropic/claude-fable-5.2-fast</c>.</summary>
    public const string AnthropicClaudeFable52Fast = "anthropic/claude-fable-5.2-fast";

    /// <summary><c>anthropic/claude-haiku-4.5</c>.</summary>
    public const string AnthropicClaudeHaiku45 = "anthropic/claude-haiku-4.5";

    /// <summary><c>anthropic/claude-haiku-4.5-batch</c>.</summary>
    public const string AnthropicClaudeHaiku45Batch = "anthropic/claude-haiku-4.5-batch";

    /// <summary><c>anthropic/claude-haiku-5.5</c>.</summary>
    public const string AnthropicClaudeHaiku55 = "anthropic/claude-haiku-5.5";

    /// <summary><c>anthropic/claude-haiku-5.5-batch</c>.</summary>
    public const string AnthropicClaudeHaiku55Batch = "anthropic/claude-haiku-5.5-batch";

    /// <summary><c>anthropic/claude-opus-4.5</c>.</summary>
    public const string AnthropicClaudeOpus45 = "anthropic/claude-opus-4.5";

    /// <summary><c>anthropic/claude-opus-4.5-batch</c>.</summary>
    public const string AnthropicClaudeOpus45Batch = "anthropic/claude-opus-4.5-batch";

    /// <summary><c>anthropic/claude-opus-4.6</c>.</summary>
    public const string AnthropicClaudeOpus46 = "anthropic/claude-opus-4.6";

    /// <summary><c>anthropic/claude-opus-4.6-batch</c>.</summary>
    public const string AnthropicClaudeOpus46Batch = "anthropic/claude-opus-4.6-batch";

    /// <summary><c>anthropic/claude-opus-4.7</c>.</summary>
    public const string AnthropicClaudeOpus47 = "anthropic/claude-opus-4.7";

    /// <summary><c>anthropic/claude-opus-4.7-batch</c>.</summary>
    public const string AnthropicClaudeOpus47Batch = "anthropic/claude-opus-4.7-batch";

    /// <summary><c>anthropic/claude-opus-4.8</c>.</summary>
    public const string AnthropicClaudeOpus48 = "anthropic/claude-opus-4.8";

    /// <summary><c>anthropic/claude-opus-4.8-batch</c>.</summary>
    public const string AnthropicClaudeOpus48Batch = "anthropic/claude-opus-4.8-batch";

    /// <summary><c>anthropic/claude-opus-4.8-fast</c>.</summary>
    public const string AnthropicClaudeOpus48Fast = "anthropic/claude-opus-4.8-fast";

    /// <summary><c>anthropic/claude-opus-5</c>.</summary>
    public const string AnthropicClaudeOpus5 = "anthropic/claude-opus-5";

    /// <summary><c>anthropic/claude-opus-5-batch</c>.</summary>
    public const string AnthropicClaudeOpus5Batch = "anthropic/claude-opus-5-batch";

    /// <summary><c>anthropic/claude-opus-5-fast</c>.</summary>
    public const string AnthropicClaudeOpus5Fast = "anthropic/claude-opus-5-fast";

    /// <summary><c>anthropic/claude-opus-5.5</c>.</summary>
    public const string AnthropicClaudeOpus55 = "anthropic/claude-opus-5.5";

    /// <summary><c>anthropic/claude-opus-5.5-batch</c>.</summary>
    public const string AnthropicClaudeOpus55Batch = "anthropic/claude-opus-5.5-batch";

    /// <summary><c>anthropic/claude-opus-5.5-fast</c>.</summary>
    public const string AnthropicClaudeOpus55Fast = "anthropic/claude-opus-5.5-fast";

    /// <summary><c>anthropic/claude-sonnet-4.5</c>.</summary>
    public const string AnthropicClaudeSonnet45 = "anthropic/claude-sonnet-4.5";

    /// <summary><c>anthropic/claude-sonnet-4.5-batch</c>.</summary>
    public const string AnthropicClaudeSonnet45Batch = "anthropic/claude-sonnet-4.5-batch";

    /// <summary><c>anthropic/claude-sonnet-4.6</c>.</summary>
    public const string AnthropicClaudeSonnet46 = "anthropic/claude-sonnet-4.6";

    /// <summary><c>anthropic/claude-sonnet-4.6-batch</c>.</summary>
    public const string AnthropicClaudeSonnet46Batch = "anthropic/claude-sonnet-4.6-batch";

    /// <summary><c>anthropic/claude-sonnet-5</c>.</summary>
    public const string AnthropicClaudeSonnet5 = "anthropic/claude-sonnet-5";

    /// <summary><c>anthropic/claude-sonnet-5-batch</c>.</summary>
    public const string AnthropicClaudeSonnet5Batch = "anthropic/claude-sonnet-5-batch";

    /// <summary><c>anthropic/claude-sonnet-5.5</c>.</summary>
    public const string AnthropicClaudeSonnet55 = "anthropic/claude-sonnet-5.5";

    /// <summary><c>anthropic/claude-sonnet-5.5-batch</c>.</summary>
    public const string AnthropicClaudeSonnet55Batch = "anthropic/claude-sonnet-5.5-batch";

    /// <summary><c>bytedance/seed-2.1-turbo</c>.</summary>
    public const string BytedanceSeed21Turbo = "bytedance/seed-2.1-turbo";

    /// <summary><c>callstack/apex</c>.</summary>
    public const string CallstackApex = "callstack/apex";

    /// <summary><c>cohere/command-a</c>.</summary>
    public const string CohereCommandA = "cohere/command-a";

    /// <summary><c>deepseek/deepseek-r1</c>.</summary>
    public const string DeepseekDeepseekR1 = "deepseek/deepseek-r1";

    /// <summary><c>deepseek/deepseek-v3.1</c>.</summary>
    public const string DeepseekDeepseekV31 = "deepseek/deepseek-v3.1";

    /// <summary><c>deepseek/deepseek-v3.2</c>.</summary>
    public const string DeepseekDeepseekV32 = "deepseek/deepseek-v3.2";

    /// <summary><c>deepseek/deepseek-v3.2-thinking</c>.</summary>
    public const string DeepseekDeepseekV32Thinking = "deepseek/deepseek-v3.2-thinking";

    /// <summary><c>deepseek/deepseek-v4-flash</c>.</summary>
    public const string DeepseekDeepseekV4Flash = "deepseek/deepseek-v4-flash";

    /// <summary><c>deepseek/deepseek-v4-flash-0731</c>.</summary>
    public const string DeepseekDeepseekV4Flash0731 = "deepseek/deepseek-v4-flash-0731";

    /// <summary><c>deepseek/deepseek-v4-flash-vision-exp</c>.</summary>
    public const string DeepseekDeepseekV4FlashVisionExp = "deepseek/deepseek-v4-flash-vision-exp";

    /// <summary><c>deepseek/deepseek-v4-pro</c>.</summary>
    public const string DeepseekDeepseekV4Pro = "deepseek/deepseek-v4-pro";

    /// <summary><c>deepseek/deepseek-v4-pro-0813</c>.</summary>
    public const string DeepseekDeepseekV4Pro0813 = "deepseek/deepseek-v4-pro-0813";

    /// <summary><c>deepseek/deepseek-v4.1-flash</c>.</summary>
    public const string DeepseekDeepseekV41Flash = "deepseek/deepseek-v4.1-flash";

    /// <summary><c>deepseek/deepseek-v4.1-flash-1017</c>.</summary>
    public const string DeepseekDeepseekV41Flash1017 = "deepseek/deepseek-v4.1-flash-1017";

    /// <summary><c>deepseek/deepseek-v4.1-flash-1017-fast</c>.</summary>
    public const string DeepseekDeepseekV41Flash1017Fast = "deepseek/deepseek-v4.1-flash-1017-fast";

    /// <summary><c>deepseek/deepseek-v4.1-flash-1017-ultrafast</c>.</summary>
    public const string DeepseekDeepseekV41Flash1017Ultrafast =
        "deepseek/deepseek-v4.1-flash-1017-ultrafast";

    /// <summary><c>deepseek/deepseek-v4.1-flash-fast</c>.</summary>
    public const string DeepseekDeepseekV41FlashFast = "deepseek/deepseek-v4.1-flash-fast";

    /// <summary><c>elorian/glimpse</c>.</summary>
    public const string ElorianGlimpse = "elorian/glimpse";

    /// <summary><c>fireworks/ember-1</c>.</summary>
    public const string FireworksEmber1 = "fireworks/ember-1";

    /// <summary><c>google/gemini-2.5-flash</c>.</summary>
    public const string GoogleGemini25Flash = "google/gemini-2.5-flash";

    /// <summary><c>google/gemini-2.5-flash-batch</c>.</summary>
    public const string GoogleGemini25FlashBatch = "google/gemini-2.5-flash-batch";

    /// <summary><c>google/gemini-2.5-flash-image</c>.</summary>
    public const string GoogleGemini25FlashImage = "google/gemini-2.5-flash-image";

    /// <summary><c>google/gemini-2.5-flash-lite</c>.</summary>
    public const string GoogleGemini25FlashLite = "google/gemini-2.5-flash-lite";

    /// <summary><c>google/gemini-2.5-flash-lite-batch</c>.</summary>
    public const string GoogleGemini25FlashLiteBatch = "google/gemini-2.5-flash-lite-batch";

    /// <summary><c>google/gemini-2.5-pro</c>.</summary>
    public const string GoogleGemini25Pro = "google/gemini-2.5-pro";

    /// <summary><c>google/gemini-2.5-pro-batch</c>.</summary>
    public const string GoogleGemini25ProBatch = "google/gemini-2.5-pro-batch";

    /// <summary><c>google/gemini-3-flash</c>.</summary>
    public const string GoogleGemini3Flash = "google/gemini-3-flash";

    /// <summary><c>google/gemini-3-flash-batch</c>.</summary>
    public const string GoogleGemini3FlashBatch = "google/gemini-3-flash-batch";

    /// <summary><c>google/gemini-3-pro-image</c>.</summary>
    public const string GoogleGemini3ProImage = "google/gemini-3-pro-image";

    /// <summary><c>google/gemini-3.1-flash-image</c>.</summary>
    public const string GoogleGemini31FlashImage = "google/gemini-3.1-flash-image";

    /// <summary><c>google/gemini-3.1-flash-lite</c>.</summary>
    public const string GoogleGemini31FlashLite = "google/gemini-3.1-flash-lite";

    /// <summary><c>google/gemini-3.1-flash-lite-batch</c>.</summary>
    public const string GoogleGemini31FlashLiteBatch = "google/gemini-3.1-flash-lite-batch";

    /// <summary><c>google/gemini-3.1-flash-lite-image</c>.</summary>
    public const string GoogleGemini31FlashLiteImage = "google/gemini-3.1-flash-lite-image";

    /// <summary><c>google/gemini-3.1-pro-preview</c>.</summary>
    public const string GoogleGemini31ProPreview = "google/gemini-3.1-pro-preview";

    /// <summary><c>google/gemini-3.1-pro-preview-batch</c>.</summary>
    public const string GoogleGemini31ProPreviewBatch = "google/gemini-3.1-pro-preview-batch";

    /// <summary><c>google/gemini-3.5-flash</c>.</summary>
    public const string GoogleGemini35Flash = "google/gemini-3.5-flash";

    /// <summary><c>google/gemini-3.5-flash-lite</c>.</summary>
    public const string GoogleGemini35FlashLite = "google/gemini-3.5-flash-lite";

    /// <summary><c>google/gemini-3.5-flash-lite-batch</c>.</summary>
    public const string GoogleGemini35FlashLiteBatch = "google/gemini-3.5-flash-lite-batch";

    /// <summary><c>google/gemini-3.6-flash</c>.</summary>
    public const string GoogleGemini36Flash = "google/gemini-3.6-flash";

    /// <summary><c>google/gemini-3.6-flash-batch</c>.</summary>
    public const string GoogleGemini36FlashBatch = "google/gemini-3.6-flash-batch";

    /// <summary><c>google/gemini-3.7-flash</c>.</summary>
    public const string GoogleGemini37Flash = "google/gemini-3.7-flash";

    /// <summary><c>google/gemini-3.8-flash</c>.</summary>
    public const string GoogleGemini38Flash = "google/gemini-3.8-flash";

    /// <summary><c>google/gemini-4-argon</c>.</summary>
    public const string GoogleGemini4Argon = "google/gemini-4-argon";

    /// <summary><c>google/gemini-4-carbon</c>.</summary>
    public const string GoogleGemini4Carbon = "google/gemini-4-carbon";

    /// <summary><c>google/gemini-4-hydrogen</c>.</summary>
    public const string GoogleGemini4Hydrogen = "google/gemini-4-hydrogen";

    /// <summary><c>google/gemini-4-neon</c>.</summary>
    public const string GoogleGemini4Neon = "google/gemini-4-neon";

    /// <summary><c>google/gemini-nano-banana-2.1</c>.</summary>
    public const string GoogleGeminiNanoBanana21 = "google/gemini-nano-banana-2.1";

    /// <summary><c>google/gemini-omni-1.2-flash</c>.</summary>
    public const string GoogleGeminiOmni12Flash = "google/gemini-omni-1.2-flash";

    /// <summary><c>google/gemini-omni-flash-preview</c>.</summary>
    public const string GoogleGeminiOmniFlashPreview = "google/gemini-omni-flash-preview";

    /// <summary><c>google/gemma-4-26b-a4b-it</c>.</summary>
    public const string GoogleGemma426bA4bIt = "google/gemma-4-26b-a4b-it";

    /// <summary><c>google/gemma-4-31b-it</c>.</summary>
    public const string GoogleGemma431bIt = "google/gemma-4-31b-it";

    /// <summary><c>humansand/ampersand-v0.1</c>.</summary>
    public const string HumansandAmpersandV01 = "humansand/ampersand-v0.1";

    /// <summary><c>inception/mercury-3</c>.</summary>
    public const string InceptionMercury3 = "inception/mercury-3";

    /// <summary><c>inclusionai/ling-3.0-flash</c>.</summary>
    public const string InclusionaiLing30Flash = "inclusionai/ling-3.0-flash";

    /// <summary><c>inclusionai/ling-3.0-flash-fin</c>.</summary>
    public const string InclusionaiLing30FlashFin = "inclusionai/ling-3.0-flash-fin";

    /// <summary><c>inclusionai/ling-3.0-flash-sante</c>.</summary>
    public const string InclusionaiLing30FlashSante = "inclusionai/ling-3.0-flash-sante";

    /// <summary><c>inclusionai/ling-3.0-flash-vl</c>.</summary>
    public const string InclusionaiLing30FlashVl = "inclusionai/ling-3.0-flash-vl";

    /// <summary><c>inclusionai/ling-3.1-flash</c>.</summary>
    public const string InclusionaiLing31Flash = "inclusionai/ling-3.1-flash";

    /// <summary><c>inclusionai/ling-3.1-flash-1019</c>.</summary>
    public const string InclusionaiLing31Flash1019 = "inclusionai/ling-3.1-flash-1019";

    /// <summary><c>inference-net/schematron-v2-small</c>.</summary>
    public const string InferenceNetSchematronV2Small = "inference-net/schematron-v2-small";

    /// <summary><c>inference-net/schematron-v2-turbo</c>.</summary>
    public const string InferenceNetSchematronV2Turbo = "inference-net/schematron-v2-turbo";

    /// <summary><c>interfaze/interfaze-beta</c>.</summary>
    public const string InterfazeInterfazeBeta = "interfaze/interfaze-beta";

    /// <summary><c>liquid/lfm2.5-24b-a2b</c>.</summary>
    public const string LiquidLfm2524bA2b = "liquid/lfm2.5-24b-a2b";

    /// <summary><c>meituan/longcat-2.5</c>.</summary>
    public const string MeituanLongcat25 = "meituan/longcat-2.5";

    /// <summary><c>meta/llama-3.1-70b</c>.</summary>
    public const string MetaLlama3170b = "meta/llama-3.1-70b";

    /// <summary><c>meta/llama-3.1-8b</c>.</summary>
    public const string MetaLlama318b = "meta/llama-3.1-8b";

    /// <summary><c>meta/llama-3.3-70b</c>.</summary>
    public const string MetaLlama3370b = "meta/llama-3.3-70b";

    /// <summary><c>meta/llama-4-maverick</c>.</summary>
    public const string MetaLlama4Maverick = "meta/llama-4-maverick";

    /// <summary><c>meta/llama-4-scout</c>.</summary>
    public const string MetaLlama4Scout = "meta/llama-4-scout";

    /// <summary><c>meta/muse-ember</c>.</summary>
    public const string MetaMuseEmber = "meta/muse-ember";

    /// <summary><c>meta/muse-glimmer-30b</c>.</summary>
    public const string MetaMuseGlimmer30b = "meta/muse-glimmer-30b";

    /// <summary><c>meta/muse-spark-1.1</c>.</summary>
    public const string MetaMuseSpark11 = "meta/muse-spark-1.1";

    /// <summary><c>meta/muse-spark-1.2</c>.</summary>
    public const string MetaMuseSpark12 = "meta/muse-spark-1.2";

    /// <summary><c>meta/muse-spark-1.2-contributor</c>.</summary>
    public const string MetaMuseSpark12Contributor = "meta/muse-spark-1.2-contributor";

    /// <summary><c>meta/muse-spark-1.3</c>.</summary>
    public const string MetaMuseSpark13 = "meta/muse-spark-1.3";

    /// <summary><c>meta/muse-spark-1.3-contributor</c>.</summary>
    public const string MetaMuseSpark13Contributor = "meta/muse-spark-1.3-contributor";

    /// <summary><c>meta/muse-spark-open</c>.</summary>
    public const string MetaMuseSparkOpen = "meta/muse-spark-open";

    /// <summary><c>meta/muse-wonder-1.0</c>.</summary>
    public const string MetaMuseWonder10 = "meta/muse-wonder-1.0";

    /// <summary><c>minimax/minimax-m2.1</c>.</summary>
    public const string MinimaxMinimaxM21 = "minimax/minimax-m2.1";

    /// <summary><c>minimax/minimax-m2.5</c>.</summary>
    public const string MinimaxMinimaxM25 = "minimax/minimax-m2.5";

    /// <summary><c>minimax/minimax-m2.7</c>.</summary>
    public const string MinimaxMinimaxM27 = "minimax/minimax-m2.7";

    /// <summary><c>minimax/minimax-m3</c>.</summary>
    public const string MinimaxMinimaxM3 = "minimax/minimax-m3";

    /// <summary><c>minimax/minimax-m3.1-flash</c>.</summary>
    public const string MinimaxMinimaxM31Flash = "minimax/minimax-m3.1-flash";

    /// <summary><c>mistral/codestral</c>.</summary>
    public const string MistralCodestral = "mistral/codestral";

    /// <summary><c>mistral/ministral-14b</c>.</summary>
    public const string MistralMinistral14b = "mistral/ministral-14b";

    /// <summary><c>mistral/ministral-3b</c>.</summary>
    public const string MistralMinistral3b = "mistral/ministral-3b";

    /// <summary><c>mistral/ministral-8b</c>.</summary>
    public const string MistralMinistral8b = "mistral/ministral-8b";

    /// <summary><c>mistral/mistral-large-3</c>.</summary>
    public const string MistralMistralLarge3 = "mistral/mistral-large-3";

    /// <summary><c>mistral/mistral-large-4</c>.</summary>
    public const string MistralMistralLarge4 = "mistral/mistral-large-4";

    /// <summary><c>mistral/mistral-medium-3.5</c>.</summary>
    public const string MistralMistralMedium35 = "mistral/mistral-medium-3.5";

    /// <summary><c>mistral/mistral-nemo</c>.</summary>
    public const string MistralMistralNemo = "mistral/mistral-nemo";

    /// <summary><c>mistral/mistral-small</c>.</summary>
    public const string MistralMistralSmall = "mistral/mistral-small";

    /// <summary><c>mixedbread/toast-1</c>.</summary>
    public const string MixedbreadToast1 = "mixedbread/toast-1";

    /// <summary><c>moonshotai/kimi-k2</c>.</summary>
    public const string MoonshotaiKimiK2 = "moonshotai/kimi-k2";

    /// <summary><c>moonshotai/kimi-k2.5</c>.</summary>
    public const string MoonshotaiKimiK25 = "moonshotai/kimi-k2.5";

    /// <summary><c>moonshotai/kimi-k2.6</c>.</summary>
    public const string MoonshotaiKimiK26 = "moonshotai/kimi-k2.6";

    /// <summary><c>moonshotai/kimi-k2.7-code</c>.</summary>
    public const string MoonshotaiKimiK27Code = "moonshotai/kimi-k2.7-code";

    /// <summary><c>moonshotai/kimi-k2.7-code-highspeed</c>.</summary>
    public const string MoonshotaiKimiK27CodeHighspeed = "moonshotai/kimi-k2.7-code-highspeed";

    /// <summary><c>moonshotai/kimi-k3</c>.</summary>
    public const string MoonshotaiKimiK3 = "moonshotai/kimi-k3";

    /// <summary><c>moonshotai/kimi-k3-fast</c>.</summary>
    public const string MoonshotaiKimiK3Fast = "moonshotai/kimi-k3-fast";

    /// <summary><c>moonshotai/kimi-k3.1-pro</c>.</summary>
    public const string MoonshotaiKimiK31Pro = "moonshotai/kimi-k3.1-pro";

    /// <summary><c>moonshotai/kimi-k3.1-pro-highspeed</c>.</summary>
    public const string MoonshotaiKimiK31ProHighspeed = "moonshotai/kimi-k3.1-pro-highspeed";

    /// <summary><c>morph/morph-v3-fast</c>.</summary>
    public const string MorphMorphV3Fast = "morph/morph-v3-fast";

    /// <summary><c>morph/morph-v3-large</c>.</summary>
    public const string MorphMorphV3Large = "morph/morph-v3-large";

    /// <summary><c>nvidia/nemotron-3-super-120b-a12b</c>.</summary>
    public const string NvidiaNemotron3Super120bA12b = "nvidia/nemotron-3-super-120b-a12b";

    /// <summary><c>nvidia/nemotron-3-ultra-550b-a55b</c>.</summary>
    public const string NvidiaNemotron3Ultra550bA55b = "nvidia/nemotron-3-ultra-550b-a55b";

    /// <summary><c>nvidia/nemotron-3.5-lightning</c>.</summary>
    public const string NvidiaNemotron35Lightning = "nvidia/nemotron-3.5-lightning";

    /// <summary><c>nvidia/nemotron-3.5-lightning-free</c>.</summary>
    public const string NvidiaNemotron35LightningFree = "nvidia/nemotron-3.5-lightning-free";

    /// <summary><c>nvidia/nemotron-3.5-super-vl</c>.</summary>
    public const string NvidiaNemotron35SuperVl = "nvidia/nemotron-3.5-super-vl";

    /// <summary><c>nvidia/nemotron-nano-12b-v2-vl</c>.</summary>
    public const string NvidiaNemotronNano12bV2Vl = "nvidia/nemotron-nano-12b-v2-vl";

    /// <summary><c>nvidia/nemotron-nano-9b-v2</c>.</summary>
    public const string NvidiaNemotronNano9bV2 = "nvidia/nemotron-nano-9b-v2";

    /// <summary><c>openai/gpt-4.1</c>.</summary>
    public const string OpenaiGpt41 = "openai/gpt-4.1";

    /// <summary><c>openai/gpt-4.1-batch</c>.</summary>
    public const string OpenaiGpt41Batch = "openai/gpt-4.1-batch";

    /// <summary><c>openai/gpt-4.1-fast</c>.</summary>
    public const string OpenaiGpt41Fast = "openai/gpt-4.1-fast";

    /// <summary><c>openai/gpt-4.1-mini</c>.</summary>
    public const string OpenaiGpt41Mini = "openai/gpt-4.1-mini";

    /// <summary><c>openai/gpt-4.1-mini-batch</c>.</summary>
    public const string OpenaiGpt41MiniBatch = "openai/gpt-4.1-mini-batch";

    /// <summary><c>openai/gpt-4.1-mini-fast</c>.</summary>
    public const string OpenaiGpt41MiniFast = "openai/gpt-4.1-mini-fast";

    /// <summary><c>openai/gpt-4.1-nano</c>.</summary>
    public const string OpenaiGpt41Nano = "openai/gpt-4.1-nano";

    /// <summary><c>openai/gpt-4o</c>.</summary>
    public const string OpenaiGpt4o = "openai/gpt-4o";

    /// <summary><c>openai/gpt-4o-batch</c>.</summary>
    public const string OpenaiGpt4oBatch = "openai/gpt-4o-batch";

    /// <summary><c>openai/gpt-4o-fast</c>.</summary>
    public const string OpenaiGpt4oFast = "openai/gpt-4o-fast";

    /// <summary><c>openai/gpt-4o-mini</c>.</summary>
    public const string OpenaiGpt4oMini = "openai/gpt-4o-mini";

    /// <summary><c>openai/gpt-4o-mini-batch</c>.</summary>
    public const string OpenaiGpt4oMiniBatch = "openai/gpt-4o-mini-batch";

    /// <summary><c>openai/gpt-4o-mini-fast</c>.</summary>
    public const string OpenaiGpt4oMiniFast = "openai/gpt-4o-mini-fast";

    /// <summary><c>openai/gpt-5</c>.</summary>
    public const string OpenaiGpt5 = "openai/gpt-5";

    /// <summary><c>openai/gpt-5-batch</c>.</summary>
    public const string OpenaiGpt5Batch = "openai/gpt-5-batch";

    /// <summary><c>openai/gpt-5-codex</c>.</summary>
    public const string OpenaiGpt5Codex = "openai/gpt-5-codex";

    /// <summary><c>openai/gpt-5-fast</c>.</summary>
    public const string OpenaiGpt5Fast = "openai/gpt-5-fast";

    /// <summary><c>openai/gpt-5-mini</c>.</summary>
    public const string OpenaiGpt5Mini = "openai/gpt-5-mini";

    /// <summary><c>openai/gpt-5-mini-batch</c>.</summary>
    public const string OpenaiGpt5MiniBatch = "openai/gpt-5-mini-batch";

    /// <summary><c>openai/gpt-5-mini-fast</c>.</summary>
    public const string OpenaiGpt5MiniFast = "openai/gpt-5-mini-fast";

    /// <summary><c>openai/gpt-5-nano</c>.</summary>
    public const string OpenaiGpt5Nano = "openai/gpt-5-nano";

    /// <summary><c>openai/gpt-5-nano-batch</c>.</summary>
    public const string OpenaiGpt5NanoBatch = "openai/gpt-5-nano-batch";

    /// <summary><c>openai/gpt-5-pro</c>.</summary>
    public const string OpenaiGpt5Pro = "openai/gpt-5-pro";

    /// <summary><c>openai/gpt-5-pro-batch</c>.</summary>
    public const string OpenaiGpt5ProBatch = "openai/gpt-5-pro-batch";

    /// <summary><c>openai/gpt-5.1-codex</c>.</summary>
    public const string OpenaiGpt51Codex = "openai/gpt-5.1-codex";

    /// <summary><c>openai/gpt-5.1-codex-max</c>.</summary>
    public const string OpenaiGpt51CodexMax = "openai/gpt-5.1-codex-max";

    /// <summary><c>openai/gpt-5.1-codex-mini</c>.</summary>
    public const string OpenaiGpt51CodexMini = "openai/gpt-5.1-codex-mini";

    /// <summary><c>openai/gpt-5.1-thinking</c>.</summary>
    public const string OpenaiGpt51Thinking = "openai/gpt-5.1-thinking";

    /// <summary><c>openai/gpt-5.1-thinking-fast</c>.</summary>
    public const string OpenaiGpt51ThinkingFast = "openai/gpt-5.1-thinking-fast";

    /// <summary><c>openai/gpt-5.2</c>.</summary>
    public const string OpenaiGpt52 = "openai/gpt-5.2";

    /// <summary><c>openai/gpt-5.2-batch</c>.</summary>
    public const string OpenaiGpt52Batch = "openai/gpt-5.2-batch";

    /// <summary><c>openai/gpt-5.2-codex</c>.</summary>
    public const string OpenaiGpt52Codex = "openai/gpt-5.2-codex";

    /// <summary><c>openai/gpt-5.2-fast</c>.</summary>
    public const string OpenaiGpt52Fast = "openai/gpt-5.2-fast";

    /// <summary><c>openai/gpt-5.2-pro</c>.</summary>
    public const string OpenaiGpt52Pro = "openai/gpt-5.2-pro";

    /// <summary><c>openai/gpt-5.3-codex</c>.</summary>
    public const string OpenaiGpt53Codex = "openai/gpt-5.3-codex";

    /// <summary><c>openai/gpt-5.3-codex-fast</c>.</summary>
    public const string OpenaiGpt53CodexFast = "openai/gpt-5.3-codex-fast";

    /// <summary><c>openai/gpt-5.4</c>.</summary>
    public const string OpenaiGpt54 = "openai/gpt-5.4";

    /// <summary><c>openai/gpt-5.4-batch</c>.</summary>
    public const string OpenaiGpt54Batch = "openai/gpt-5.4-batch";

    /// <summary><c>openai/gpt-5.4-fast</c>.</summary>
    public const string OpenaiGpt54Fast = "openai/gpt-5.4-fast";

    /// <summary><c>openai/gpt-5.4-mini</c>.</summary>
    public const string OpenaiGpt54Mini = "openai/gpt-5.4-mini";

    /// <summary><c>openai/gpt-5.4-mini-batch</c>.</summary>
    public const string OpenaiGpt54MiniBatch = "openai/gpt-5.4-mini-batch";

    /// <summary><c>openai/gpt-5.4-mini-fast</c>.</summary>
    public const string OpenaiGpt54MiniFast = "openai/gpt-5.4-mini-fast";

    /// <summary><c>openai/gpt-5.4-nano</c>.</summary>
    public const string OpenaiGpt54Nano = "openai/gpt-5.4-nano";

    /// <summary><c>openai/gpt-5.4-nano-batch</c>.</summary>
    public const string OpenaiGpt54NanoBatch = "openai/gpt-5.4-nano-batch";

    /// <summary><c>openai/gpt-5.4-pro</c>.</summary>
    public const string OpenaiGpt54Pro = "openai/gpt-5.4-pro";

    /// <summary><c>openai/gpt-5.5</c>.</summary>
    public const string OpenaiGpt55 = "openai/gpt-5.5";

    /// <summary><c>openai/gpt-5.5-batch</c>.</summary>
    public const string OpenaiGpt55Batch = "openai/gpt-5.5-batch";

    /// <summary><c>openai/gpt-5.5-fast</c>.</summary>
    public const string OpenaiGpt55Fast = "openai/gpt-5.5-fast";

    /// <summary><c>openai/gpt-5.5-pro</c>.</summary>
    public const string OpenaiGpt55Pro = "openai/gpt-5.5-pro";

    /// <summary><c>openai/gpt-5.5-pro-batch</c>.</summary>
    public const string OpenaiGpt55ProBatch = "openai/gpt-5.5-pro-batch";

    /// <summary><c>openai/gpt-5.6-luna</c>.</summary>
    public const string OpenaiGpt56Luna = "openai/gpt-5.6-luna";

    /// <summary><c>openai/gpt-5.6-luna-batch</c>.</summary>
    public const string OpenaiGpt56LunaBatch = "openai/gpt-5.6-luna-batch";

    /// <summary><c>openai/gpt-5.6-luna-fast</c>.</summary>
    public const string OpenaiGpt56LunaFast = "openai/gpt-5.6-luna-fast";

    /// <summary><c>openai/gpt-5.6-sol</c>.</summary>
    public const string OpenaiGpt56Sol = "openai/gpt-5.6-sol";

    /// <summary><c>openai/gpt-5.6-sol-batch</c>.</summary>
    public const string OpenaiGpt56SolBatch = "openai/gpt-5.6-sol-batch";

    /// <summary><c>openai/gpt-5.6-sol-fast</c>.</summary>
    public const string OpenaiGpt56SolFast = "openai/gpt-5.6-sol-fast";

    /// <summary><c>openai/gpt-5.6-terra</c>.</summary>
    public const string OpenaiGpt56Terra = "openai/gpt-5.6-terra";

    /// <summary><c>openai/gpt-5.6-terra-batch</c>.</summary>
    public const string OpenaiGpt56TerraBatch = "openai/gpt-5.6-terra-batch";

    /// <summary><c>openai/gpt-5.6-terra-fast</c>.</summary>
    public const string OpenaiGpt56TerraFast = "openai/gpt-5.6-terra-fast";

    /// <summary><c>openai/gpt-6-astra</c>.</summary>
    public const string OpenaiGpt6Astra = "openai/gpt-6-astra";

    /// <summary><c>openai/gpt-6-astra-batch</c>.</summary>
    public const string OpenaiGpt6AstraBatch = "openai/gpt-6-astra-batch";

    /// <summary><c>openai/gpt-6-astra-fast</c>.</summary>
    public const string OpenaiGpt6AstraFast = "openai/gpt-6-astra-fast";

    /// <summary><c>openai/gpt-6-luna</c>.</summary>
    public const string OpenaiGpt6Luna = "openai/gpt-6-luna";

    /// <summary><c>openai/gpt-6-luna-batch</c>.</summary>
    public const string OpenaiGpt6LunaBatch = "openai/gpt-6-luna-batch";

    /// <summary><c>openai/gpt-6-luna-fast</c>.</summary>
    public const string OpenaiGpt6LunaFast = "openai/gpt-6-luna-fast";

    /// <summary><c>openai/gpt-6-sol</c>.</summary>
    public const string OpenaiGpt6Sol = "openai/gpt-6-sol";

    /// <summary><c>openai/gpt-6-sol-batch</c>.</summary>
    public const string OpenaiGpt6SolBatch = "openai/gpt-6-sol-batch";

    /// <summary><c>openai/gpt-6-sol-fast</c>.</summary>
    public const string OpenaiGpt6SolFast = "openai/gpt-6-sol-fast";

    /// <summary><c>openai/gpt-6.1-sol</c>.</summary>
    public const string OpenaiGpt61Sol = "openai/gpt-6.1-sol";

    /// <summary><c>openai/gpt-6.1-sol-batch</c>.</summary>
    public const string OpenaiGpt61SolBatch = "openai/gpt-6.1-sol-batch";

    /// <summary><c>openai/gpt-6.1-sol-fast</c>.</summary>
    public const string OpenaiGpt61SolFast = "openai/gpt-6.1-sol-fast";

    /// <summary><c>openai/gpt-6.1-terra</c>.</summary>
    public const string OpenaiGpt61Terra = "openai/gpt-6.1-terra";

    /// <summary><c>openai/gpt-6.1-terra-fast</c>.</summary>
    public const string OpenaiGpt61TerraFast = "openai/gpt-6.1-terra-fast";

    /// <summary><c>openai/gpt-6.2-astra</c>.</summary>
    public const string OpenaiGpt62Astra = "openai/gpt-6.2-astra";

    /// <summary><c>openai/gpt-6.2-astra-fast</c>.</summary>
    public const string OpenaiGpt62AstraFast = "openai/gpt-6.2-astra-fast";

    /// <summary><c>openai/gpt-6.2-sol</c>.</summary>
    public const string OpenaiGpt62Sol = "openai/gpt-6.2-sol";

    /// <summary><c>openai/gpt-6.2-sol-fast</c>.</summary>
    public const string OpenaiGpt62SolFast = "openai/gpt-6.2-sol-fast";

    /// <summary><c>openai/gpt-oss-120b</c>.</summary>
    public const string OpenaiGptOss120b = "openai/gpt-oss-120b";

    /// <summary><c>openai/gpt-oss-20b</c>.</summary>
    public const string OpenaiGptOss20b = "openai/gpt-oss-20b";

    /// <summary><c>openai/gpt-oss-safeguard-120b</c>.</summary>
    public const string OpenaiGptOssSafeguard120b = "openai/gpt-oss-safeguard-120b";

    /// <summary><c>openai/gpt-oss-safeguard-20b</c>.</summary>
    public const string OpenaiGptOssSafeguard20b = "openai/gpt-oss-safeguard-20b";

    /// <summary><c>openai/o1</c>.</summary>
    public const string OpenaiO1 = "openai/o1";

    /// <summary><c>openai/o3</c>.</summary>
    public const string OpenaiO3 = "openai/o3";

    /// <summary><c>openai/o3-fast</c>.</summary>
    public const string OpenaiO3Fast = "openai/o3-fast";

    /// <summary><c>openai/o3-mini</c>.</summary>
    public const string OpenaiO3Mini = "openai/o3-mini";

    /// <summary><c>openai/o3-pro</c>.</summary>
    public const string OpenaiO3Pro = "openai/o3-pro";

    /// <summary><c>openai/o4-mini</c>.</summary>
    public const string OpenaiO4Mini = "openai/o4-mini";

    /// <summary><c>paperinstruments/ultramarine-1-flash</c>.</summary>
    public const string PaperinstrumentsUltramarine1Flash = "paperinstruments/ultramarine-1-flash";

    /// <summary><c>particle/particle-1</c>.</summary>
    public const string ParticleParticle1 = "particle/particle-1";

    /// <summary><c>perplexity/sonar</c>.</summary>
    public const string PerplexitySonar = "perplexity/sonar";

    /// <summary><c>quiverai/arrow-2</c>.</summary>
    public const string QuiveraiArrow2 = "quiverai/arrow-2";

    /// <summary><c>quiverai/arrow-2-telos</c>.</summary>
    public const string QuiveraiArrow2Telos = "quiverai/arrow-2-telos";

    /// <summary><c>recursive/flint-v1.0</c>.</summary>
    public const string RecursiveFlintV10 = "recursive/flint-v1.0";

    /// <summary><c>reflection/beam</c>.</summary>
    public const string ReflectionBeam = "reflection/beam";

    /// <summary><c>reflection/beam-free</c>.</summary>
    public const string ReflectionBeamFree = "reflection/beam-free";

    /// <summary><c>sakana/fugu-max</c>.</summary>
    public const string SakanaFuguMax = "sakana/fugu-max";

    /// <summary><c>sakana/fugu-ultra</c>.</summary>
    public const string SakanaFuguUltra = "sakana/fugu-ultra";

    /// <summary><c>sakana/fugu-ultra-v2</c>.</summary>
    public const string SakanaFuguUltraV2 = "sakana/fugu-ultra-v2";

    /// <summary><c>sakana/namazu</c>.</summary>
    public const string SakanaNamazu = "sakana/namazu";

    /// <summary><c>scaledown/scaledown-extract-1</c>.</summary>
    public const string ScaledownScaledownExtract1 = "scaledown/scaledown-extract-1";

    /// <summary><c>scaledown/scaledown-summarize-1</c>.</summary>
    public const string ScaledownScaledownSummarize1 = "scaledown/scaledown-summarize-1";

    /// <summary><c>spacexai/grok-4.20-multi-agent</c>.</summary>
    public const string SpacexaiGrok420MultiAgent = "spacexai/grok-4.20-multi-agent";

    /// <summary><c>spacexai/grok-4.20-multi-agent-batch</c>.</summary>
    public const string SpacexaiGrok420MultiAgentBatch = "spacexai/grok-4.20-multi-agent-batch";

    /// <summary><c>spacexai/grok-4.20-multi-agent-beta</c>.</summary>
    public const string SpacexaiGrok420MultiAgentBeta = "spacexai/grok-4.20-multi-agent-beta";

    /// <summary><c>spacexai/grok-4.20-non-reasoning</c>.</summary>
    public const string SpacexaiGrok420NonReasoning = "spacexai/grok-4.20-non-reasoning";

    /// <summary><c>spacexai/grok-4.20-non-reasoning-batch</c>.</summary>
    public const string SpacexaiGrok420NonReasoningBatch = "spacexai/grok-4.20-non-reasoning-batch";

    /// <summary><c>spacexai/grok-4.20-non-reasoning-beta</c>.</summary>
    public const string SpacexaiGrok420NonReasoningBeta = "spacexai/grok-4.20-non-reasoning-beta";

    /// <summary><c>spacexai/grok-4.20-reasoning</c>.</summary>
    public const string SpacexaiGrok420Reasoning = "spacexai/grok-4.20-reasoning";

    /// <summary><c>spacexai/grok-4.20-reasoning-batch</c>.</summary>
    public const string SpacexaiGrok420ReasoningBatch = "spacexai/grok-4.20-reasoning-batch";

    /// <summary><c>spacexai/grok-4.20-reasoning-beta</c>.</summary>
    public const string SpacexaiGrok420ReasoningBeta = "spacexai/grok-4.20-reasoning-beta";

    /// <summary><c>spacexai/grok-4.3</c>.</summary>
    public const string SpacexaiGrok43 = "spacexai/grok-4.3";

    /// <summary><c>spacexai/grok-4.3-batch</c>.</summary>
    public const string SpacexaiGrok43Batch = "spacexai/grok-4.3-batch";

    /// <summary><c>spacexai/grok-4.5</c>.</summary>
    public const string SpacexaiGrok45 = "spacexai/grok-4.5";

    /// <summary><c>spacexai/grok-4.6</c>.</summary>
    public const string SpacexaiGrok46 = "spacexai/grok-4.6";

    /// <summary><c>spacexai/grok-4.7</c>.</summary>
    public const string SpacexaiGrok47 = "spacexai/grok-4.7";

    /// <summary><c>spacexai/grok-build-0.1</c>.</summary>
    public const string SpacexaiGrokBuild01 = "spacexai/grok-build-0.1";

    /// <summary><c>stealth/glyph-cluster</c>.</summary>
    public const string StealthGlyphCluster = "stealth/glyph-cluster";

    /// <summary><c>stepfun/step-3.7-flash</c>.</summary>
    public const string StepfunStep37Flash = "stepfun/step-3.7-flash";

    /// <summary><c>stepfun/step-5-preview</c>.</summary>
    public const string StepfunStep5Preview = "stepfun/step-5-preview";

    /// <summary><c>tencent/hy-mt2-lite</c>.</summary>
    public const string TencentHyMt2Lite = "tencent/hy-mt2-lite";

    /// <summary><c>tencent/hy-mt2-plus</c>.</summary>
    public const string TencentHyMt2Plus = "tencent/hy-mt2-plus";

    /// <summary><c>tencent/hy-mt2-pro</c>.</summary>
    public const string TencentHyMt2Pro = "tencent/hy-mt2-pro";

    /// <summary><c>tencent/hy3</c>.</summary>
    public const string TencentHy3 = "tencent/hy3";

    /// <summary><c>tencent/hy4</c>.</summary>
    public const string TencentHy4 = "tencent/hy4";

    /// <summary><c>tencent/hy4-flash</c>.</summary>
    public const string TencentHy4Flash = "tencent/hy4-flash";

    /// <summary><c>tencent/hy4-preview</c>.</summary>
    public const string TencentHy4Preview = "tencent/hy4-preview";

    /// <summary><c>thinkingmachines/inkling</c>.</summary>
    public const string ThinkingmachinesInkling = "thinkingmachines/inkling";

    /// <summary><c>thinkingmachines/inkling-2-small</c>.</summary>
    public const string ThinkingmachinesInkling2Small = "thinkingmachines/inkling-2-small";

    /// <summary><c>thinkingmachines/inkling-small</c>.</summary>
    public const string ThinkingmachinesInklingSmall = "thinkingmachines/inkling-small";

    /// <summary><c>xiaomi/mimo-v2.6-flash</c>.</summary>
    public const string XiaomiMimoV26Flash = "xiaomi/mimo-v2.6-flash";

    /// <summary><c>xiaomi/mimo-v2.6-pro</c>.</summary>
    public const string XiaomiMimoV26Pro = "xiaomi/mimo-v2.6-pro";

    /// <summary><c>xiaomi/mimo-v2.6-pro-ultraspeed</c>.</summary>
    public const string XiaomiMimoV26ProUltraspeed = "xiaomi/mimo-v2.6-pro-ultraspeed";

    /// <summary><c>zai/glm-4.5</c>.</summary>
    public const string ZaiGlm45 = "zai/glm-4.5";

    /// <summary><c>zai/glm-4.5-air</c>.</summary>
    public const string ZaiGlm45Air = "zai/glm-4.5-air";

    /// <summary><c>zai/glm-4.5v</c>.</summary>
    public const string ZaiGlm45v = "zai/glm-4.5v";

    /// <summary><c>zai/glm-4.6</c>.</summary>
    public const string ZaiGlm46 = "zai/glm-4.6";

    /// <summary><c>zai/glm-4.7</c>.</summary>
    public const string ZaiGlm47 = "zai/glm-4.7";

    /// <summary><c>zai/glm-4.7-flash</c>.</summary>
    public const string ZaiGlm47Flash = "zai/glm-4.7-flash";

    /// <summary><c>zai/glm-4.7-flashx</c>.</summary>
    public const string ZaiGlm47Flashx = "zai/glm-4.7-flashx";

    /// <summary><c>zai/glm-5</c>.</summary>
    public const string ZaiGlm5 = "zai/glm-5";

    /// <summary><c>zai/glm-5-turbo</c>.</summary>
    public const string ZaiGlm5Turbo = "zai/glm-5-turbo";

    /// <summary><c>zai/glm-5.1</c>.</summary>
    public const string ZaiGlm51 = "zai/glm-5.1";

    /// <summary><c>zai/glm-5.2</c>.</summary>
    public const string ZaiGlm52 = "zai/glm-5.2";

    /// <summary><c>zai/glm-5.2-fast</c>.</summary>
    public const string ZaiGlm52Fast = "zai/glm-5.2-fast";

    /// <summary><c>zai/glm-5.3</c>.</summary>
    public const string ZaiGlm53 = "zai/glm-5.3";

    /// <summary><c>zai/glm-5.3-fast</c>.</summary>
    public const string ZaiGlm53Fast = "zai/glm-5.3-fast";

    /// <summary><c>zai/glm-5.3-flash</c>.</summary>
    public const string ZaiGlm53Flash = "zai/glm-5.3-flash";

    /// <summary><c>zai/glm-5.3-flashx</c>.</summary>
    public const string ZaiGlm53Flashx = "zai/glm-5.3-flashx";

    /// <summary><c>zai/glm-5.3-flashx-legal</c>.</summary>
    public const string ZaiGlm53FlashxLegal = "zai/glm-5.3-flashx-legal";

    /// <summary><c>zai/glm-5v-turbo</c>.</summary>
    public const string ZaiGlm5vTurbo = "zai/glm-5v-turbo";
}
