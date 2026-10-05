// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class HeaderTests
{
    [UpstreamTest("packages/ai/src/util/prepare-headers.test.ts::prepareHeaders::should set Content-Type header if not present", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Sets_content_type_when_missing()
    {
        var headers = PrepareHeaders.Apply(new HeaderCollection(), Defaults());
        Assert.Equal("application/json", headers.Get("Content-Type")!);
    }

    [UpstreamTest("packages/ai/src/util/prepare-headers.test.ts::prepareHeaders::should not overwrite existing Content-Type header", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Keeps_an_existing_content_type()
    {
        var init = new HeaderCollection();
        init.Add("Content-Type", "text/html");
        var headers = PrepareHeaders.Apply(init, Defaults());
        Assert.Equal("text/html", headers.Get("Content-Type")!);
    }

    [UpstreamTest("packages/ai/src/util/prepare-headers.test.ts::prepareHeaders::should handle undefined init", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Accepts_null_init()
    {
        var headers = PrepareHeaders.Apply(null, Defaults());
        Assert.Equal("application/json", headers.Get("Content-Type")!);
    }

    [UpstreamTest("packages/ai/src/util/prepare-headers.test.ts::prepareHeaders::should handle init headers as Headers object", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Copies_a_header_collection()
    {
        var init = new HeaderCollection();
        init.Add("init", "foo");
        var headers = PrepareHeaders.Apply(init, Defaults());
        Assert.Equal("foo", headers.Get("init")!);
        Assert.Equal("application/json", headers.Get("Content-Type")!);
    }

    [UpstreamTest("packages/ai/src/util/prepare-headers.test.ts::prepareHeaders::should handle Response object headers", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Copies_response_headers()
    {
        var init = new HeaderCollection();
        init.Add("init", "foo");
        init.Add("extra", "bar");
        var headers = PrepareHeaders.Apply(init, Defaults());
        Assert.Equal("foo", headers.Get("init")!);
        Assert.Equal("bar", headers.Get("extra")!);
        Assert.Equal("application/json", headers.Get("Content-Type")!);
    }

    private static Dictionary<string, string> Defaults()
    {
        return new Dictionary<string, string> { { "content-type", "application/json" } };
    }
}
