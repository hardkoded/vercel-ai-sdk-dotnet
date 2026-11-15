// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.ProviderUtils.ResolveProviderOptions.ResolveProviderOptionsFixture;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ResolveProviderOptions;

/// <summary>Port of <c>resolve-provider-options.test.ts</c>, the tests outside any nested <c>describe</c>.</summary>
public sealed class ResolveProviderOptionsTests
{
    private const string Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d";

    [Theory]
    [InlineData(ProviderOptionsResolution.First)]
    [InlineData(ProviderOptionsResolution.Merge)]
    [UpstreamTest("packages/provider-utils/src/resolve-provider-options.test.ts::resolveProviderOptions::returns undefined options when no namespace is present (%s)", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Returns_undefined_options_when_no_namespace_is_present(ProviderOptionsResolution resolution)
    {
        var result = Resolve(
            ProviderOptions(("other", "{\"a\":\"other\"}")),
            new[] { "first", "second" },
            resolution);

        Assert.Null(result.Options);
        Assert.Empty(result.Namespaces);
    }

    [Theory]
    [InlineData(ProviderOptionsResolution.First)]
    [InlineData(ProviderOptionsResolution.Merge)]
    [UpstreamTest("packages/provider-utils/src/resolve-provider-options.test.ts::resolveProviderOptions::throws for invalid options in a present namespace (%s)", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Throws_for_invalid_options_in_a_present_namespace(ProviderOptionsResolution resolution)
    {
        Assert.Throws<InvalidArgumentError>(() => Resolve(
            ProviderOptions(("second", "{\"b\":\"invalid\"}")),
            new[] { "first", "second" },
            resolution));
    }
}
