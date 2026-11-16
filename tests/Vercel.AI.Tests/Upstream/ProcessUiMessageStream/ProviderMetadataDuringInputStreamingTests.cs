// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>provider metadata during input-streaming</c>.</summary>
public sealed class ProviderMetadataDuringInputStreamingTests
{
    private const string Prefix =
        "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(
        Prefix
            + "provider metadata during input-streaming::should propagate providerMetadata during input-streaming state",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_propagate_providerMetadata_during_input_streaming_state()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-start","toolCallId":"tool-call-id","toolName":"tool-name","providerMetadata":{"testProvider":{"someKey":"someValue"}}},{"type":"tool-input-delta","toolCallId":"tool-call-id","inputTextDelta":"{\"query\":"},{"type":"tool-input-delta","toolCallId":"tool-call-id","inputTextDelta":"\"test\"}"},{"type":"tool-input-available","toolCallId":"tool-call-id","toolName":"tool-name","input":{"query":"test"}},{"type":"finish-step"},{"type":"finish"}]
            """
        );

        var inputStreamingUpdate = harness.FindWriteCall(part =>
            (string?)part["toolCallId"] == "tool-call-id"
            && (string?)part["state"] == "input-streaming"
        );
        Assert.NotNull(inputStreamingUpdate);
        var toolPart = ProcessUIMessageStreamHarness.Find(
            (JsonArray)inputStreamingUpdate["message"]!["parts"]!,
            part => (string?)part["toolCallId"] == "tool-call-id"
        )!;
        ProcessUIMessageStreamHarness.AssertJson(
            """
            {"testProvider":{"someKey":"someValue"}}
            """,
            toolPart["callProviderMetadata"]
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "provider metadata during input-streaming::should retain providerMetadata in final input-available state",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_retain_providerMetadata_in_final_input_available_state()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-start","toolCallId":"tool-call-id","toolName":"tool-name","providerMetadata":{"testProvider":{"someKey":"someValue"}}},{"type":"tool-input-delta","toolCallId":"tool-call-id","inputTextDelta":"{\"query\":"},{"type":"tool-input-delta","toolCallId":"tool-call-id","inputTextDelta":"\"test\"}"},{"type":"tool-input-available","toolCallId":"tool-call-id","toolName":"tool-name","input":{"query":"test"}},{"type":"finish-step"},{"type":"finish"}]
            """
        );

        var toolPart = harness.FindToolPart("tool-call-id")!;
        Assert.Equal("input-available", (string?)toolPart["state"]);
        ProcessUIMessageStreamHarness.AssertJson(
            """
            {"testProvider":{"someKey":"someValue"}}
            """,
            toolPart["callProviderMetadata"]
        );
    }
}
