// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.GoogleVertexVideoModel.GoogleVertexVideoModelFixture;

namespace Vercel.AI.Tests.GoogleVertexVideoModel;

/// <summary>Port of <c>google-vertex-video-model.test.ts</c> &gt; <c>GoogleVertexVideoModel</c> &gt; <c>doStart</c> &gt; <c>Provider Options</c>.</summary>
public sealed class DoStartProviderOptionsTests
{
    private const string Id =
        "packages/google-vertex/src/google-vertex-video-model.test.ts::GoogleVertexVideoModel > doStart > Provider Options::";

    [Fact]
    [UpstreamTest(Id + "should pass personGeneration option", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassPersonGenerationOption()
    {
        var server = new VertexVideoServer();

        await CreateModel(server)
            .DoStartAsync(
                Call(providerOptions: "{\"vertex\":{\"personGeneration\":\"allow_adult\"}}"),
                CancellationToken.None
            );

        AssertJson(
            "{\"instances\":[{\"prompt\":\"A futuristic city with flying cars\"}],\"parameters\":{\"sampleCount\":1,\"personGeneration\":\"allow_adult\"}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(Id + "should pass negativePrompt option", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassNegativePromptOption()
    {
        var server = new VertexVideoServer();

        await CreateModel(server)
            .DoStartAsync(
                Call(providerOptions: "{\"vertex\":{\"negativePrompt\":\"blurry, low quality\"}}"),
                CancellationToken.None
            );

        AssertJson(
            "{\"instances\":[{\"prompt\":\"A futuristic city with flying cars\"}],\"parameters\":{\"sampleCount\":1,\"negativePrompt\":\"blurry, low quality\"}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(Id + "should pass generateAudio option", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassGenerateAudioOption()
    {
        var server = new VertexVideoServer();

        await CreateModel(server)
            .DoStartAsync(
                Call(providerOptions: "{\"vertex\":{\"generateAudio\":true}}"),
                CancellationToken.None
            );

        AssertJson(
            "{\"instances\":[{\"prompt\":\"A futuristic city with flying cars\"}],\"parameters\":{\"sampleCount\":1,\"generateAudio\":true}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass gcsOutputDirectory option",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task ShouldPassGcsOutputDirectoryOption()
    {
        var server = new VertexVideoServer();

        await CreateModel(server)
            .DoStartAsync(
                Call(
                    providerOptions: "{\"vertex\":{\"gcsOutputDirectory\":\"gs://bucket/output/\"}}"
                ),
                CancellationToken.None
            );

        AssertJson(
            "{\"instances\":[{\"prompt\":\"A futuristic city with flying cars\"}],\"parameters\":{\"sampleCount\":1,\"gcsOutputDirectory\":\"gs://bucket/output/\"}}",
            CapturedBody(server)
        );
    }

    [Fact]
    [UpstreamTest(Id + "should pass referenceImages option", Coverage = UpstreamCoverage.Covered)]
    public async Task ShouldPassReferenceImagesOption()
    {
        var server = new VertexVideoServer();

        await CreateModel(server)
            .DoStartAsync(
                Call(
                    providerOptions: "{\"vertex\":{\"referenceImages\":[{\"bytesBase64Encoded\":\"reference-image-data\"},{\"gcsUri\":\"gs://bucket/reference.png\"}]}}"
                ),
                CancellationToken.None
            );

        AssertJson(
            "[{\"bytesBase64Encoded\":\"reference-image-data\"},{\"gcsUri\":\"gs://bucket/reference.png\"}]",
            CapturedBody(server)!["instances"]![0]!["referenceImages"]
        );
    }
}
