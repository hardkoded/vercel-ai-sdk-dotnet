// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.OpenResponsesLanguageModelTests.OpenResponsesLanguageModelTestSupport;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel</c>.</summary>
public sealed class OpenResponsesLanguageModelTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel::";

    [Fact]
    [UpstreamTest(Prefix + "should reject workflow serialization when extension codecs are registered", Coverage = UpstreamCoverage.Covered)]
    public void Should_reject_workflow_serialization_when_extension_codecs_are_registered()
    {
        var model = CreateModel(new UpstreamCapture(), "gemma-7b-it", CreateDocumentSearchExtension(providerExecuted: true));

        var error = Assert.Throws<SerializationError>(() => model.Serialize());

        Assert.Contains("Open Responses models with registered extensions cannot be serialized across workflow boundaries.", error.Message, StringComparison.Ordinal);
    }
}
