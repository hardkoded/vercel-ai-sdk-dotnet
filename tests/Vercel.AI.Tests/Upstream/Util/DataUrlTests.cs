// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class DataUrlTests
{
    [UpstreamTest("packages/ai/src/util/data-url.test.ts::getTextFromDataUrl::should throw InvalidArgumentError for a malformed data URL", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Rejects_a_malformed_data_url()
    {
        const string dataUrl = "not-a-data-url";
        var error = Assert.Throws<InvalidArgumentError>(delegate { DataUrl.GetTextFromDataUrl(dataUrl); });
        Assert.True(InvalidArgumentError.IsInstance(error));
        Assert.Equal("dataUrl", error.Parameter);
        Assert.Equal(dataUrl, Assert.IsType<string>(error.Value!));
        Assert.Equal("Invalid argument for parameter dataUrl: Invalid data URL format", error.Message);
    }

    [UpstreamTest("packages/ai/src/util/data-url.test.ts::getTextFromDataUrl::should throw InvalidArgumentError when the data URL cannot be decoded", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Rejects_invalid_base64()
    {
        const string dataUrl = "data:text/plain;base64,invalid-base64";
        var error = Assert.Throws<InvalidArgumentError>(delegate { DataUrl.GetTextFromDataUrl(dataUrl); });
        Assert.True(InvalidArgumentError.IsInstance(error));
        Assert.Equal("dataUrl", error.Parameter);
        Assert.Equal(dataUrl, Assert.IsType<string>(error.Value!));
        Assert.Equal("Invalid argument for parameter dataUrl: Error decoding data URL", error.Message);
    }

    [UpstreamTest("packages/ai/src/util/data-url.test.ts::getTextFromDataUrl::decodes a base64 text data URL", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Decodes_base64_text()
    {
        Assert.Equal("hi", DataUrl.GetTextFromDataUrl("data:text/plain;base64,aGk="));
    }

    [UpstreamTest("packages/ai/src/util/data-url.test.ts::getTextFromDataUrl::decodes UTF-8 %s", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Decodes_utf8_cases()
    {
        Assert.Equal("café", DataUrl.GetTextFromDataUrl("data:text/plain;charset=utf-8;base64,Y2Fmw6k="));
        Assert.Equal("日本語", DataUrl.GetTextFromDataUrl("data:text/plain;charset=utf-8;base64,5pel5pys6Kqe"));
        Assert.Equal("emoji 🙂", DataUrl.GetTextFromDataUrl("data:text/plain;charset=utf-8;base64,ZW1vamkg8J+Zgg=="));
        Assert.Equal("Cafe\u0301", DataUrl.GetTextFromDataUrl("data:text/plain;charset=utf-8;base64,Q2FmZcyB"));
        Assert.Equal("first line\ncafé\n日本語 🙂", DataUrl.GetTextFromDataUrl("data:text/plain;charset=utf-8;base64,Zmlyc3QgbGluZQpjYWbDqQrml6XmnKzoqp4g8J+Zgg=="));
    }

    [UpstreamTest("packages/ai/src/util/data-url.test.ts::getTextFromDataUrl::decodes text using a non-UTF-8 declared charset", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Decodes_latin1()
    {
        Assert.Equal("café", DataUrl.GetTextFromDataUrl("data:text/plain;charset=iso-8859-1;base64,Y2Fm6Q=="));
    }
}
