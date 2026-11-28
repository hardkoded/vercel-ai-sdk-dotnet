// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using S = Vercel.AI.Tests.Upstream.Google.ConvertToGoogleInteractionsInput.ConvertToGoogleInteractionsInputMessageSupport;

namespace Vercel.AI.Tests.Upstream.Google.ConvertToGoogleInteractionsInput;

/// <summary>Port of the <c>convert-to-google-interactions-input.test.ts</c> tests outside any nested <c>describe</c>.</summary>
public sealed class ConvertToGoogleInteractionsInputTests
{
    private const string Prefix = ConvertToGoogleInteractionsInputSupport.Id;
    private const string FileUri = "https://generativelanguage.googleapis.com/v1beta/files/report";

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(
        Prefix + "keeps mixed file content inside its tool result (compact: %s)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Keeps_mixed_file_content_inside_its_tool_result(bool compact)
    {
        var reference = new Dictionary<string, string> { ["google"] = FileUri };
        var first = new ToolModelMessage(
            "first-call",
            "viewFile",
            new UserContentPart[]
            {
                new TextContentPart("First selection"),
                new FileContentPart("application/pdf", null, new byte[] { 1, 2, 3 }, null)
                {
                    ProviderOptions = GoogleUpstream.ProviderOptions(
                        "{\"google\":{\"resolution\":\"high\",\"processing\":{\"type\":\"static\",\"pageOptions\":{\"startPage\":1,\"endPage\":1}}}}"
                    ),
                },
                new FileContentPart("image/png", null, new byte[] { 4, 5, 6 }, null),
                FileContentPart.FromProviderReference(
                    "audio/mpeg",
                    reference,
                    null,
                    GoogleUpstream.ProviderOptions(
                        "{\"google\":{\"processing\":{\"type\":\"static\",\"startOffset\":0,\"endOffset\":10}}}"
                    )
                ),
                new FileContentPart("video/mp4", FileUri, null, null),
            }
        )
        {
            ProviderOptions = GoogleUpstream.ProviderOptions(
                "{\"google\":{\"signature\":\"first-signature\"}}"
            ),
        };
        var second = new ToolModelMessage(
            "second-call",
            "viewFile",
            new UserContentPart[]
            {
                FileContentPart.FromProviderReference(
                    "application/pdf",
                    reference,
                    null,
                    GoogleUpstream.ProviderOptions(
                        "{\"google\":{\"processing\":{\"type\":\"static\",\"pageOptions\":{\"startPage\":2,\"endPage\":2}}}}"
                    )
                ),
            }
        )
        {
            ProviderOptions = GoogleUpstream.ProviderOptions(
                "{\"google\":{\"signature\":\"second-signature\"}}"
            ),
        };

        var result = S.Convert(
            compact ? "stored-interaction" : null,
            null,
            S.Assistant(
                S.ToolCall(
                    "first-call",
                    "viewFile",
                    "{}",
                    "{\"google\":{\"interactionId\":\"stored-interaction\"}}"
                )
            ),
            S.Assistant(S.ToolCall("second-call", "viewFile", "{}")),
            new ToolModelMessage(new[] { first, second })
        );

        var firstResult =
            "{\"type\":\"function_result\",\"call_id\":\"first-call\",\"name\":\"viewFile\",\"signature\":\"first-signature\",\"result\":[{\"type\":\"text\",\"text\":\"First selection\"},{\"type\":\"document\",\"data\":\"AQID\",\"mime_type\":\"application/pdf\",\"resolution\":\"high\",\"processing\":{\"type\":\"static\",\"page_options\":{\"start_page\":1,\"end_page\":1}}},{\"type\":\"image\",\"data\":\"BAUG\",\"mime_type\":\"image/png\"},{\"type\":\"audio\",\"uri\":\""
            + FileUri
            + "\",\"mime_type\":\"audio/mpeg\",\"processing\":{\"type\":\"static\",\"start_offset\":\"0s\",\"end_offset\":\"10s\"}},{\"type\":\"video\",\"uri\":\""
            + FileUri
            + "\",\"mime_type\":\"video/mp4\"}]}";
        var secondResult =
            "{\"type\":\"function_result\",\"call_id\":\"second-call\",\"name\":\"viewFile\",\"signature\":\"second-signature\",\"result\":[{\"type\":\"document\",\"uri\":\""
            + FileUri
            + "\",\"mime_type\":\"application/pdf\",\"processing\":{\"type\":\"static\",\"page_options\":{\"start_page\":2,\"end_page\":2}}}]}";
        var functionResults = new JsonArray();
        foreach (var step in result.Input)
        {
            if ((string?)step!["type"] == "function_result")
            {
                functionResults.Add(step.DeepClone());
            }
        }

        GoogleUpstream.JsonEqual(
            functionResults,
            compact ? "[" + secondResult + "]" : "[" + firstResult + "," + secondResult + "]"
        );
        Assert.Empty(result.Warnings);
    }
}
