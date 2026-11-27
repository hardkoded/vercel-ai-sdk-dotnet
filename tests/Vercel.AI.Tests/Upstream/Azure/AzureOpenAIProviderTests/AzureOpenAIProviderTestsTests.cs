// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Azure;
using AzureProvider = Vercel.AI.Azure.AzureOpenAIProvider;

namespace Vercel.AI.Tests.Upstream.Azure.AzureOpenAIProviderTests;

/// <summary>Port of the tests of <c>azure-openai-provider.test.ts</c> that sit outside any <c>describe</c>.</summary>
public sealed class AzureOpenAIProviderTestsTests
{
    [Fact]
    [UpstreamTest(
        "packages/azure/src/azure-openai-provider.test.ts::does not validate an unused resource name with a custom endpoint",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Does_not_validate_an_unused_resource_name_with_a_custom_endpoint()
    {
        var provider = AzureProvider.Create(
            new AzureOpenAIOptions
            {
                ResourceName = "not a resource",
                BaseUrl = "https://proxy.example/openai",
                ApiKey = "test-key",
            }
        );

        var exception = Record.Exception(() => provider.LanguageModel("test-deployment"));

        Assert.Null(exception);
    }
}
