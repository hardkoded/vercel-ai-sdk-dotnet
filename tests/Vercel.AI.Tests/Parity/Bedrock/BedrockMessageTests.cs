// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;

namespace Vercel.AI.Tests;

/// <summary>Prompt conversion into Converse system blocks and messages.</summary>
public sealed class BedrockMessageTests
{
    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::system messages::should combine multiple leading system messages into a single system message", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_leading_system_messages_as_system_blocks()
    {
        var converted = Convert(
            AmazonBedrockPromptMessage.System("Hello"),
            AmazonBedrockPromptMessage.System("World"));

        Assert.Equal(2, converted.System.Count);
        Assert.Equal("Hello", converted.System[0]!["text"]!.GetValue<string>());
        Assert.Equal("World", converted.System[1]!["text"]!.GetValue<string>());
        Assert.Empty(converted.Messages);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::system messages::should throw an error if a system message is provided after a non-system message", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_system_message_after_a_user_message()
    {
        var error = Assert.Throws<AmazonBedrockUnsupportedException>(() => Convert(
            AmazonBedrockPromptMessage.User(new AmazonBedrockPromptPart[] { new AmazonBedrockTextPart("Hello") }),
            AmazonBedrockPromptMessage.System("World")));

        Assert.Contains("Multiple system messages", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::system messages::should set isSystemCachePoint when system message has cache point", Coverage = UpstreamCoverage.Covered)]
    public void Appends_a_system_cache_point()
    {
        var system = AmazonBedrockPromptMessage.System("Hello", BedrockParity.ProviderOptions("{\"cachePoint\":{\"type\":\"default\"}}"));
        var converted = Convert(system);

        Assert.Equal("Hello", converted.System[0]!["text"]!.GetValue<string>());
        Assert.Equal("default", converted.System[1]!["cachePoint"]!["type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::system messages::should add cache point with 5m TTL to system message", Coverage = UpstreamCoverage.Covered)]
    public void Appends_a_five_minute_system_cache_point()
    {
        var converted = Convert(AmazonBedrockPromptMessage.System("Hello", BedrockParity.ProviderOptions("{\"cachePoint\":{\"type\":\"default\",\"ttl\":\"5m\"}}")));

        Assert.Equal("5m", converted.System[1]!["cachePoint"]!["ttl"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::system messages::should add cache point with 1h TTL to system message", Coverage = UpstreamCoverage.Covered)]
    public void Appends_a_one_hour_system_cache_point()
    {
        var converted = Convert(AmazonBedrockPromptMessage.System("Hello", BedrockParity.ProviderOptions("{\"cachePoint\":{\"type\":\"default\",\"ttl\":\"1h\"}}")));

        Assert.Equal("1h", converted.System[1]!["cachePoint"]!["ttl"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should convert messages with image parts", Coverage = UpstreamCoverage.Covered)]
    public void Converts_inline_image_bytes()
    {
        var image = new AmazonBedrockFilePart("image/jpeg") { Bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0x00 } };
        var converted = User(image);

        Assert.Equal("jpeg", converted[0]!["image"]!["format"]!.GetValue<string>());
        Assert.Equal(System.Convert.ToBase64String(image.Bytes!), converted[0]!["image"]!["source"]!["bytes"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should convert image parts with S3 URLs", Coverage = UpstreamCoverage.Covered)]
    public void Converts_an_s3_image_url()
    {
        var image = new AmazonBedrockFilePart("image/png") { Url = "s3://bucket/cat.png" };
        var converted = User(image);

        Assert.Equal("png", converted[0]!["image"]!["format"]!.GetValue<string>());
        Assert.Equal("s3://bucket/cat.png", converted[0]!["image"]!["source"]!["s3Location"]!["uri"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should convert messages with video parts", Coverage = UpstreamCoverage.Covered)]
    public void Converts_inline_video_bytes()
    {
        var video = new AmazonBedrockFilePart("video/mp4") { Bytes = new byte[] { 0, 0, 0, 0, (byte)'f', (byte)'t', (byte)'y', (byte)'p' } };
        var converted = User(video);

        Assert.Equal("mp4", converted[0]!["video"]!["format"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should convert video parts with S3 URLs", Coverage = UpstreamCoverage.Covered)]
    public void Converts_an_s3_video_url()
    {
        var video = new AmazonBedrockFilePart("video/webm") { Url = "s3://bucket/clip.webm" };
        var converted = User(video);

        Assert.Equal("webm", converted[0]!["video"]!["format"]!.GetValue<string>());
        Assert.Equal("s3://bucket/clip.webm", converted[0]!["video"]!["source"]!["s3Location"]!["uri"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should throw for unsupported video mime types", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_an_unsupported_video_mime_type()
    {
        var error = Assert.Throws<AmazonBedrockUnsupportedException>(() => User(new AmazonBedrockFilePart("video/avi") { Bytes = new byte[] { 1, 2, 3, 4 } }));

        Assert.Contains("video/avi", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should convert messages with document parts", Coverage = UpstreamCoverage.Covered)]
    public void Converts_a_pdf_document()
    {
        var document = new AmazonBedrockFilePart("application/pdf")
        {
            FileName = "notes.pdf",
            Bytes = Encoding.ASCII.GetBytes("%PDF-1.4"),
        };
        var converted = User(document);

        Assert.Equal("pdf", converted[0]!["document"]!["format"]!.GetValue<string>());
        Assert.Equal("notes", converted[0]!["document"]!["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should strip file extension when filename is provided", Coverage = UpstreamCoverage.Covered)]
    public void Strips_the_file_extension_at_the_first_dot()
    {
        var converted = User(TextDocument("report.final.pdf", "hello"));

        Assert.Equal("report", converted[0]!["document"]!["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should preserve filename without extension when provided", Coverage = UpstreamCoverage.Covered)]
    public void Preserves_a_filename_without_an_extension()
    {
        var converted = User(TextDocument("readme", "hello"));

        Assert.Equal("readme", converted[0]!["document"]!["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should sanitize document filenames to Bedrock-compatible names", Coverage = UpstreamCoverage.Covered)]
    public void Sanitizes_document_filenames()
    {
        var converted = User(new AmazonBedrockFilePart("text/plain")
        {
            FileName = "weird@name!.txt",
            Bytes = Encoding.UTF8.GetBytes("hello"),
        });

        Assert.Equal("weirdname", converted[0]!["document"]!["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should sanitize filenames for text document data", Coverage = UpstreamCoverage.Covered)]
    public void Sanitizes_text_document_filenames()
    {
        var converted = User(TextDocument("my file (1).txt", "body"));

        Assert.Equal("my file (1)", converted[0]!["document"]!["name"]!.GetValue<string>());
        Assert.Equal("txt", converted[0]!["document"]!["format"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should use consistent document names for prompt cache effectiveness", Coverage = UpstreamCoverage.Covered)]
    public void Reuses_the_same_sanitized_name_for_the_same_filename()
    {
        var converted = User(TextDocument("report.pdf", "one"), TextDocument("report.pdf", "two"));

        Assert.Equal("report", converted[0]!["document"]!["name"]!.GetValue<string>());
        Assert.Equal("report", converted[1]!["document"]!["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should extract the system message", Coverage = UpstreamCoverage.Covered)]
    public void Extracts_the_system_message_from_a_user_turn()
    {
        var converted = Convert(
            AmazonBedrockPromptMessage.System("System Prompt"),
            AmazonBedrockPromptMessage.User(new AmazonBedrockPromptPart[] { new AmazonBedrockTextPart("Hello") }));

        Assert.Equal("System Prompt", converted.System[0]!["text"]!.GetValue<string>());
        Assert.Equal("Hello", converted.Messages[0]!["content"]![0]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should add cache point to user message content when specified", Coverage = UpstreamCoverage.Covered)]
    public void Appends_a_cache_point_after_user_content()
    {
        var message = AmazonBedrockPromptMessage.User(
            new AmazonBedrockPromptPart[] { new AmazonBedrockTextPart("Hello") },
            BedrockParity.ProviderOptions("{\"cachePoint\":{\"type\":\"default\"}}"));
        var converted = Convert(message);

        Assert.Equal("default", converted.Messages[0]!["content"]![1]!["cachePoint"]!["type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should add cache point with 5m TTL to user message", Coverage = UpstreamCoverage.Covered)]
    public void Appends_a_five_minute_user_cache_point()
    {
        var message = AmazonBedrockPromptMessage.User(
            new AmazonBedrockPromptPart[] { new AmazonBedrockTextPart("Hello") },
            BedrockParity.ProviderOptions("{\"cachePoint\":{\"type\":\"default\",\"ttl\":\"5m\"}}"));

        Assert.Equal("5m", Convert(message).Messages[0]!["content"]![1]!["cachePoint"]!["ttl"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should add cache point with 1h TTL to user message", Coverage = UpstreamCoverage.Covered)]
    public void Appends_a_one_hour_user_cache_point()
    {
        var message = AmazonBedrockPromptMessage.User(
            new AmazonBedrockPromptPart[] { new AmazonBedrockTextPart("Hello") },
            BedrockParity.ProviderOptions("{\"cachePoint\":{\"type\":\"default\",\"ttl\":\"1h\"}}"));

        Assert.Equal("1h", Convert(message).Messages[0]!["content"]![1]!["cachePoint"]!["ttl"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should convert text part to guardContent when guardContent provider option is true", Coverage = UpstreamCoverage.Covered)]
    public void Converts_text_to_guard_content()
    {
        var text = new AmazonBedrockTextPart("secret")
        {
            ProviderOptions = BedrockParity.ProviderOptions("{\"guardContent\":true}"),
        };
        var converted = User(text);

        Assert.Equal("secret", converted[0]!["guardContent"]!["text"]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should convert text part to guardContent with qualifiers", Coverage = UpstreamCoverage.Covered)]
    public void Adds_a_guard_content_qualifier()
    {
        var text = new AmazonBedrockTextPart("secret")
        {
            ProviderOptions = BedrockParity.ProviderOptions("{\"guardContent\":true,\"guardContentQualifiers\":[\"grounding_source\"]}"),
        };
        var converted = User(text);

        Assert.Equal("grounding_source", converted[0]!["guardContent"]!["text"]!["qualifiers"]![0]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should convert text part to guardContent with multiple qualifiers", Coverage = UpstreamCoverage.Covered)]
    public void Adds_multiple_guard_content_qualifiers()
    {
        var text = new AmazonBedrockTextPart("secret")
        {
            ProviderOptions = BedrockParity.ProviderOptions("{\"guardContent\":true,\"guardContentQualifiers\":[\"grounding_source\",\"query\"]}"),
        };
        var converted = User(text);

        Assert.Equal(2, converted[0]!["guardContent"]!["text"]!["qualifiers"]!.AsArray().Count);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should convert text part as normal text when guardContent is false", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_text_alone_when_guard_content_is_false()
    {
        var text = new AmazonBedrockTextPart("hello")
        {
            ProviderOptions = BedrockParity.ProviderOptions("{\"guardContent\":false}"),
        };

        Assert.Equal("hello", User(text)[0]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should convert text part as normal text when no provider options", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_text_alone_without_provider_options()
    {
        Assert.Equal("hello", User(new AmazonBedrockTextPart("hello"))[0]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should convert image part to guardContent when guardContent provider option is true", Coverage = UpstreamCoverage.Covered)]
    public void Wraps_an_image_in_guard_content()
    {
        var image = new AmazonBedrockFilePart("image/png")
        {
            Bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 },
            ProviderOptions = BedrockParity.ProviderOptions("{\"guardContent\":true}"),
        };
        var converted = User(image);

        Assert.Equal("png", converted[0]!["guardContent"]!["image"]!["format"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should convert image part as normal image when guardContent is false", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_an_image_alone_when_guard_content_is_false()
    {
        var image = new AmazonBedrockFilePart("image/png")
        {
            Bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 },
            ProviderOptions = BedrockParity.ProviderOptions("{\"guardContent\":false}"),
        };

        Assert.Equal("png", User(image)[0]!["image"]!["format"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should convert image part as normal image when no provider options", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_an_image_alone_without_provider_options()
    {
        var image = new AmazonBedrockFilePart("image/gif") { Bytes = Encoding.ASCII.GetBytes("GIF89a") };

        Assert.Equal("gif", User(image)[0]!["image"]!["format"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should throw for file parts with provider references", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_provider_file_references()
    {
        var file = new AmazonBedrockFilePart("application/pdf") { IsReference = true };

        var error = Assert.Throws<AmazonBedrockUnsupportedException>(() => User(file));

        Assert.Contains("provider references", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::user messages::should add cache point to user content part when specified", Coverage = UpstreamCoverage.Covered)]
    public void Appends_a_cache_point_after_a_user_part()
    {
        var text = new AmazonBedrockTextPart("Hello")
        {
            ProviderOptions = BedrockParity.ProviderOptions("{\"cachePoint\":{\"type\":\"default\"}}"),
        };
        var converted = User(text);

        Assert.Equal("default", converted[1]!["cachePoint"]!["type"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should remove trailing whitespace from last assistant message when there is no further user message", Coverage = UpstreamCoverage.Covered)]
    public void Trims_the_last_assistant_text()
    {
        var converted = Convert(AmazonBedrockPromptMessage.Assistant(new AmazonBedrockPromptPart[] { new AmazonBedrockTextPart("hello  ") }));

        Assert.Equal("hello", converted.Messages[0]!["content"]![0]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should keep trailing whitespace from assistant message when there is a further user message", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_trailing_whitespace_when_a_user_message_follows()
    {
        var converted = Convert(
            AmazonBedrockPromptMessage.Assistant(new AmazonBedrockPromptPart[] { new AmazonBedrockTextPart("hello  ") }),
            AmazonBedrockPromptMessage.User(new AmazonBedrockPromptPart[] { new AmazonBedrockTextPart("next") }));

        Assert.Equal("hello  ", converted.Messages[0]!["content"]![0]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should combine multiple sequential assistant messages into a single message", Coverage = UpstreamCoverage.Covered)]
    public void Combines_sequential_assistant_messages()
    {
        var converted = Convert(
            AmazonBedrockPromptMessage.Assistant(new AmazonBedrockPromptPart[] { new AmazonBedrockTextPart("one") }),
            AmazonBedrockPromptMessage.Assistant(new AmazonBedrockPromptPart[] { new AmazonBedrockTextPart("two") }));

        Assert.Single(converted.Messages);
        Assert.Equal("one", converted.Messages[0]!["content"]![0]!["text"]!.GetValue<string>());
        Assert.Equal("two", converted.Messages[0]!["content"]![1]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should properly convert reasoning content type", Coverage = UpstreamCoverage.Covered)]
    public void Replays_signed_reasoning()
    {
        var reasoning = new AmazonBedrockReasoningPart("think")
        {
            ProviderOptions = BedrockParity.ProviderOptions("{\"signature\":\"sig\"}", "amazonBedrock"),
        };
        var converted = Convert(AmazonBedrockPromptMessage.Assistant(new AmazonBedrockPromptPart[] { reasoning }));

        Assert.Equal("think", converted.Messages[0]!["content"]![0]!["reasoningContent"]!["reasoningText"]!["text"]!.GetValue<string>());
        Assert.Equal("sig", converted.Messages[0]!["content"]![0]!["reasoningContent"]!["reasoningText"]!["signature"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should replay reasoning redacted as `redactedContent`", Coverage = UpstreamCoverage.Covered)]
    public void Replays_redacted_content()
    {
        var reasoning = new AmazonBedrockReasoningPart(string.Empty)
        {
            ProviderOptions = BedrockParity.ProviderOptions("{\"redactedContent\":\"opaque\"}"),
        };
        var converted = Convert(AmazonBedrockPromptMessage.Assistant(new AmazonBedrockPromptPart[] { reasoning }));

        Assert.Equal("opaque", converted.Messages[0]!["content"]![0]!["reasoningContent"]!["redactedContent"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should omit assistant message reasoning parts signed by a foreign provider", Coverage = UpstreamCoverage.Covered)]
    public void Omits_reasoning_signed_by_another_provider()
    {
        var reasoning = new AmazonBedrockReasoningPart("think")
        {
            ProviderOptions = BedrockParity.ProviderOptions("{\"signature\":\"sig\"}", "anthropic"),
        };
        var converted = Convert(AmazonBedrockPromptMessage.Assistant(new AmazonBedrockPromptPart[]
        {
            reasoning,
            new AmazonBedrockTextPart("answer"),
        }));

        Assert.Equal("answer", converted.Messages[0]!["content"]![0]!["text"]!.GetValue<string>());
        Assert.Single(converted.Messages[0]!["content"]!.AsArray());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should omit reasoning content without signature", Coverage = UpstreamCoverage.Covered)]
    public void Omits_unsigned_reasoning()
    {
        var converted = Convert(AmazonBedrockPromptMessage.Assistant(new AmazonBedrockPromptPart[]
        {
            new AmazonBedrockReasoningPart("think"),
            new AmazonBedrockTextPart("answer"),
        }));

        Assert.Equal("answer", converted.Messages[0]!["content"]![0]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should omit an assistant message when unsigned reasoning is its only content", Coverage = UpstreamCoverage.Covered)]
    public void Omits_an_assistant_message_that_is_only_unsigned_reasoning()
    {
        var converted = Convert(AmazonBedrockPromptMessage.Assistant(new AmazonBedrockPromptPart[] { new AmazonBedrockReasoningPart("think") }));

        Assert.Empty(converted.Messages);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should not trim reasoning text when a signature is present", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_trim_signed_reasoning_text()
    {
        var reasoning = new AmazonBedrockReasoningPart(" think ")
        {
            ProviderOptions = BedrockParity.ProviderOptions("{\"signature\":\"sig\"}"),
        };
        var converted = Convert(AmazonBedrockPromptMessage.Assistant(new AmazonBedrockPromptPart[] { reasoning }));

        Assert.Equal(" think ", converted.Messages[0]!["content"]![0]!["reasoningContent"]!["reasoningText"]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should filter out empty text blocks in assistant messages", Coverage = UpstreamCoverage.Covered)]
    public void Drops_empty_assistant_text()
    {
        var converted = Convert(AmazonBedrockPromptMessage.Assistant(new AmazonBedrockPromptPart[]
        {
            new AmazonBedrockTextPart("   "),
            new AmazonBedrockTextPart("kept"),
        }));

        Assert.Single(converted.Messages[0]!["content"]!.AsArray());
        Assert.Equal("kept", converted.Messages[0]!["content"]![0]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should wrap non-object (invalid) tool call input in an object", Coverage = UpstreamCoverage.Covered)]
    public void Wraps_non_object_tool_input()
    {
        var call = new AmazonBedrockToolCallPart("id", "lookup", JsonValue.Create("nope"));
        var converted = Convert(AmazonBedrockPromptMessage.Assistant(new AmazonBedrockPromptPart[] { call }));

        Assert.Equal("nope", converted.Messages[0]!["content"]![0]!["toolUse"]!["input"]!["rawInvalidInput"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should strip invalid characters from tool call names", Coverage = UpstreamCoverage.Covered)]
    public void Strips_invalid_tool_name_characters()
    {
        var call = new AmazonBedrockToolCallPart("id", "get weather!", new JsonObject());
        var converted = Convert(AmazonBedrockPromptMessage.Assistant(new AmazonBedrockPromptPart[] { call }));

        Assert.Equal("getweather", converted.Messages[0]!["content"]![0]!["toolUse"]!["name"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::Mistral tool call ID normalization::should normalize tool call IDs in tool calls when isMistral is true", Coverage = UpstreamCoverage.Covered)]
    public void Normalizes_mistral_tool_call_ids()
    {
        var call = new AmazonBedrockToolCallPart("tooluse_bpe71yCfRu2b5i-nKGDr5g", "lookup", new JsonObject());
        var converted = AmazonBedrockMessages.Convert(new[] { AmazonBedrockPromptMessage.Assistant(new AmazonBedrockPromptPart[] { call }) }, true);

        Assert.Equal("toolusebp", converted.Messages[0]!["content"]![0]!["toolUse"]!["toolUseId"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::Mistral tool call ID normalization::should not normalize tool call IDs when isMistral is false", Coverage = UpstreamCoverage.Covered)]
    public void Leaves_tool_call_ids_unchanged_for_other_models()
    {
        var call = new AmazonBedrockToolCallPart("tooluse_bpe71yCfRu2b5i-nKGDr5g", "lookup", new JsonObject());
        var converted = AmazonBedrockMessages.Convert(new[] { AmazonBedrockPromptMessage.Assistant(new AmazonBedrockPromptPart[] { call }) }, false);

        Assert.Equal("tooluse_bpe71yCfRu2b5i-nKGDr5g", converted.Messages[0]!["content"]![0]!["toolUse"]!["toolUseId"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::Mistral tool call ID normalization::should normalize tool call IDs in tool results when isMistral is true", Coverage = UpstreamCoverage.Covered)]
    public void Normalizes_mistral_tool_result_ids()
    {
        var result = new AmazonBedrockToolResultPart("tooluse_bpe71yCfRu2b5i-nKGDr5g", "lookup")
        {
            OutputType = "text",
            OutputValue = JsonValue.Create("ok"),
        };
        var converted = AmazonBedrockMessages.Convert(new[] { AmazonBedrockPromptMessage.Tool(new AmazonBedrockPromptPart[] { result }) }, true);

        Assert.Equal("toolusebp", converted.Messages[0]!["content"]![0]!["toolResult"]!["toolUseId"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::Mistral tool call ID normalization::should default to not normalizing when isMistral is not provided", Coverage = UpstreamCoverage.Covered)]
    public void Defaults_to_leaving_tool_ids_unchanged()
    {
        var call = new AmazonBedrockToolCallPart("tooluse_abc", "lookup", new JsonObject());
        var converted = AmazonBedrockMessages.Convert(new[] { AmazonBedrockPromptMessage.Assistant(new AmazonBedrockPromptPart[] { call }) });

        Assert.Equal("tooluse_abc", converted.Messages[0]!["content"]![0]!["toolUse"]!["toolUseId"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::citations::should handle citations enabled for PDF", Coverage = UpstreamCoverage.Covered)]
    public void Enables_citations_on_a_pdf()
    {
        var document = new AmazonBedrockFilePart("application/pdf")
        {
            Bytes = Encoding.ASCII.GetBytes("%PDF"),
            ProviderOptions = BedrockParity.ProviderOptions("{\"citations\":{\"enabled\":true}}"),
        };

        Assert.True(User(document)[0]!["document"]!["citations"]!["enabled"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::citations::should handle citations disabled for PDF", Coverage = UpstreamCoverage.Covered)]
    public void Omits_citations_when_disabled()
    {
        var document = new AmazonBedrockFilePart("application/pdf")
        {
            Bytes = Encoding.ASCII.GetBytes("%PDF"),
            ProviderOptions = BedrockParity.ProviderOptions("{\"citations\":{\"enabled\":false}}"),
        };

        Assert.Null(User(document)[0]!["document"]!["citations"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::additional file format tests::should throw an error for unsupported file mime type in user message content", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_an_unsupported_document_mime_type()
    {
        var error = Assert.Throws<AmazonBedrockUnsupportedException>(() => User(new AmazonBedrockFilePart("application/zip") { Bytes = new byte[] { 1, 2 } }));

        Assert.Contains("application/zip", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::additional file format tests::should handle xlsx files correctly", Coverage = UpstreamCoverage.Covered)]
    public void Converts_an_xlsx_document()
    {
        var file = new AmazonBedrockFilePart("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileName = "sheet.xlsx",
            Bytes = new byte[] { 1, 2, 3 },
        };

        Assert.Equal("xlsx", User(file)[0]!["document"]!["format"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::additional file format tests::should handle docx files correctly", Coverage = UpstreamCoverage.Covered)]
    public void Converts_a_docx_document()
    {
        var file = new AmazonBedrockFilePart("application/vnd.openxmlformats-officedocument.wordprocessingml.document")
        {
            FileName = "memo.docx",
            Bytes = new byte[] { 1, 2, 3 },
        };

        Assert.Equal("docx", User(file)[0]!["document"]!["format"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::top-level-only mediaType resolution::should pass through a full image mediaType unchanged", Coverage = UpstreamCoverage.Covered)]
    public void Passes_through_a_full_image_media_type()
    {
        var image = new AmazonBedrockFilePart("image/webp") { Bytes = new byte[] { 1, 2, 3, 4 } };

        Assert.Equal("webp", User(image)[0]!["image"]!["format"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::top-level-only mediaType resolution::should detect subtype from inline bytes when mediaType is top-level-only (image)", Coverage = UpstreamCoverage.Covered)]
    public void Detects_png_from_bytes_when_the_media_type_is_image()
    {
        var image = new AmazonBedrockFilePart("image") { Bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0, 0 } };

        Assert.Equal("png", User(image)[0]!["image"]!["format"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::top-level-only mediaType resolution::should detect subtype from inline bytes when mediaType is top-level-only (application/pdf)", Coverage = UpstreamCoverage.Covered)]
    public void Detects_pdf_bytes_when_the_media_type_is_application()
    {
        var file = new AmazonBedrockFilePart("application") { Bytes = Encoding.ASCII.GetBytes("%PDF-1.7"), FileName = "doc" };

        Assert.Equal("pdf", User(file)[0]!["document"]!["format"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::top-level-only mediaType resolution::should throw UnsupportedFunctionalityError for URL data (File URL)", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_non_s3_file_url()
    {
        var error = Assert.Throws<AmazonBedrockUnsupportedException>(() => User(new AmazonBedrockFilePart("image/png") { Url = "https://example.test/cat.png" }));

        Assert.Contains("File URL", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::top-level-only mediaType resolution::should throw UnsupportedFunctionalityError for unsupported full image mediaType", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_an_unsupported_image_media_type()
    {
        var error = Assert.Throws<AmazonBedrockUnsupportedException>(() => User(new AmazonBedrockFilePart("image/bmp") { Bytes = new byte[] { 1, 2, 3 } }));

        Assert.Contains("image/bmp", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/amazon-bedrock-chat-language-model.test.ts::supportedUrls::should support S3 URLs for image and video parts", Coverage = UpstreamCoverage.Covered)]
    public void Accepts_s3_urls_for_images_and_video()
    {
        var image = User(new AmazonBedrockFilePart("image/jpeg") { Url = "s3://bucket/a.jpg" });
        var video = User(new AmazonBedrockFilePart("video/mp4") { Url = "s3://bucket/a.mp4" });

        Assert.Equal("s3://bucket/a.jpg", image[0]!["image"]!["source"]!["s3Location"]!["uri"]!.GetValue<string>());
        Assert.Equal("s3://bucket/a.mp4", video[0]!["video"]!["source"]!["s3Location"]!["uri"]!.GetValue<string>());
    }

    private static AmazonBedrockMessageConversion Convert(params AmazonBedrockPromptMessage[] messages)
    {
        return AmazonBedrockMessages.Convert(messages);
    }

    private static JsonArray User(params AmazonBedrockPromptPart[] parts)
    {
        return Convert(AmazonBedrockPromptMessage.User(parts)).Messages[0]!["content"]!.AsArray();
    }

    private static AmazonBedrockFilePart TextDocument(string name, string text)
    {
        return new AmazonBedrockFilePart("text/plain")
        {
            FileName = name,
            Text = text,
        };
    }
}
