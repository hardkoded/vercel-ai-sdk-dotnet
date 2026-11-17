// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ConvertToModelMessages.ConvertToModelMessagesSupport;

namespace Vercel.AI.Tests.ConvertToModelMessages;

/// <summary>Port of <c>convert-to-model-messages.test.ts</c> &gt; <c>convertToModelMessages > data part conversion > in user messages</c>.</summary>
[Collection("LogWarnings")]
public sealed class DataPartConversionInUserMessagesTests : WarningsOff
{
    private const string Suite =
        Prefix + "convertToModelMessages > data part conversion > in user messages::";

    [UpstreamTest(
        Suite + "should convert data parts to text when converter provided",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_data_parts_to_text_when_converter_provided()
    {
        var result = Convert(
            UiMessages(
                """
                [
                  {
                    "role": "user",
                    "parts": [
                      {
                        "type": "data-url",
                        "data": {
                          "url": "https://example.com",
                          "content": "Article text"
                        }
                      }
                    ]
                  }
                ]
                """
            ),
            new ConvertToModelMessagesOptions
            {
                ConvertDataPart = part => new TextContentPart(
                    $"\n\n[{part.Data.GetProperty("url").GetString()}]\n{part.Data.GetProperty("content").GetString()}"
                ),
            }
        );

        AssertEqual(
            """
            [
              {
                "role": "user",
                "content": [
                  {
                    "type": "text",
                    "text": "\n\n[https://example.com]\nArticle text"
                  }
                ]
              }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should skip data parts when no converter provided",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_skip_data_parts_when_no_converter_provided()
    {
        var result = Convert(
            UiMessages(
                """
                [
                  {
                    "role": "user",
                    "parts": [
                      {
                        "type": "text",
                        "text": "Hello"
                      },
                      {
                        "type": "data-url",
                        "data": {
                          "url": "https://example.com"
                        }
                      }
                    ]
                  }
                ]
                """
            )
        );

        AssertEqual(
            """
            [
              {
                "role": "user",
                "content": [
                  {
                    "type": "text",
                    "text": "Hello"
                  }
                ]
              }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should selectively convert data parts",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_selectively_convert_data_parts()
    {
        var result = Convert(
            UiMessages(
                """
                [
                  {
                    "role": "user",
                    "parts": [
                      {
                        "type": "data-url",
                        "data": {
                          "url": "https://example.com"
                        }
                      },
                      {
                        "type": "data-ui-state",
                        "data": {
                          "enabled": true
                        }
                      }
                    ]
                  }
                ]
                """
            ),
            new ConvertToModelMessagesOptions
            {
                ConvertDataPart = part =>
                    part.Name == "url"
                        ? new TextContentPart(part.Data.GetProperty("url").GetString()!)
                        : null,
            }
        );

        AssertEqual(
            """
            [
              {
                "role": "user",
                "content": [
                  {
                    "type": "text",
                    "text": "https://example.com"
                  }
                ]
              }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should convert data parts to file parts",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_convert_data_parts_to_file_parts()
    {
        var result = Convert(
            UiMessages(
                """
                [
                  {
                    "role": "user",
                    "parts": [
                      {
                        "type": "text",
                        "text": "Check this file"
                      },
                      {
                        "type": "data-attachment",
                        "data": {
                          "mediaType": "application/pdf",
                          "filename": "document.pdf",
                          "data": "base64data"
                        }
                      }
                    ]
                  }
                ]
                """
            ),
            new ConvertToModelMessagesOptions
            {
                ConvertDataPart = part =>
                    part.Name == "attachment"
                        ? new FileContentPart(
                            part.Data.GetProperty("mediaType").GetString()!,
                            null,
                            Encoding.UTF8.GetBytes(part.Data.GetProperty("data").GetString()!),
                            part.Data.GetProperty("filename").GetString()
                        )
                        : null,
            }
        );

        // The file data is a string upstream. Here it is its UTF-8 bytes, which the shared renderer does not show.
        var content = Assert.IsType<UserModelMessage>(Assert.Single(result)).Content;
        Assert.Equal(2, content.Count);
        Assert.Equal("Check this file", Assert.IsType<TextContentPart>(content[0]).Text);
        var file = Assert.IsType<FileContentPart>(content[1]);
        Assert.Equal("application/pdf", file.MediaType);
        Assert.Equal("document.pdf", file.FileName);
        Assert.Equal("base64data", Encoding.UTF8.GetString(file.Data!));
    }

    [UpstreamTest(
        Suite + "should handle multiple data parts of different types",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_handle_multiple_data_parts_of_different_types()
    {
        var result = Convert(
            UiMessages(
                """
                [
                  {
                    "role": "user",
                    "parts": [
                      {
                        "type": "text",
                        "text": "Review these:"
                      },
                      {
                        "type": "data-url",
                        "data": {
                          "url": "https://example.com",
                          "title": "Example"
                        }
                      },
                      {
                        "type": "data-code",
                        "data": {
                          "code": "console.log(\"test\")",
                          "language": "javascript"
                        }
                      },
                      {
                        "type": "data-note",
                        "data": {
                          "text": "Internal note"
                        }
                      }
                    ]
                  }
                ]
                """
            ),
            new ConvertToModelMessagesOptions
            {
                ConvertDataPart = part =>
                    part.Name switch
                    {
                        "url" => new TextContentPart(
                            $"[{part.Data.GetProperty("title").GetString()}]({part.Data.GetProperty("url").GetString()})"
                        ),
                        "code" => new TextContentPart(
                            $"```{part.Data.GetProperty("language").GetString()}\n{part.Data.GetProperty("code").GetString()}\n```"
                        ),
                        _ => null,
                    },
            }
        );

        AssertEqual(
            """
            [
              {
                "role": "user",
                "content": [
                  {
                    "type": "text",
                    "text": "Review these:"
                  },
                  {
                    "type": "text",
                    "text": "[Example](https://example.com)"
                  },
                  {
                    "type": "text",
                    "text": "```javascript\nconsole.log(\"test\")\n```"
                  }
                ]
              }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should work with messages that have no data parts",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_work_with_messages_that_have_no_data_parts()
    {
        var result = Convert(
            UiMessages(
                """
                [
                  {
                    "role": "user",
                    "parts": [
                      {
                        "type": "text",
                        "text": "Hello"
                      },
                      {
                        "type": "file",
                        "mediaType": "image/png",
                        "url": "https://example.com/image.png"
                      }
                    ]
                  }
                ]
                """
            ),
            new ConvertToModelMessagesOptions
            {
                ConvertDataPart = part => new TextContentPart("converted"),
            }
        );

        AssertEqual(
            """
            [
              {
                "role": "user",
                "content": [
                  {
                    "type": "text",
                    "text": "Hello"
                  },
                  {
                    "type": "file",
                    "mediaType": "image/png",
                    "data": {
                      "type": "url",
                      "url": "https://example.com/image.png"
                    }
                  }
                ]
              }
            ]
            """,
            result
        );
    }

    [UpstreamTest(
        Suite + "should preserve order of parts including converted data parts",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_preserve_order_of_parts_including_converted_data_parts()
    {
        var result = Convert(
            UiMessages(
                """
                [
                  {
                    "role": "user",
                    "parts": [
                      {
                        "type": "text",
                        "text": "First"
                      },
                      {
                        "type": "data-tag",
                        "data": {
                          "value": "tag1"
                        }
                      },
                      {
                        "type": "text",
                        "text": "Second"
                      },
                      {
                        "type": "data-tag",
                        "data": {
                          "value": "tag2"
                        }
                      },
                      {
                        "type": "text",
                        "text": "Third"
                      }
                    ]
                  }
                ]
                """
            ),
            new ConvertToModelMessagesOptions
            {
                ConvertDataPart = part => new TextContentPart(
                    $"[{part.Data.GetProperty("value").GetString()}]"
                ),
            }
        );

        AssertEqual(
            """
            [
              {
                "role": "user",
                "content": [
                  {
                    "type": "text",
                    "text": "First"
                  },
                  {
                    "type": "text",
                    "text": "[tag1]"
                  },
                  {
                    "type": "text",
                    "text": "Second"
                  },
                  {
                    "type": "text",
                    "text": "[tag2]"
                  },
                  {
                    "type": "text",
                    "text": "Third"
                  }
                ]
              }
            ]
            """,
            result
        );
    }
}
