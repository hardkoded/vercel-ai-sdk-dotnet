// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.ProviderUtils.ResolveProviderOptions.ResolveProviderOptionsFixture;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ResolveProviderOptions;

/// <summary>Port of <c>resolve-provider-options.test.ts</c> &gt; <c>resolveProviderOptions</c> &gt; <c>merge</c>.</summary>
public sealed class MergeTests
{
    private const string Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d";

    [Fact]
    [UpstreamTest("packages/provider-utils/src/resolve-provider-options.test.ts::resolveProviderOptions > merge::merges present namespaces in order, later namespaces win", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Merges_present_namespaces_in_order_later_namespaces_win()
    {
        var result = Resolve(
            ProviderOptions(("first", "{\"a\":\"first\",\"b\":1}"), ("third", "{\"a\":\"third\"}")),
            new[] { "first", "second", "third" },
            ProviderOptionsResolution.Merge);

        Assert.Equal(new TestOptions("third", 1), result.Options);
        Assert.Equal(new[] { "first", "third" }, result.Namespaces);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/resolve-provider-options.test.ts::resolveProviderOptions > merge::ignores duplicate namespaces", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Ignores_duplicate_namespaces()
    {
        var result = Resolve(
            ProviderOptions(("first", "{\"a\":\"first\"}")),
            new[] { "first", "first" },
            ProviderOptionsResolution.Merge);

        Assert.Equal(new TestOptions("first"), result.Options);
        Assert.Equal(new[] { "first" }, result.Namespaces);
    }
}
