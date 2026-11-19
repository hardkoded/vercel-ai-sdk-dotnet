// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using LiquidProviderType = Vercel.AI.Liquid.LiquidProvider;

namespace Vercel.AI.Tests.Upstream.Liquid.LiquidProvider;

/// <summary>Port of <c>liquid-provider.test.ts</c> &gt; <c>createLiquid</c>.</summary>
public sealed class CreateLiquidTests
{
    private const string Prefix = "packages/liquid/src/liquid-provider.test.ts::createLiquid::";
    private const string Commit = "cf5fe9a22344de9285e77aef4144bbb54e15b788";
    private const string KeyVariable = "LIQUID_API_KEY";

    [Fact]
    [UpstreamTest(
        Prefix + "rejects unsupported model types",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public void Rejects_unsupported_model_types()
    {
        var provider = LiquidProviderType.Create();
        Assert.Throws<AiSdkException>(() => provider.LanguageModel("other"));
        Assert.Throws<AiSdkException>(() => provider.EmbeddingModel("other"));
        Assert.Throws<AiSdkException>(() => provider.ImageModel("other"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(
        Prefix + "uses explicit or environment keys and the default endpoint (explicit=%s)",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Uses_explicit_or_environment_keys_and_the_default_endpoint(bool explicitKey)
    {
        var previous = Environment.GetEnvironmentVariable(KeyVariable);
        Environment.SetEnvironmentVariable(KeyVariable, "env-key");
        try
        {
            var handler = new ParityHandler(_ =>
                ParityHandler.Json("{\"answers\":{\"refund\":{\"type\":\"noul\",\"noul\":0.9}}}")
            );
            var provider = LiquidProviderType.Create(
                new OpenAICompatibleOptions { ApiKey = explicitKey ? "explicit-key" : null },
                handler
            );
            var result = await provider
                .DecisionModel("d1.2-ultra")
                .DoDecideAsync(
                    new DecisionModelCall(
                        [new DecisionModelTextPart("Refund me.")],
                        new Dictionary<string, DecisionQuestion>
                        {
                            ["refund"] = new("boolean", DecisionInput.From("Refund request?")),
                        },
                        JsonDocument.Parse("{}").RootElement.Clone(),
                        new Dictionary<string, string>(),
                        CancellationToken.None
                    )
                );
            Assert.Null(result.Usage);
            Assert.Equal("d1.2-ultra", result.Response!.ModelId);
            var call = Assert.Single(handler.Calls);
            Assert.Equal("https://api.liquid.ai/decisions/v1/systemone", call.Uri.AbsoluteUri);
            Assert.Equal(
                "Bearer " + (explicitKey ? "explicit-key" : "env-key"),
                call.Header("authorization")
            );
            Assert.Contains("ai-sdk/liquid/", call.Header("user-agent"));
            Assert.Equal(
                "d1.2-ultra",
                JsonDocument.Parse(call.Body!).RootElement.GetProperty("model").GetString()
            );
        }
        finally
        {
            Environment.SetEnvironmentVariable(KeyVariable, previous);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "loads credentials lazily and reports a missing key",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Loads_credentials_lazily_and_reports_a_missing_key()
    {
        var previous = Environment.GetEnvironmentVariable(KeyVariable);
        Environment.SetEnvironmentVariable(KeyVariable, null);
        try
        {
            var model = LiquidProviderType
                .Create(handler: new ParityHandler())
                .DecisionModel("d1.2");
            var error = await Assert.ThrowsAsync<AiSdkException>(() =>
                model.DoDecideAsync(
                    new DecisionModelCall(
                        [new DecisionModelTextPart("Refund me.")],
                        new Dictionary<string, DecisionQuestion>
                        {
                            ["refund"] = new("boolean", DecisionInput.From("Refund request?")),
                        },
                        JsonDocument.Parse("{}").RootElement.Clone(),
                        new Dictionary<string, string>(),
                        CancellationToken.None
                    )
                )
            );
            Assert.Contains(KeyVariable, error.Message);
        }
        finally
        {
            Environment.SetEnvironmentVariable(KeyVariable, previous);
        }
    }
}
