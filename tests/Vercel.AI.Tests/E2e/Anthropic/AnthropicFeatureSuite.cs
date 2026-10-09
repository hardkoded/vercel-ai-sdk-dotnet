// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Anthropic;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.E2e.Anthropic;

/// <summary>Port of examples/ai-functions/src/e2e/anthropic.test.ts: models and shared helpers for the feature suite.</summary>
internal static class AnthropicFeatureSuite
{
    public const string Category = "RealModel";

    /// <summary>The upstream languageModels list, all with the default chat capabilities.</summary>
    public static IEnumerable<object[]> LanguageModels()
    {
        yield return new object[] { "claude-sonnet-4-20250514" };
        yield return new object[] { "claude-haiku-4-5-20251001" };
        yield return new object[] { "claude-haiku-5-5" };
    }

    public static void SkipWithoutApiKey()
    {
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")));
    }

    public static ILanguageModel Chat(string modelId) => AnthropicProvider.Create().LanguageModel(modelId);

    public static AiClient Client() => new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "unused" }));

    public static byte[] Fixture(string name) => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));
}
