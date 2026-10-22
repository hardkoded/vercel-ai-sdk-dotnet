// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Alibaba;
using static Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests.AlibabaChatTestSupport;

namespace Vercel.AI.Tests.Upstream.Alibaba.GetCacheControl;

/// <summary>Port of <c>get-cache-control.test.ts</c> &gt; <c>CacheControlValidator</c>.</summary>
public sealed class CacheControlValidatorTests
{
    private const string Prefix = "packages/alibaba/src/get-cache-control.test.ts::CacheControlValidator::";

    private const string Ephemeral = """{"cacheControl":{"type":"ephemeral"}}""";

    [Fact]
    [UpstreamTest(Prefix + "should extract cacheControl from providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public void Should_extract_cacheControl_from_providerMetadata()
    {
        var validator = new AlibabaCacheControlValidator();

        var result = validator.GetCacheControl(AlibabaOptions(Ephemeral));

        JsonAssert.Equal(result, """{"type":"ephemeral"}""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn when exceeding 4 cache breakpoints", Coverage = UpstreamCoverage.Covered)]
    public void Should_warn_when_exceeding_4_cache_breakpoints()
    {
        var validator = new AlibabaCacheControlValidator();

        for (var i = 0; i < 4; i++)
        {
            validator.GetCacheControl(AlibabaOptions(Ephemeral));
        }

        var result = validator.GetCacheControl(AlibabaOptions(Ephemeral));

        JsonAssert.Equal(result, """{"type":"ephemeral"}""");
        var warning = Assert.Single(validator.Warnings);
        Assert.Equal("Max breakpoint limit exceeded. Only the last 4 cache markers will take effect.", warning.Message);
        Assert.Equal("other", warning.Type);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return undefined when no cache control is present", Coverage = UpstreamCoverage.Covered)]
    public void Should_return_undefined_when_no_cache_control_is_present()
    {
        var validator = new AlibabaCacheControlValidator();

        var result = validator.GetCacheControl(AlibabaOptions("{}"));

        Assert.Null(result);
    }
}
