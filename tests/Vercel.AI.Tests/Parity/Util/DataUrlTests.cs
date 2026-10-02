// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class DataUrlTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/data-url.test.ts::getTextFromDataUrl::should throw InvalidArgumentError for a malformed data URL",
        Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_a_malformed_data_url()
    {
        const string dataUrl = "not-a-data-url";
        var error = Assert.Throws<InvalidArgumentException>(() => DataUrls.GetTextFromDataUrl(dataUrl));
        Assert.True(InvalidArgumentException.IsInstance(error));
        Assert.Equal("dataUrl", error.Parameter);
        Assert.Same(dataUrl, error.Value);
        Assert.Equal("Invalid argument for parameter dataUrl: Invalid data URL format", error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/data-url.test.ts::getTextFromDataUrl::should throw InvalidArgumentError when the data URL cannot be decoded",
        Coverage = UpstreamCoverage.Covered)]
    public void Throws_when_base64_cannot_be_decoded()
    {
        const string dataUrl = "data:text/plain;base64,invalid-base64";
        var error = Assert.Throws<InvalidArgumentException>(() => DataUrls.GetTextFromDataUrl(dataUrl));
        Assert.True(InvalidArgumentException.IsInstance(error));
        Assert.Equal("dataUrl", error.Parameter);
        Assert.Same(dataUrl, error.Value);
        Assert.Equal("Invalid argument for parameter dataUrl: Error decoding data URL", error.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/data-url.test.ts::getTextFromDataUrl::decodes a base64 text data URL",
        Coverage = UpstreamCoverage.Covered)]
    public void Decodes_a_base64_text_data_url()
    {
        Assert.Equal("hi", DataUrls.GetTextFromDataUrl("data:text/plain;base64,aGk="));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/data-url.test.ts::getTextFromDataUrl::decodes UTF-8 %s",
        Coverage = UpstreamCoverage.Covered)]
    public void Decodes_utf8_text()
    {
        Assert.Equal("café", DataUrls.GetTextFromDataUrl("data:text/plain;charset=utf-8;base64,Y2Fmw6k="));
        Assert.Equal("日本語", DataUrls.GetTextFromDataUrl("data:text/plain;charset=utf-8;base64,5pel5pys6Kqe"));
        Assert.Equal("emoji 🙂", DataUrls.GetTextFromDataUrl("data:text/plain;charset=utf-8;base64,ZW1vamkg8J+Zgg=="));
        Assert.Equal("Cafe\u0301", DataUrls.GetTextFromDataUrl("data:text/plain;charset=utf-8;base64,Q2FmZcyB"));
        Assert.Equal(
            "first line\ncafé\n日本語 🙂",
            DataUrls.GetTextFromDataUrl("data:text/plain;charset=utf-8;base64,Zmlyc3QgbGluZQpjYWbDqQrml6XmnKzoqp4g8J+Zgg=="));
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/data-url.test.ts::getTextFromDataUrl::decodes text using a non-UTF-8 declared charset",
        Coverage = UpstreamCoverage.Covered)]
    public void Decodes_a_declared_iso_8859_1_charset()
    {
        Assert.Equal("café", DataUrls.GetTextFromDataUrl("data:text/plain;charset=iso-8859-1;base64,Y2Fm6Q=="));
    }
}
