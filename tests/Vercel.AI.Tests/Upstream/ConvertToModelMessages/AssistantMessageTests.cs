// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ConvertToModelMessages.ConvertToModelMessagesSupport;

namespace Vercel.AI.Tests.ConvertToModelMessages;

/// <summary>Port of <c>convert-to-model-messages.test.ts</c> &gt; <c>convertToModelMessages &gt; assistant message</c>.</summary>
[Collection("LogWarnings")]
public sealed class AssistantMessageTests : WarningsOff
{
    private const string Suite = Prefix + "convertToModelMessages > assistant message::";

    [UpstreamTest(Suite + "should convert an assistant message with file parts", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_convert_an_assistant_message_with_file_parts()
    {
        var result = Convert(new[]
        {
            Assistant(new FileUIPart("image/png", "data:image/png;base64,dGVzdA==")),
        });

        AssertEqual(
            """
            [
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "file",
                    "mediaType": "image/png",
                    "data": { "type": "url", "url": "data:image/png;base64,dGVzdA==" }
                  }
                ]
              }
            ]
            """,
            result);
    }

    [UpstreamTest(Suite + "should include filename for assistant file parts when provided", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_include_filename_for_assistant_file_parts_when_provided()
    {
        var result = Convert(new[]
        {
            Assistant(new FileUIPart("image/png", "data:image/png;base64,dGVzdA==") { Filename = "test.png" }),
        });

        AssertEqual(
            """
            [
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "file",
                    "mediaType": "image/png",
                    "data": { "type": "url", "url": "data:image/png;base64,dGVzdA==" },
                    "filename": "test.png"
                  }
                ]
              }
            ]
            """,
            result);
    }

    [UpstreamTest(Suite + "should handle assistant message file parts with provider metadata", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_handle_assistant_message_file_parts_with_provider_metadata()
    {
        var result = Convert(new[]
        {
            Assistant(new FileUIPart("image/png", "data:image/png;base64,dGVzdA==")
            {
                ProviderMetadata = new Dictionary<string, JsonElement>
                {
                    ["testProvider"] = Json("{\"signature\":\"test-signature\"}"),
                },
            }),
        });

        AssertEqual(
            """
            [
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "file",
                    "mediaType": "image/png",
                    "data": { "type": "url", "url": "data:image/png;base64,dGVzdA==" },
                    "providerOptions": { "testProvider": { "signature": "test-signature" } }
                  }
                ]
              }
            ]
            """,
            result);
    }

    [UpstreamTest(Suite + "should use providerReference as data for assistant file parts", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_use_providerReference_as_data_for_assistant_file_parts()
    {
        var result = Convert(new[]
        {
            Assistant(new FileUIPart("application/pdf", "data:application/pdf;base64,xyz")
            {
                Filename = "doc.pdf",
                ProviderReference = new Dictionary<string, string> { ["anthropic"] = "file-xyz789" },
            }),
        });

        AssertEqual(
            """
            [
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "file",
                    "mediaType": "application/pdf",
                    "filename": "doc.pdf",
                    "data": { "type": "reference", "reference": { "anthropic": "file-xyz789" } }
                  }
                ]
              }
            ]
            """,
            result);
    }
}
