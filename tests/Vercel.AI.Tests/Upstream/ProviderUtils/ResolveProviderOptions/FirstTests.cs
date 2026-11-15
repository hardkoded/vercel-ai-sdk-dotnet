// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.ProviderUtils.ResolveProviderOptions.ResolveProviderOptionsFixture;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ResolveProviderOptions;

/// <summary>Port of <c>resolve-provider-options.test.ts</c> &gt; <c>resolveProviderOptions</c> &gt; <c>first</c>.</summary>
public sealed class FirstTests
{
    private const string Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d";

    [Fact]
    [UpstreamTest("packages/provider-utils/src/resolve-provider-options.test.ts::resolveProviderOptions > first::returns the first present namespace as a whole", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Returns_the_first_present_namespace_as_a_whole()
    {
        var result = Resolve(
            ProviderOptions(("second", "{\"a\":\"second\"}"), ("third", "{\"b\":3}")),
            new[] { "first", "second", "third" },
            ProviderOptionsResolution.First);

        Assert.Equal(new TestOptions("second"), result.Options);
        Assert.Equal(new[] { "second" }, result.Namespaces);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/resolve-provider-options.test.ts::resolveProviderOptions > first::treats an empty object as present", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Treats_an_empty_object_as_present()
    {
        var result = Resolve(
            ProviderOptions(("first", "{}"), ("second", "{\"a\":\"second\"}")),
            new[] { "first", "second" },
            ProviderOptionsResolution.First);

        Assert.Equal(new TestOptions(), result.Options);
        Assert.Equal(new[] { "first" }, result.Namespaces);
    }

    [Fact]
    [UpstreamTest("packages/provider-utils/src/resolve-provider-options.test.ts::resolveProviderOptions > first::does not validate namespaces after the first present one", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Does_not_validate_namespaces_after_the_first_present_one()
    {
        var result = Resolve(
            ProviderOptions(("first", "{\"a\":\"first\"}"), ("second", "{\"b\":\"invalid\"}")),
            new[] { "first", "second" },
            ProviderOptionsResolution.First);

        Assert.Equal(new TestOptions("first"), result.Options);
    }
}
