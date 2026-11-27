// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate</c>, the tests outside a nested <c>describe</c>.</summary>
public sealed partial class DoGenerateTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate::";

    private static LanguageModelCallOptions JsonResponse(string schema)
    {
        var options = AnthropicParity.Hello();
        options.JsonResponse = true;
        options.JsonSchema = AnthropicParity.Json(schema);
        return options;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use native structured output by default for claude-fable-5",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_native_structured_output_by_default_for_claude_fable_5()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-json-output-format.1");

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-fable-5",
            JsonResponse(
                "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"],\"additionalProperties\":false}"
            )
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        LanguageModelTestSupport.AssertJson(
            "{\"type\":\"json_schema\",\"schema\":{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}},\"required\":[\"name\"],\"additionalProperties\":false}}",
            requestBody["output_config"]!["format"]
        );
        Assert.Null(requestBody["tools"]);
        Assert.Null(requestBody["tool_choice"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should sanitize unsupported JSON schema keywords for output format",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_sanitize_unsupported_JSON_schema_keywords_for_output_format()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-json-output-format.1");

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-sonnet-4-5",
            JsonResponse(
                """
                {
                  "type": "object",
                  "properties": {
                    "recurringIntervalMinutes": { "type": "number", "exclusiveMinimum": 0 },
                    "tags": {
                      "type": "array",
                      "minItems": 2,
                      "maxItems": 4,
                      "items": { "type": "string", "minLength": 1 }
                    }
                  },
                  "required": ["recurringIntervalMinutes", "tags"],
                  "additionalProperties": false
                }
                """
            )
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        LanguageModelTestSupport.AssertJson(
            """
            {
              "additionalProperties": false,
              "properties": {
                "recurringIntervalMinutes": {
                  "description": "exclusive minimum: 0.",
                  "type": "number"
                },
                "tags": {
                  "description": "min items: 2; max items: 4.",
                  "items": { "description": "min length: 1.", "type": "string" },
                  "type": "array"
                }
              },
              "required": ["recurringIntervalMinutes", "tags"],
              "type": "object"
            }
            """,
            requestBody["output_config"]!["format"]!["schema"]
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass sanitized zod output schema as output_config.format",
        Coverage = UpstreamCoverage.Covered,
        Note = "The schema is the draft-07 JSON Schema that asSchema(z.object(...)) produces, because the port has no zod."
    )]
    public async Task Should_pass_sanitized_zod_output_schema_as_output_config_format()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-json-output-format.1");

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-sonnet-4-5",
            JsonResponse(
                """
                {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "type": "object",
                  "properties": {
                    "recipe": {
                      "type": "object",
                      "properties": {
                        "name": { "type": "string" },
                        "ingredients": {
                          "type": "array",
                          "items": {
                            "type": "object",
                            "properties": {
                              "name": { "type": "string" },
                              "amount": { "type": "string" }
                            },
                            "required": ["name", "amount"],
                            "additionalProperties": false
                          },
                          "minItems": 10,
                          "maxItems": 12
                        },
                        "steps": { "type": "array", "items": { "type": "string" } }
                      },
                      "required": ["name", "ingredients", "steps"],
                      "additionalProperties": false
                    }
                  },
                  "required": ["recipe"],
                  "additionalProperties": false
                }
                """
            )
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        LanguageModelTestSupport.AssertJson(
            """
            {
              "format": {
                "schema": {
                  "$schema": "http://json-schema.org/draft-07/schema#",
                  "additionalProperties": false,
                  "properties": {
                    "recipe": {
                      "additionalProperties": false,
                      "properties": {
                        "ingredients": {
                          "description": "min items: 10; max items: 12.",
                          "items": {
                            "additionalProperties": false,
                            "properties": {
                              "amount": { "type": "string" },
                              "name": { "type": "string" }
                            },
                            "required": ["name", "amount"],
                            "type": "object"
                          },
                          "type": "array"
                        },
                        "name": { "type": "string" },
                        "steps": { "items": { "type": "string" }, "type": "array" }
                      },
                      "required": ["name", "ingredients", "steps"],
                      "type": "object"
                    }
                  },
                  "required": ["recipe"],
                  "type": "object"
                },
                "type": "json_schema"
              }
            }
            """,
            requestBody["output_config"]
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit preserved thinking input transformations when absent",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_preserved_thinking_input_transformations_when_absent()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-fable-5-1",
            AnthropicParity.Hello()
        );

        var anthropic = result.ProviderMetadata!.Value.GetProperty("anthropic");
        Assert.False(anthropic.TryGetProperty("inputTransformations", out _));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should expose the raw response headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_expose_the_raw_response_headers()
    {
        var handler = AnthropicParity.Ok(
            "{\"id\":\"msg_017TfcQ4AgGxKyBduUpqYPZn\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"\"}],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":\"end_turn\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":4,\"output_tokens\":30}}"
        );
        handler.ResponseHeaders["test-header"] = "test-value";

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307"
        );

        // default headers, and the custom header
        Assert.Equal(
            new Dictionary<string, string>
            {
                ["content-length"] = "237",
                ["content-type"] = "application/json",
                ["test-header"] = "test-value",
            }.OrderBy(pair => pair.Key),
            result
                .ResponseHeaders.ToDictionary(
                    pair => pair.Key.ToLowerInvariant(),
                    pair => pair.Value.Split(';')[0]
                )
                .OrderBy(pair => pair.Key)
        );
        Assert.Contains("ai-sdk/anthropic/", handler.Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send the model id and settings",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_the_model_id_and_settings()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
        var options = AnthropicParity.Hello();
        options.Temperature = 0.5;
        options.MaxOutputTokens = 100;
        options.TopK = 1;
        options.StopSequences = new[] { "abc", "def" };
        options.FrequencyPenalty = 0.15;

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            options
        );

        LanguageModelTestSupport.AssertJson(
            """
            {
              "max_tokens": 100,
              "messages": [
                { "content": [{ "text": "Hello", "type": "text" }], "role": "user" }
              ],
              "model": "claude-3-haiku-20240307",
              "stop_sequences": ["abc", "def"],
              "temperature": 0.5,
              "top_k": 1
            }
            """,
            LanguageModelTestSupport.RequestBody(handler)
        );

        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("frequencyPenalty", warning.Feature);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use default thinking budget when it is not set",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_default_thinking_budget_when_it_is_not_set()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-haiku-4-5",
            AnthropicParity.WithProvider("{\"thinking\":{\"type\":\"enabled\"}}")
        );

        LanguageModelTestSupport.AssertJson(
            """
            {
              "max_tokens": 64000,
              "messages": [
                { "content": [{ "text": "Hello", "type": "text" }], "role": "user" }
              ],
              "model": "claude-haiku-4-5",
              "thinking": { "budget_tokens": 1024, "type": "enabled" }
            }
            """,
            LanguageModelTestSupport.RequestBody(handler)
        );

        var warning = Assert.Single(result.Warnings);
        LanguageModelTestSupport.AssertWarning(
            warning,
            "compatibility",
            "extended thinking",
            "thinking budget is required when thinking is enabled. using default budget of 1024 tokens."
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass disableParallelToolUse",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_disableParallelToolUse()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
        var options = AnthropicParity.WithProvider("{\"disableParallelToolUse\":true}");
        options.Tools = new[]
        {
            new LanguageModelTool(
                "test-tool",
                null,
                AnthropicParity.Json(
                    """
                    {
                      "type": "object",
                      "properties": { "value": { "type": "string" } },
                      "required": ["value"],
                      "additionalProperties": false,
                      "$schema": "http://json-schema.org/draft-07/schema#"
                    }
                    """
                )
            ),
        };

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            options
        );

        var requestBody = LanguageModelTestSupport.RequestBody(handler);
        Assert.Equal("auto", requestBody["tool_choice"]!["type"]!.GetValue<string>());
        Assert.True(requestBody["tool_choice"]!["disable_parallel_tool_use"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
        var providerOptions = new AnthropicOptions { ApiKey = "test-api-key" };
        providerOptions.Headers["Custom-Provider-Header"] = "provider-header-value";
        var options = AnthropicParity.Hello();
        options.Headers = new Dictionary<string, string?>
        {
            ["Custom-Request-Header"] = "request-header-value",
        };

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler, providerOptions),
            "claude-3-haiku-20240307",
            options
        );

        var headers = handler
            .Headers.Where(header =>
                !string.Equals(header.Key, "user-agent", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(header.Key, "content-length", StringComparison.OrdinalIgnoreCase)
            )
            .ToDictionary(
                header => header.Key.ToLowerInvariant(),
                header => header.Value.Split(';')[0]
            );
        Assert.Equal(
            new Dictionary<string, string>
            {
                ["anthropic-version"] = "2023-06-01",
                ["content-type"] = "application/json",
                ["custom-provider-header"] = "provider-header-value",
                ["custom-request-header"] = "request-header-value",
                ["x-api-key"] = "test-api-key",
            }.OrderBy(pair => pair.Key),
            headers.OrderBy(pair => pair.Key)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should support cache control and return extra fields in provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_support_cache_control_and_return_extra_fields_in_provider_metadata()
    {
        const string ResponseBody =
            "{\"id\":\"msg_017TfcQ4AgGxKyBduUpqYPZn\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"\"}],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":\"end_turn\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":20,\"output_tokens\":50,\"cache_creation_input_tokens\":10,\"cache_read_input_tokens\":5,\"cache_creation\":{\"ephemeral_5m_input_tokens\":0,\"ephemeral_1h_input_tokens\":10}}}";
        var handler = AnthropicParity.Ok(ResponseBody);
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[] { new TextContentPart("Hello") })
                {
                    ProviderOptions = LanguageModelTestSupport.Anthropic(
                        "{\"cacheControl\":{\"type\":\"ephemeral\",\"ttl\":\"1h\"}}"
                    ),
                },
            },
        };

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            options
        );

        const string ExpectedRequest = """
            {
              "model": "claude-3-haiku-20240307",
              "messages": [
                {
                  "role": "user",
                  "content": [
                    {
                      "type": "text",
                      "text": "Hello",
                      "cache_control": { "type": "ephemeral", "ttl": "1h" }
                    }
                  ]
                }
              ],
              "max_tokens": 4096
            }
            """;
        LanguageModelTestSupport.AssertJson(
            ExpectedRequest,
            LanguageModelTestSupport.RequestBody(handler)
        );

        LanguageModelTestSupport.AssertJson(
            "[{\"type\":\"text\",\"text\":\"\"}]",
            LanguageModelTestSupport.Snapshot(result.Content)
        );
        Assert.Equal("end_turn", result.RawFinishReason);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        JsonAssert.Equal(
            result.ProviderMetadata!.Value,
            """
            {
              "anthropic": {
                "container": null,
                "contextManagement": null,
                "iterations": null,
                "stopSequence": null,
                "usage": {
                  "cache_creation": {
                    "ephemeral_1h_input_tokens": 10,
                    "ephemeral_5m_input_tokens": 0
                  },
                  "cache_creation_input_tokens": 10,
                  "cache_read_input_tokens": 5,
                  "input_tokens": 20,
                  "output_tokens": 50
                }
              }
            }
            """
        );
        JsonAssert.Equal(result.Request!.Body!.Value, ExpectedRequest);
        JsonAssert.Equal(JsonDocument.Parse(result.RawResponse!).RootElement, ResponseBody);
        Assert.Equal(
            new Dictionary<string, string>
            {
                ["content-length"] = "379",
                ["content-type"] = "application/json",
            }.OrderBy(pair => pair.Key),
            result
                .ResponseHeaders.ToDictionary(
                    pair => pair.Key.ToLowerInvariant(),
                    pair => pair.Value.Split(';')[0]
                )
                .OrderBy(pair => pair.Key)
        );
        Assert.Equal("msg_017TfcQ4AgGxKyBduUpqYPZn", result.ResponseId);
        Assert.Equal("claude-3-haiku-20240307", result.ResponseModelId);
        Assert.Equal(35, result.Usage.InputTokens);
        Assert.Equal(5, result.Usage.CacheReadTokens);
        Assert.Equal(10, result.Usage.CacheWriteTokens);
        Assert.Equal(20, result.Usage.NoCacheInputTokens);
        Assert.Equal(50, result.Usage.OutputTokens);
        Assert.Null(result.Usage.ReasoningTokens);
        Assert.Null(result.Usage.TextTokens);
        JsonAssert.Equal(
            result.Usage.Raw!.Value,
            "{\"cache_creation\":{\"ephemeral_1h_input_tokens\":10,\"ephemeral_5m_input_tokens\":0},\"cache_creation_input_tokens\":10,\"cache_read_input_tokens\":5,\"input_tokens\":20,\"output_tokens\":50}"
        );
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_request_body()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-3-haiku-20240307",
            AnthropicParity.Hello()
        );

        JsonAssert.Equal(
            result.Request!.Body!.Value,
            """
            {
              "max_tokens": 4096,
              "messages": [
                { "content": [{ "text": "Hello", "type": "text" }], "role": "user" }
              ],
              "model": "claude-3-haiku-20240307"
            }
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should process PDF citation responses",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_process_PDF_citation_responses()
    {
        var handler = AnthropicParity.Ok(
            "{\"id\":\"msg_017TfcQ4AgGxKyBduUpqYPZn\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Based on the document, the results show positive growth.\",\"citations\":[{\"type\":\"page_location\",\"cited_text\":\"Revenue increased by 25% year over year\",\"document_index\":0,\"document_title\":\"Financial Report 2023\",\"start_page_number\":5,\"end_page_number\":6}]}],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":\"end_turn\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":4,\"output_tokens\":30}}"
        );
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(
                    new UserContentPart[]
                    {
                        new FileContentPart(
                            "application/pdf",
                            null,
                            new byte[] { 1, 2, 3 },
                            "financial-report.pdf"
                        )
                        {
                            ProviderOptions = LanguageModelTestSupport.Anthropic(
                                "{\"citations\":{\"enabled\":true}}"
                            ),
                        },
                        new TextContentPart("What do the results show?"),
                    }
                ),
            },
        };

        var result = await AnthropicParity.Generate(
            LanguageModelTestSupport.ProviderWithIds(handler),
            "claude-3-haiku-20240307",
            options
        );

        LanguageModelTestSupport.AssertJson(
            """
            [
              {
                "type": "text",
                "text": "Based on the document, the results show positive growth.",
                "citations": [
                  {
                    "citedText": "Revenue increased by 25% year over year",
                    "source": {
                      "type": "source",
                      "sourceType": "document",
                      "id": "0",
                      "mediaType": "application/pdf",
                      "title": "Financial Report 2023",
                      "filename": "financial-report.pdf",
                      "providerMetadata": {
                        "anthropic": {
                          "cited_text": "Revenue increased by 25% year over year",
                          "document_index": 0,
                          "document_title": "Financial Report 2023",
                          "end_page_number": 6,
                          "start_page_number": 5,
                          "type": "page_location"
                        }
                      }
                    }
                  }
                ],
                "providerMetadata": {
                  "anthropic": {
                    "citations": [
                      {
                        "cited_text": "Revenue increased by 25% year over year",
                        "document_index": 0,
                        "document_title": "Financial Report 2023",
                        "end_page_number": 6,
                        "start_page_number": 5,
                        "type": "page_location"
                      }
                    ]
                  }
                }
              },
              {
                "type": "source",
                "sourceType": "document",
                "id": "id-0",
                "mediaType": "application/pdf",
                "title": "Financial Report 2023",
                "filename": "financial-report.pdf",
                "providerMetadata": {
                  "anthropic": {
                    "citedText": "Revenue increased by 25% year over year",
                    "endPageNumber": 6,
                    "startPageNumber": 5
                  }
                }
              }
            ]
            """,
            LanguageModelTestSupport.Snapshot(result.Content)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should process text citation responses",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_process_text_citation_responses()
    {
        var handler = AnthropicParity.Ok(
            "{\"id\":\"msg_017TfcQ4AgGxKyBduUpqYPZn\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"The text shows important information.\",\"citations\":[{\"type\":\"char_location\",\"cited_text\":\"important information\",\"document_index\":0,\"document_title\":\"Test Document\",\"start_char_index\":15,\"end_char_index\":35}]}],\"model\":\"claude-3-haiku-20240307\",\"stop_reason\":\"end_turn\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":4,\"output_tokens\":30}}"
        );
        var provider = AnthropicParity.Client(
            handler,
            new AnthropicOptions
            {
                ApiKey = "test-api-key",
                GenerateId = () => "test-text-citation-id",
            }
        );
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(
                    new UserContentPart[]
                    {
                        new FileContentPart(
                            "text/plain",
                            null,
                            System.Convert.FromBase64String("VGVzdCBkb2N1bWVudCBjb250ZW50"),
                            "test.txt"
                        )
                        {
                            ProviderOptions = LanguageModelTestSupport.Anthropic(
                                "{\"citations\":{\"enabled\":true}}"
                            ),
                        },
                        new TextContentPart("What does this say?"),
                    }
                ),
            },
        };

        var result = await AnthropicParity.Generate(provider, "claude-3-haiku-20240307", options);

        LanguageModelTestSupport.AssertJson(
            """
            [
              {
                "type": "text",
                "text": "The text shows important information.",
                "citations": [
                  {
                    "citedText": "important information",
                    "source": {
                      "type": "source",
                      "sourceType": "document",
                      "id": "0",
                      "mediaType": "text/plain",
                      "title": "Test Document",
                      "filename": "test.txt",
                      "providerMetadata": {
                        "anthropic": {
                          "cited_text": "important information",
                          "document_index": 0,
                          "document_title": "Test Document",
                          "end_char_index": 35,
                          "start_char_index": 15,
                          "type": "char_location"
                        }
                      }
                    }
                  }
                ],
                "providerMetadata": {
                  "anthropic": {
                    "citations": [
                      {
                        "cited_text": "important information",
                        "document_index": 0,
                        "document_title": "Test Document",
                        "end_char_index": 35,
                        "start_char_index": 15,
                        "type": "char_location"
                      }
                    ]
                  }
                }
              },
              {
                "type": "source",
                "sourceType": "document",
                "id": "test-text-citation-id",
                "mediaType": "text/plain",
                "title": "Test Document",
                "filename": "test.txt",
                "providerMetadata": {
                  "anthropic": {
                    "citedText": "important information",
                    "endCharIndex": 35,
                    "startCharIndex": 15
                  }
                }
              }
            ]
            """,
            LanguageModelTestSupport.Snapshot(result.Content)
        );
    }
}
