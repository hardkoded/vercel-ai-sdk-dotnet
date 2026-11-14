// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using static Vercel.AI.Tests.Upstream.Azure.AzureTranscriptionTestSupport;

namespace Vercel.AI.Tests.Upstream.Azure.AzureTranscriptionModel;

/// <summary>Port of <c>azure-transcription-model.test.ts</c> &gt; <c>provider</c>.</summary>
public sealed class ProviderTests
{
    private const string Prefix = "packages/azure/src/azure-transcription-model.test.ts::provider::";

    [Fact]
    [UpstreamTest(Prefix + "exposes transcriptionModel as an alias of transcription", Coverage = UpstreamCoverage.Covered, Commit = "6400cf8b881c1167e97d09be7f1bc74c97781d9d")]
    public async Task Exposes_transcriptionModel_as_an_alias_of_transcription()
    {
        var harness = Setup();

        var model = (ITranscriptionCaller)harness.Provider.TranscriptionModel("mai-transcribe-2");
        await model.DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Contains("/speechtotext/", harness.Request.Url);
    }
}
