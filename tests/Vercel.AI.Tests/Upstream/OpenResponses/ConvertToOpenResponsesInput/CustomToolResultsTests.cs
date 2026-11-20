// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenResponses;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.OpenResponses.ConvertToOpenResponsesInput.ConvertToOpenResponsesInputSupport;

namespace Vercel.AI.Tests.Upstream.OpenResponses.ConvertToOpenResponsesInput;

/// <summary>Port of <c>convert-to-open-responses-input.test.ts</c> &gt; <c>custom tool results</c>.</summary>
public sealed class CustomToolResultsTests
{
    private const string Prefix =
        "packages/open-responses/src/responses/convert-to-open-responses-input.test.ts::custom tool results::";

    private static OpenResponsesInputResult ConvertCustom(ToolModelMessage message)
    {
        return OpenResponsesInput.Convert(
            [message],
            providerToolsByName: new Dictionary<string, LanguageModelTool>
            {
                ["render"] = LanguageModelTool.ProviderDefined(
                    "acme.custom",
                    "render",
                    JsonDocument.Parse("{}").RootElement
                ),
            },
            customToolId: "acme.custom"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves text, images, and files in custom tool history",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Preserves_text_images_and_files_in_custom_tool_history()
    {
        var result = ConvertCustom(
            new ToolModelMessage(
                "call_1",
                "render",
                new UserContentPart[]
                {
                    new TextContentPart("Rendered result"),
                    UrlFile("image/png", "https://example.com/result.png"),
                    UrlFile("application/pdf", "https://example.com/result.pdf"),
                    File("image/png", "aW1hZ2U="),
                    File("application/pdf", "cGRm", "result.pdf"),
                }
            )
        );

        Assert.Empty(result.Warnings);
        JsonAssert.Equal(
            result.Input,
            """
            [{"type":"custom_tool_call_output","call_id":"call_1","output":[
              {"type":"input_text","text":"Rendered result"},
              {"type":"input_image","image_url":"https://example.com/result.png","detail":"auto"},
              {"type":"input_file","file_url":"https://example.com/result.pdf"},
              {"type":"input_image","image_url":"data:image/png;base64,aW1hZ2U=","detail":"auto"},
              {"type":"input_file","filename":"result.pdf","file_data":"data:application/pdf;base64,cGRm"}
            ]}]
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "warns when a custom tool result contains unsupported provider file references",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Warns_when_a_custom_tool_result_contains_unsupported_provider_file_references()
    {
        var result = ConvertCustom(
            new ToolModelMessage(
                "call_1",
                "render",
                new UserContentPart[]
                {
                    FileContentPart.FromProviderReference(
                        "image/png",
                        new Dictionary<string, string> { ["acme"] = "file_1" }
                    ),
                }
            )
        );

        var warning = Assert.Single(result.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Equal(
            "unsupported tool content part type: file with data type: reference",
            warning.Message
        );
    }
}
