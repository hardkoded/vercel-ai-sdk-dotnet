// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>custom</c>.</summary>
public sealed class CustomTests
{
    private const string Prefix =
        "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(
        Prefix + "custom::should append custom parts to the message",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_append_custom_parts_to_the_message()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123"},{"type":"custom","kind":"test-provider.compaction","providerMetadata":{"openai":{"itemId":"cmp_123"}}},{"type":"finish"}]
            """
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            [
              {
                "type": "custom",
                "kind": "test-provider.compaction",
                "providerMetadata": {
                  "openai": {
                    "itemId": "cmp_123"
                  }
                }
              }
            ]
            """,
            harness.Parts
        );
    }
}
