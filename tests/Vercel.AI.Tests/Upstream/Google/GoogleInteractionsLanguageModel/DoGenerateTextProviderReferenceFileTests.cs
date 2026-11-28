// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.GoogleInteractionsLanguageModel;

/// <summary>
/// Port of <c>google-interactions-language-model.test.ts</c> &gt; <c>GoogleInteractionsLanguageModel.doGenerate</c> &gt; <c>text ProviderReference file</c>.
/// </summary>
public sealed class DoGenerateTextProviderReferenceFileTests
{
    private const string Prefix = DoGenerateSupport.Id + "text ProviderReference file::";

    [Fact]
    [UpstreamTest(
        Prefix + "forwards the uploaded text document to the model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Forwards_the_uploaded_text_document_to_the_model()
    {
        var handler = new SequencedHandler().Json(
            GoogleInteractionsStreamSupport.JsonFixture("basic")
        );

        var result = await GoogleInteractionsStreamSupport
            .Model(handler, "gemini-3.5-flash")
            .DoGenerateAsync(
                GoogleUpstream.Prompt(
                    new UserModelMessage(
                        new UserContentPart[]
                        {
                            new TextContentPart(
                                "Return only the secret verification code from the attached text document."
                            ),
                            FileContentPart.FromProviderReference(
                                "text/plain",
                                new Dictionary<string, string>
                                {
                                    ["google"] =
                                        "https://generativelanguage.googleapis.com/v1beta/files/gzed1s6hqcsn",
                                }
                            ),
                        }
                    )
                ),
                CancellationToken.None
            );

        GoogleUpstream.JsonEqual(
            DoGenerateSupport.Body(handler)["input"],
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"Return only the secret verification code from the attached text document.\"},{\"type\":\"document\",\"uri\":\"https://generativelanguage.googleapis.com/v1beta/files/gzed1s6hqcsn\",\"mime_type\":\"text/plain\"}]}]"
        );
        Assert.Empty(result.Warnings);
    }
}
