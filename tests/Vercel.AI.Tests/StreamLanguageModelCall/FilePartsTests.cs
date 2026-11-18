// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.StreamLanguageModelCall.StreamLanguageModelCallFixture;

namespace Vercel.AI.Tests.StreamLanguageModelCall;

/// <summary>Port of <c>stream-language-model-call.test.ts</c> &gt; <c>streamLanguageModelCall &gt; file parts</c>.</summary>
[Collection("DownloadFetch")]
public sealed class FilePartsTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-language-model-call.test.ts::streamLanguageModelCall > file parts::";

    private static readonly byte[] HelloWorld = Encoding.UTF8.GetBytes("Hello World");

    [Fact]
    [UpstreamTest(Prefix + "should use GeneratedFile", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_GeneratedFile()
    {
        var result = await StreamLanguageModelCallResult([
            new FileStreamPart(HelloWorld, "text/plain"),
            Finish(),
        ]);

        Assert.Equal(2, result.Count);
        var file = Assert.IsType<FilePart>(result[0]);
        Assert.Equal(HelloWorld, file.File.Data);
        Assert.Equal("text/plain", file.File.MediaType);
        Assert.Null(file.ProviderMetadata);
        var end = ModelCallEnd(result[1]);
        Assert.Equal(FinishReason.Stop, end.FinishReason);
        Assert.Equal(0, end.Performance.EffectiveOutputTokensPerSecond);
        Assert.Equal(0, end.Performance.EffectiveTotalTokensPerSecond);
        Assert.Equal(0, end.Performance.InputTokensPerSecond);
        Assert.Equal(0, end.Performance.OutputTokensPerSecond);
        Assert.Equal(0, end.Performance.ResponseTimeMs);
        Assert.Null(end.Performance.TimeBetweenOutputChunksMs);
        Assert.Equal(0, end.Performance.TimeToFirstOutputMs);
    }

    [Theory]
    [InlineData("file")]
    [InlineData("reasoning-file")]
    [UpstreamTest(Prefix + "should download HTTPS %s parts", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_download_HTTPS_parts(string type)
    {
        var fetches = 0;
        var previous = Download.Fetch;
        try
        {
            Download.Fetch = (url, request) =>
            {
                fetches++;
                return Task.FromResult(DownloadResponse.Text("Hello World"));
            };
            LanguageModelStreamPart part =
                type == "file"
                    ? new FileStreamPart(new Uri("https://example.com/generated.txt"), "text/plain")
                    : new ReasoningFileStreamPart(
                        new Uri("https://example.com/generated.txt"),
                        "text/plain"
                    );

            var result = await StreamLanguageModelCallResult([part, Finish()]);

            var file = result[0] switch
            {
                FilePart filePart when type == "file" => filePart.File,
                ReasoningFilePart reasoningFilePart when type == "reasoning-file" =>
                    reasoningFilePart.File,
                _ => throw new InvalidOperationException("Expected a file part."),
            };
            Assert.Equal("SGVsbG8gV29ybGQ=", Convert.ToBase64String(file.Data));
            Assert.Equal(HelloWorld, file.Data);
            Assert.Equal(1, fetches);
        }
        finally
        {
            Download.Fetch = previous;
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use GeneratedFile with providerMetadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_GeneratedFile_with_providerMetadata()
    {
        var result = await StreamLanguageModelCallResult([
            new FileStreamPart(
                HelloWorld,
                "text/plain",
                Json("{\"testProvider\":{\"signature\":\"test-signature\"}}")
            ),
            Finish(),
        ]);

        Assert.Equal(2, result.Count);
        var file = Assert.IsType<FilePart>(result[0]);
        Assert.Equal(HelloWorld, file.File.Data);
        Assert.Equal("text/plain", file.File.MediaType);
        Assert.Equal(
            "{\"testProvider\":{\"signature\":\"test-signature\"}}",
            file.ProviderMetadata!.Value.GetRawText()
        );
        var end = ModelCallEnd(result[1]);
        Assert.Equal(FinishReason.Stop, end.FinishReason);
        Assert.Equal(0, end.Performance.ResponseTimeMs);
        Assert.Equal(0, end.Performance.TimeToFirstOutputMs);
        Assert.Null(end.Performance.TimeBetweenOutputChunksMs);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should measure time to first output from file parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_measure_time_to_first_output_from_file_parts()
    {
        var result = await StreamLanguageModelCallResult(
            [new FileStreamPart(HelloWorld, "text/plain"), Finish()],
            now: Now(1000, 1250, 1600)
        );

        AssertTimeToFirstOutput(ModelCallEnd(result[^1]));
    }
}
