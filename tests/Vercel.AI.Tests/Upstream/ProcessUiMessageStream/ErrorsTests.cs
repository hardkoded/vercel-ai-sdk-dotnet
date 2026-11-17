// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>errors</c>.</summary>
public sealed class ErrorsTests
{
    private const string Prefix =
        "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(
        Prefix + "errors::should call the update function with the correct arguments",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_the_update_function_with_the_correct_arguments()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        var errors = new JsonArray();
        await harness.ProcessAsync(
            """
            [{"type":"error","errorText":"test error"}]
            """,
            harness.Options(error => errors.Add(error.Message))
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            []
            """,
            harness.WriteCalls
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "errors::should have the correct final message state",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_have_the_correct_final_message_state()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        var errors = new JsonArray();
        await harness.ProcessAsync(
            """
            [{"type":"error","errorText":"test error"}]
            """,
            harness.Options(error => errors.Add(error.Message))
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            {
              "id": "msg-123",
              "parts": [],
              "role": "assistant"
            }
            """,
            harness.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "errors::should call the onError function with the correct arguments",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_the_onError_function_with_the_correct_arguments()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        var errors = new JsonArray();
        await harness.ProcessAsync(
            """
            [{"type":"error","errorText":"test error"}]
            """,
            harness.Options(error => errors.Add(error.Message))
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            [
              "test error"
            ]
            """,
            errors
        );
    }
}
