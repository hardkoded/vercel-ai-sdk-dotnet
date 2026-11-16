// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>malformed stream errors</c>.</summary>
public sealed class MalformedStreamErrorsTests
{
    private const string Prefix = "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(Prefix + "malformed stream errors::should throw descriptive error when text-delta is received without text-start", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_descriptive_error_when_text_delta_is_received_without_text_start()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);

        var error = await Assert.ThrowsAsync<UIMessageStreamError>(() => harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"text-delta","id":"text-1","delta":"Hello"}]
            """));

        Assert.Equal(
            "Received text-delta for missing text part with ID \"text-1\". " +
            "Ensure a \"text-start\" chunk is sent before any \"text-delta\" chunks.",
            error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "malformed stream errors::should throw descriptive error when reasoning-delta is received without reasoning-start", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_descriptive_error_when_reasoning_delta_is_received_without_reasoning_start()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);

        var error = await Assert.ThrowsAsync<UIMessageStreamError>(() => harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"reasoning-delta","id":"reasoning-1","delta":"Thinking..."}]
            """));

        Assert.Equal(
            "Received reasoning-delta for missing reasoning part with ID \"reasoning-1\". " +
            "Ensure a \"reasoning-start\" chunk is sent before any \"reasoning-delta\" chunks.",
            error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "malformed stream errors::should throw descriptive error when tool-input-delta is received without tool-input-start", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_descriptive_error_when_tool_input_delta_is_received_without_tool_input_start()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);

        var error = await Assert.ThrowsAsync<UIMessageStreamError>(() => harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-delta","toolCallId":"tool-1","inputTextDelta":"{\"key\":"}]
            """));

        Assert.Equal(
            "Received tool-input-delta for missing tool call with ID \"tool-1\". " +
            "Ensure a \"tool-input-start\" chunk is sent before any \"tool-input-delta\" chunks.",
            error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "malformed stream errors::should not read Object.prototype for missing text part ids", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_read_Object_prototype_for_missing_text_part_ids()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);

        var error = await Assert.ThrowsAsync<UIMessageStreamError>(() => harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"text-delta","id":"__proto__","delta":"Hello"}]
            """));

        Assert.Equal(
            "Received text-delta for missing text part with ID \"__proto__\". " +
            "Ensure a \"text-start\" chunk is sent before any \"text-delta\" chunks.",
            error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "malformed stream errors::should not read Object.prototype for missing reasoning part ids", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_read_Object_prototype_for_missing_reasoning_part_ids()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);

        var error = await Assert.ThrowsAsync<UIMessageStreamError>(() => harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"reasoning-delta","id":"__proto__","delta":"Thinking..."}]
            """));

        Assert.Equal(
            "Received reasoning-delta for missing reasoning part with ID \"__proto__\". " +
            "Ensure a \"reasoning-start\" chunk is sent before any \"reasoning-delta\" chunks.",
            error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "malformed stream errors::should not read Object.prototype for missing partial tool call ids", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_read_Object_prototype_for_missing_partial_tool_call_ids()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);

        var error = await Assert.ThrowsAsync<UIMessageStreamError>(() => harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-delta","toolCallId":"__proto__","inputTextDelta":"{\"key\":"}]
            """));

        Assert.Equal(
            "Received tool-input-delta for missing tool call with ID \"__proto__\". " +
            "Ensure a \"tool-input-start\" chunk is sent before any \"tool-input-delta\" chunks.",
            error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "malformed stream errors::should throw descriptive error when text-end is received without text-start", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_descriptive_error_when_text_end_is_received_without_text_start()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);

        var error = await Assert.ThrowsAsync<UIMessageStreamError>(() => harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"text-end","id":"text-1"}]
            """));

        Assert.Equal(
            "Received text-end for missing text part with ID \"text-1\". " +
            "Ensure a \"text-start\" chunk is sent before any \"text-end\" chunks.",
            error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "malformed stream errors::should throw descriptive error when reasoning-end is received without reasoning-start", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_descriptive_error_when_reasoning_end_is_received_without_reasoning_start()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);

        var error = await Assert.ThrowsAsync<UIMessageStreamError>(() => harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"reasoning-end","id":"reasoning-1"}]
            """));

        Assert.Equal(
            "Received reasoning-end for missing reasoning part with ID \"reasoning-1\". " +
            "Ensure a \"reasoning-start\" chunk is sent before any \"reasoning-end\" chunks.",
            error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "malformed stream errors::should throw UIMessageStreamError with correct properties for text-delta without text-start", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_UIMessageStreamError_with_correct_properties_for_text_delta_without_text_start()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);

        var error = await Assert.ThrowsAsync<UIMessageStreamError>(() => harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"text-delta","id":"missing-id","delta":"Hello"}]
            """));

        Assert.Equal(
            "Received text-delta for missing text part with ID \"missing-id\". " +
            "Ensure a \"text-start\" chunk is sent before any \"text-delta\" chunks.",
            error.Message);
        Assert.True(UIMessageStreamError.IsInstance(error));
        Assert.Equal("text-delta", error.ChunkType);
        Assert.Equal("missing-id", error.ChunkId);
    }

    [Fact]
    [UpstreamTest(Prefix + "malformed stream errors::should throw UIMessageStreamError with correct properties for tool-input-delta without tool-input-start", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_UIMessageStreamError_with_correct_properties_for_tool_input_delta_without_tool_input_start()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);

        var error = await Assert.ThrowsAsync<UIMessageStreamError>(() => harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"tool-input-delta","toolCallId":"missing-tool-id","inputTextDelta":"{\"key\":"}]
            """));

        Assert.Equal(
            "Received tool-input-delta for missing tool call with ID \"missing-tool-id\". " +
            "Ensure a \"tool-input-start\" chunk is sent before any \"tool-input-delta\" chunks.",
            error.Message);
        Assert.True(UIMessageStreamError.IsInstance(error));
        Assert.Equal("tool-input-delta", error.ChunkType);
        Assert.Equal("missing-tool-id", error.ChunkId);
    }
}
