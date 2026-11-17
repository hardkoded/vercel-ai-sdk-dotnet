// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>file parts with providerMetadata</c>.</summary>
public sealed class FilePartsWithProviderMetadataTests
{
    private const string Prefix =
        "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Fact]
    [UpstreamTest(
        Prefix + "file parts with providerMetadata::should have the correct final message state",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_have_the_correct_final_message_state()
    {
        var harness = new ProcessUIMessageStreamHarness();
        harness.CreateState("msg-123", null);
        await harness.ProcessAsync(
            """
            [{"type":"start","messageId":"msg-123"},{"type":"start-step"},{"type":"file","url":"data:text/plain;base64,SGVsbG8gV29ybGQ=","mediaType":"text/plain","providerMetadata":{"testProvider":{"signature":"sig-1"}}},{"type":"file","url":"data:image/jpeg;base64,QkFVRw==","mediaType":"image/jpeg"},{"type":"finish-step"},{"type":"finish"}]
            """
        );

        ProcessUIMessageStreamHarness.AssertJson(
            """
            {
              "id": "msg-123",
              "parts": [
                {
                  "type": "step-start"
                },
                {
                  "mediaType": "text/plain",
                  "providerMetadata": {
                    "testProvider": {
                      "signature": "sig-1"
                    }
                  },
                  "type": "file",
                  "url": "data:text/plain;base64,SGVsbG8gV29ybGQ="
                },
                {
                  "mediaType": "image/jpeg",
                  "type": "file",
                  "url": "data:image/jpeg;base64,QkFVRw=="
                }
              ],
              "role": "assistant"
            }
            """,
            harness.Message
        );
    }
}
