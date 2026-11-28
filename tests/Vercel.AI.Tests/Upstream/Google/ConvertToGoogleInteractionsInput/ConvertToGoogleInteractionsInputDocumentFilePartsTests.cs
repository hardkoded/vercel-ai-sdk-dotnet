// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.Google.ConvertToGoogleInteractionsInput;

/// <summary>Port of <c>convert-to-google-interactions-input.test.ts</c> &gt; <c>convertToGoogleInteractionsInput</c> &gt; <c>document file parts</c>.</summary>
public sealed class ConvertToGoogleInteractionsInputDocumentFilePartsTests
{
    private const string Prefix =
        ConvertToGoogleInteractionsInputSupport.Id + "document file parts::";

    [Fact]
    [UpstreamTest(
        Prefix + "maps an application/pdf data part to a document block",
        Coverage = UpstreamCoverage.Covered
    )]
    public void maps_an_application_pdf_data_part_to_a_document_block()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    new FileContentPart("application/pdf", null, new byte[] { 1, 2, 3 }, null),
                }
            )
        );

        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"document\",\"data\":\"AQID\",\"mime_type\":\"application/pdf\"}]}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "passes a base64-string PDF data part through as a document block",
        Coverage = UpstreamCoverage.Covered
    )]
    public void passes_a_base64_string_PDF_data_part_through_as_a_document_block()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage(
                new UserContentPart[] { new FileDataContentPart("JVBERi0=", "application/pdf") }
            )
        );

        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"document\",\"data\":\"JVBERi0=\",\"mime_type\":\"application/pdf\"}]}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps a url PDF data part to a document block with `uri` (URL passthrough)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void maps_a_url_PDF_data_part_to_a_document_block_with_uri_URL_passthrough()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    new TextContentPart("Summarize this"),
                    new FileContentPart(
                        "application/pdf",
                        "https://example.com/paper.pdf",
                        null,
                        null
                    ),
                }
            )
        );

        GoogleUpstream.JsonEqual(
            ConvertToGoogleInteractionsInputSupport.Snapshot(result),
            "{\"input\":[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"Summarize this\"},{\"type\":\"document\",\"mime_type\":\"application/pdf\",\"uri\":\"https://example.com/paper.pdf\"}]}],\"warnings\":[]}"
        );
        Assert.Null(result.SystemInstruction);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "maps a reference PDF data part to a document block with the resolved Files API URI",
        Coverage = UpstreamCoverage.Covered
    )]
    public void maps_a_reference_PDF_data_part_to_a_document_block_with_the_resolved_Files_API_URI()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    FileContentPart.FromProviderReference(
                        "application/pdf",
                        new Dictionary<string, string>
                        {
                            ["google"] =
                                "https://generativelanguage.googleapis.com/v1beta/files/doc-xyz",
                        }
                    ),
                }
            )
        );

        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"document\",\"mime_type\":\"application/pdf\",\"uri\":\"https://generativelanguage.googleapis.com/v1beta/files/doc-xyz\"}]}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "collapses a text-data document part to an inline text block (no text+data on the wire)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void collapses_a_text_data_document_part_to_an_inline_text_block_no_text_data_on_the_wire()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    FileContentPart.FromText("application/pdf", "extracted PDF body"),
                }
            )
        );

        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"extracted PDF body\"}]}]"
        );
    }
}
