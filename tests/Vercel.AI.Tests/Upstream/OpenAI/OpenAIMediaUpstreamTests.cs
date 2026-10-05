// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests;

/// <summary>Embeddings, images, speech, transcription, files, skills, completions, and realtime client secrets.</summary>
public sealed class OpenAIMediaUpstreamTests
{
    private const string EmbeddingFixtureJson = "{\"object\":\"list\",\"data\":[{\"object\":\"embedding\",\"index\":0,\"embedding\":[0.0057293195,-0.012727811,0.020042092,-0.013437585,0.022833068]},{\"object\":\"embedding\",\"index\":1,\"embedding\":[-0.037104916,-0.05178114,-0.008340587,0.001164541,-0.0035253682]}],\"model\":\"text-embedding-3-small\",\"usage\":{\"prompt_tokens\":12,\"total_tokens\":12}}";

    private const string SkillCreateJson = "{\"id\":\"skill_699fc58f408c8191825d8d06ae75fd5c06de7b381a5db7f5\",\"object\":\"skill\",\"name\":\"test-capture-skill\",\"description\":\"A test skill for fixture capture\",\"default_version\":\"1\",\"latest_version\":\"1\",\"created_at\":1772078479}";

    private static readonly string[] EmbeddingValues = { "sunny day at the beach", "rainy day in the city" };

    [Fact]
    [UpstreamTest("packages/openai/src/embedding/openai-embedding-model.test.ts::model limits::should expose the aggregate token limit", Coverage = UpstreamCoverage.Covered)]
    public void ExposesEmbeddingByteLimit()
    {
        Assert.Equal(300000, new OpenAIEmbeddingModel(OpenAIUpstream.Provider(new OpenAICapture()), "text-embedding-3-large").MaxInputBytesPerCall);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/embedding/openai-embedding-model.test.ts::doEmbed::should extract embedding", Coverage = UpstreamCoverage.Covered)]
    public async Task ExtractsEmbeddings()
    {
        const string json = "{\"data\":[{\"embedding\":[0.0057293195,-0.012727811,0.020042092,-0.013437585,0.022833068]},{\"embedding\":[-0.037104916,-0.05178114,-0.008340587,0.001164541,-0.0035253682]}],\"usage\":{\"prompt_tokens\":12}}";
        var result = await Embed(json, "text-embedding-3-large", new[] { "sunny day at the beach", "rainy day in the city" });
        Assert.Equal(2, result.Result.Embeddings.Count);
        var root = JsonNode.Parse(json)!;
        Assert.Equal(root["data"]![0]!["embedding"]![0]!.GetValue<float>(), result.Result.Embeddings[0][0]);
        Assert.Equal(root["data"]![1]!["embedding"]![4]!.GetValue<float>(), result.Result.Embeddings[1][4]);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/embedding/openai-embedding-model.test.ts::doEmbed::should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task ExtractsEmbeddingUsage()
    {
        var result = await Embed("{\"data\":[{\"embedding\":[0.5]}],\"usage\":{\"prompt_tokens\":12}}", "text-embedding-3-large", new[] { "sunny day at the beach" });
        Assert.Equal(12, result.Result.Tokens);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/embedding/openai-embedding-model.test.ts::doEmbed::should pass the model and the values", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesEmbeddingModelAndValues()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"data\":[{\"embedding\":[0.5]},{\"embedding\":[0.25]}],\"usage\":{\"prompt_tokens\":12}}" };
        await new OpenAIEmbeddingModel(OpenAIUpstream.Provider(capture), "text-embedding-3-large").EmbedAsync(new[] { "sunny day at the beach", "rainy day in the city" }, null, null, null, CancellationToken.None);
        OpenAIUpstream.Equal(JsonNode.Parse(capture.Body), "{\"model\":\"text-embedding-3-large\",\"input\":[\"sunny day at the beach\",\"rainy day in the city\"],\"encoding_format\":\"float\"}");
        Assert.EndsWith("/embeddings", capture.Uri, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/embedding/openai-embedding-model.test.ts::doEmbed::should pass the dimensions setting", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesEmbeddingDimensions()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"data\":[{\"embedding\":[0.5]}],\"usage\":{\"prompt_tokens\":1}}" };
        await new OpenAIEmbeddingModel(OpenAIUpstream.Provider(capture), "text-embedding-3-large").EmbedAsync(new[] { "hello" }, 64, "user-1", null, CancellationToken.None);
        var body = JsonNode.Parse(capture.Body)!;
        Assert.Equal(64, body["dimensions"]!.GetValue<int>());
        Assert.Equal("user-1", body["user"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai/src/image/openai-image-model.test.ts::doGenerate > %s quality::should pass %s quality", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesGptImageQualities()
    {
        foreach (var modelId in new[] { "gpt-image-2.5-flare", "gpt-image-2.5-sunburst" })
        {
            foreach (var quality in new[] { "xhigh", "max" })
            {
                var capture = await Image(modelId, new OpenAIImageCall("A cute baby sea otter")
                {
                    Count = 1,
                    Size = "1024x1024",
                    ProviderOptions = OpenAIUpstream.Json("{\"openai\":{\"quality\":\"" + quality + "\"}}"),
                });
                OpenAIUpstream.Equal(JsonNode.Parse(capture.Body), "{\"model\":\"" + modelId + "\",\"prompt\":\"A cute baby sea otter\",\"n\":1,\"size\":\"1024x1024\",\"quality\":\"" + quality + "\"}");
            }
        }
    }

    [Fact]
    [UpstreamTest("packages/openai/src/image/openai-image-model.test.ts::doGenerate::should pass the model and the settings", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesDallESettings()
    {
        var capture = await Image("dall-e-3", new OpenAIImageCall("A cute baby sea otter")
        {
            Count = 1,
            Size = "1024x1024",
            ProviderOptions = OpenAIUpstream.Json("{\"openai\":{\"style\":\"vivid\"}}"),
        });
        OpenAIUpstream.Equal(JsonNode.Parse(capture.Body), "{\"model\":\"dall-e-3\",\"prompt\":\"A cute baby sea otter\",\"n\":1,\"size\":\"1024x1024\",\"style\":\"vivid\",\"response_format\":\"b64_json\"}");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/image/openai-image-model.test.ts::doGenerate::should map provider options to snake_case for /images/generations", Coverage = UpstreamCoverage.Covered)]
    public async Task MapsImageProviderOptions()
    {
        var capture = await Image("gpt-image-1", new OpenAIImageCall("A cute baby sea otter")
        {
            Count = 1,
            Size = "1024x1024",
            ProviderOptions = OpenAIUpstream.Json("{\"quality\":\"high\",\"background\":\"transparent\",\"moderation\":\"low\",\"outputFormat\":\"webp\",\"outputCompression\":80,\"user\":\"user-123\"}"),
        });
        var body = JsonNode.Parse(capture.Body)!;
        Assert.Equal("high", body["quality"]!.GetValue<string>());
        Assert.Equal("transparent", body["background"]!.GetValue<string>());
        Assert.Equal("low", body["moderation"]!.GetValue<string>());
        Assert.Equal("webp", body["output_format"]!.GetValue<string>());
        Assert.Equal(80, body["output_compression"]!.GetValue<int>());
        Assert.Equal("user-123", body["user"]!.GetValue<string>());
        Assert.Null(body["response_format"]);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/image/openai-image-model.test.ts::doGenerate::should extract the generated images", Coverage = UpstreamCoverage.Covered)]
    public async Task ExtractsGeneratedImages()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"data\":[{\"b64_json\":\"AQID\"},{\"b64_json\":\"BAUG\"}]}" };
        var result = await new OpenAIImageModel(OpenAIUpstream.Provider(capture), "dall-e-3").GenerateAsync(new OpenAIImageCall("A cute baby sea otter"), CancellationToken.None);
        Assert.Equal(new byte[] { 1, 2, 3 }, result.Images[0].Data!);
        Assert.Equal(new byte[] { 4, 5, 6 }, result.Images[1].Data!);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/image/openai-image-model.test.ts::doGenerate::should return warnings for unsupported settings", Coverage = UpstreamCoverage.Covered)]
    public async Task WarnsForImageAspectRatioAndSeed()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"data\":[{\"b64_json\":\"AQID\"}]}" };
        var result = await new OpenAIImageModel(OpenAIUpstream.Provider(capture), "dall-e-3").GenerateAsync(new OpenAIImageCall("A cute baby sea otter")
        {
            Size = "1024x1024",
            AspectRatio = "1:1",
            Seed = 123,
        }, CancellationToken.None);
        Assert.Equal("aspectRatio", result.Warnings[0].Feature);
        Assert.Equal("This model does not support aspect ratio. Use `size` instead.", result.Warnings[0].Details);
        Assert.Equal("seed", result.Warnings[1].Feature);
        Assert.Null(result.Warnings[1].Details);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/image/openai-image-model.test.ts::doGenerate::should respect maxImagesPerCall setting", Coverage = UpstreamCoverage.Covered)]
    public void RespectsMaxImages()
    {
        var provider = OpenAIUpstream.Provider(new OpenAICapture());
        Assert.Equal(10, new OpenAIImageModel(provider, "dall-e-2").MaxImagesPerCall);
        Assert.Equal(10, new OpenAIImageModel(provider, "gpt-image-99").MaxImagesPerCall);
        Assert.Equal(1, new OpenAIImageModel(provider, "unknown-model").MaxImagesPerCall);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/image/openai-image-model.test.ts::doGenerate::should not include response_format for gpt-image-1", Coverage = UpstreamCoverage.Covered)]
    public async Task OmitsResponseFormatForGptImage1()
    {
        await AssertNoResponseFormat("gpt-image-1");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/image/openai-image-model.test.ts::doGenerate::should not include response_format for gpt-image-2", Coverage = UpstreamCoverage.Covered)]
    public async Task OmitsResponseFormatForGptImage2()
    {
        await AssertNoResponseFormat("gpt-image-2");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/image/openai-image-model.test.ts::doGenerate::should not include response_format for chatgpt-image-latest", Coverage = UpstreamCoverage.Covered)]
    public async Task OmitsResponseFormatForChatgptImage()
    {
        await AssertNoResponseFormat("chatgpt-image-latest");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/image/openai-image-model.test.ts::doGenerate::should include response_format for dall-e-3", Coverage = UpstreamCoverage.Covered)]
    public async Task IncludesResponseFormatForDallE3()
    {
        var capture = await Image("dall-e-3", new OpenAIImageCall("A cute baby sea otter") { Count = 1, Size = "1024x1024" });
        Assert.Equal("b64_json", JsonNode.Parse(capture.Body)!["response_format"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai/src/image/openai-image-model.test.ts::doGenerate::should distribute input token details evenly across images", Coverage = UpstreamCoverage.Covered)]
    public async Task DistributesImageTokens()
    {
        var capture = new OpenAICapture
        {
            ResponseJson = "{\"data\":[{\"b64_json\":\"AQID\"},{\"b64_json\":\"AQID\"},{\"b64_json\":\"AQID\"}],\"usage\":{\"input_tokens\":222,\"output_tokens\":10,\"total_tokens\":232,\"input_tokens_details\":{\"image_tokens\":194,\"text_tokens\":28}}}",
        };
        var result = await new OpenAIImageModel(OpenAIUpstream.Provider(capture), "gpt-image-1").GenerateAsync(new OpenAIImageCall("otter") { Count = 3 }, CancellationToken.None);
        var images = result.ProviderMetadata.GetProperty("openai").GetProperty("images");
        Assert.Equal(64, images[0].GetProperty("imageTokens").GetInt32());
        Assert.Equal(64, images[1].GetProperty("imageTokens").GetInt32());
        Assert.Equal(66, images[2].GetProperty("imageTokens").GetInt32());
        Assert.Equal(9, images[0].GetProperty("textTokens").GetInt32());
        Assert.Equal(9, images[1].GetProperty("textTokens").GetInt32());
        Assert.Equal(10, images[2].GetProperty("textTokens").GetInt32());
    }

    [Fact]
    [UpstreamTest("packages/openai/src/image/openai-image-model.test.ts::doGenerate - image editing::should call /images/edits endpoint when files are provided", Coverage = UpstreamCoverage.Covered)]
    public async Task CallsImageEdits()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"data\":[{\"b64_json\":\"AQID\"}]}" };
        await new OpenAIImageModel(OpenAIUpstream.Provider(capture), "gpt-image-1").GenerateAsync(new OpenAIImageCall("edit")
        {
            Files = new[] { new OpenAIImageFile(new byte[] { 1, 2, 3 }, "image/png", "otter.png") },
        }, CancellationToken.None);
        Assert.Contains("/images/edits", capture.Uri, StringComparison.Ordinal);
        Assert.Contains(capture.Parts, part => part.Name == "image" && part.FileName == "otter.png");
        Assert.Contains(capture.Parts, part => part.Name == "prompt" && Encoding.UTF8.GetString(part.Data) == "edit");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/speech/openai-speech-model.test.ts::doGenerate::should pass the model and text", Coverage = UpstreamCoverage.Covered)]
    public void PassesSpeechModelAndText()
    {
        var prepared = OpenAISpeechModel.Prepare("tts-1", new OpenAISpeechCall("Hello from the AI SDK!"));
        OpenAIUpstream.Equal(prepared.Body, "{\"model\":\"tts-1\",\"input\":\"Hello from the AI SDK!\",\"voice\":\"alloy\",\"response_format\":\"mp3\"}");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/speech/openai-speech-model.test.ts::doGenerate::should pass options", Coverage = UpstreamCoverage.Covered)]
    public void PassesSpeechOptions()
    {
        var prepared = OpenAISpeechModel.Prepare("tts-1", new OpenAISpeechCall("Hello from the AI SDK!")
        {
            Voice = "nova",
            Speed = 1.5,
            OutputFormat = "opus",
        });
        Assert.Equal("nova", prepared.Body["voice"]!.GetValue<string>());
        Assert.Equal(1.5, prepared.Body["speed"]!.GetValue<double>());
        Assert.Equal("opus", prepared.Body["response_format"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai/src/speech/openai-speech-model.test.ts::doGenerate::should preserve top-level options that provider options do not override", Coverage = UpstreamCoverage.Covered)]
    public void PreservesSpeechSpeed()
    {
        var prepared = OpenAISpeechModel.Prepare("tts-1", new OpenAISpeechCall("Hello")
        {
            Speed = 1.5,
            ProviderOptions = OpenAIUpstream.Json("{\"instructions\":\"speak slowly\"}"),
        });
        Assert.Equal(1.5, prepared.Body["speed"]!.GetValue<double>());
        Assert.Equal("speak slowly", prepared.Body["instructions"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai/src/speech/openai-speech-model.test.ts::doGenerate::should return audio data with correct content type", Coverage = UpstreamCoverage.Covered)]
    public async Task ReturnsSpeechAudio()
    {
        var audio = new byte[100];
        var capture = new OpenAICapture { ResponseBytes = audio, ResponseMediaType = "audio/mpeg" };
        var result = await new OpenAISpeechModel(OpenAIUpstream.Provider(capture), "tts-1").GenerateAsync(new OpenAISpeechCall("Hello"), CancellationToken.None);
        Assert.Equal(audio, result.Audio);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/speech/openai-speech-model.test.ts::doGenerate::should include warnings if any are generated", Coverage = UpstreamCoverage.Covered)]
    public async Task IncludesEmptySpeechWarnings()
    {
        var capture = new OpenAICapture { ResponseBytes = new byte[] { 1 } };
        var result = await new OpenAISpeechModel(OpenAIUpstream.Provider(capture), "tts-1").GenerateAsync(new OpenAISpeechCall("Hello"), CancellationToken.None);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/transcription/openai-transcription-model.test.ts::doGenerate::should reject gpt-realtime-whisper for non-streaming transcription", Coverage = UpstreamCoverage.Covered)]
    public async Task RejectsRealtimeWhisper()
    {
        var exception = await Assert.ThrowsAsync<AiSdkException>(() =>
            new OpenAITranscriptionModel(OpenAIUpstream.Provider(new OpenAICapture()), "gpt-realtime-whisper").TranscribeAsync(new OpenAITranscriptionCall(new AudioInput(new byte[] { 1 }, "audio/wav", "a.wav")), CancellationToken.None));
        Assert.Equal("gpt-realtime-whisper requires a streaming transcription.", exception.Message);
        Assert.True(OpenAITranscriptionModel.IsRealtimeWhisper("gpt-realtime-whisper-2025-01-01"));
    }

    [Fact]
    [UpstreamTest("packages/openai/src/transcription/openai-transcription-model.test.ts::doGenerate::should default whisper-1 to verbose_json response format", Coverage = UpstreamCoverage.Covered)]
    public async Task DefaultsWhisperVerboseJson()
    {
        var fields = await TranscriptionFields("whisper-1", null);
        Assert.Equal("whisper-1", fields["model"][0]);
        Assert.Equal("verbose_json", fields["response_format"][0]);
        Assert.False(fields.ContainsKey("temperature"));
    }

    [Fact]
    [UpstreamTest("packages/openai/src/transcription/openai-transcription-model.test.ts::doGenerate::should pass response_format when `providerOptions.openai.timestampGranularities` is set", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesWhisperTimestampGranularity()
    {
        var fields = await TranscriptionFields("whisper-1", "{\"timestampGranularities\":[\"word\"]}");
        Assert.Equal("verbose_json", fields["response_format"][0]);
        Assert.Equal("0", fields["temperature"][0]);
        Assert.Equal("word", fields["timestamp_granularities[]"][0]);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/transcription/openai-transcription-model.test.ts::doGenerate::should not set pass response_format to \"verbose_json\" when model is \"gpt-4o-transcribe\"", Coverage = UpstreamCoverage.Covered)]
    public async Task UsesJsonForGpt4oTranscribe()
    {
        var fields = await TranscriptionFields("gpt-4o-transcribe", "{\"timestampGranularities\":[\"word\"]}");
        Assert.Equal("json", fields["response_format"][0]);
        Assert.Equal("0", fields["temperature"][0]);
        Assert.Equal("word", fields["timestamp_granularities[]"][0]);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/transcription/openai-transcription-model.test.ts::doGenerate::should support gpt-4o-transcribe-diarize", Coverage = UpstreamCoverage.Partial, Note = "The request sends diarized_json and chunking_strategy auto. Duration and speaker segments are not parsed.")]
    public async Task SupportsDiarizeRequest()
    {
        var fields = await TranscriptionFields("gpt-4o-transcribe-diarize", null);
        Assert.Equal("diarized_json", fields["response_format"][0]);
        Assert.Equal("auto", fields["chunking_strategy"][0]);
        Assert.False(fields.ContainsKey("temperature"));
    }

    [Fact]
    [UpstreamTest("packages/openai/src/transcription/openai-transcription-model.test.ts::doGenerate::should pass diarization response format and chunking strategy provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesDiarizeOptions()
    {
        var fields = await TranscriptionFields("gpt-4o-transcribe-diarize", "{\"responseFormat\":\"json\",\"chunkingStrategy\":\"auto\"}");
        Assert.Equal("json", fields["response_format"][0]);
        Assert.Equal("auto", fields["chunking_strategy"][0]);
        Assert.Equal("0", fields["temperature"][0]);
        Assert.Equal("segment", fields["timestamp_granularities[]"][0]);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/transcription/openai-transcription-model.test.ts::doGenerate::should serialize a server VAD chunking strategy", Coverage = UpstreamCoverage.Covered)]
    public async Task SerializesServerVad()
    {
        var fields = await TranscriptionFields("gpt-4o-transcribe-diarize", "{\"chunkingStrategy\":{\"type\":\"server_vad\",\"threshold\":0.7,\"prefixPaddingMs\":400,\"silenceDurationMs\":300}}");
        Assert.Equal("{\"type\":\"server_vad\",\"threshold\":0.7,\"prefix_padding_ms\":400,\"silence_duration_ms\":300}", fields["chunking_strategy"][0]);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/transcription/openai-transcription-model.test.ts::doGenerate::should extract the transcription text", Coverage = UpstreamCoverage.Covered)]
    public async Task ExtractsTranscriptionText()
    {
        var capture = new OpenAICapture { ResponseBytes = System.Text.Encoding.UTF8.GetBytes("{\"text\":\"Hello\"}"), ResponseMediaType = "application/json" };
        var result = await new OpenAITranscriptionModel(OpenAIUpstream.Provider(capture), "whisper-1").TranscribeAsync(new OpenAITranscriptionCall(new AudioInput(new byte[] { 1 }, "audio/wav", "a.wav")), CancellationToken.None);
        Assert.Equal("Hello", result.Text);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/files/openai-files.test.ts::OpenAI Files - uploadFile::should default purpose to assistants when not provided", Coverage = UpstreamCoverage.Covered)]
    public async Task DefaultsFilePurpose()
    {
        var capture = await Upload(null, null);
        Assert.Equal("assistants", Encoding.UTF8.GetString(Assert.Single(capture.Parts, part => part.Name == "purpose").Data));
        Assert.DoesNotContain("expires_after", capture.Body, StringComparison.Ordinal);
        Assert.EndsWith("/files", capture.Uri, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/files/openai-files.test.ts::OpenAI Files - uploadFile::should pass expires_after as bracketed multipart fields", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesFileExpiry()
    {
        var capture = await Upload("assistants", 3600);
        Assert.Contains("name=\"expires_after[anchor]\"", capture.Body, StringComparison.Ordinal);
        Assert.Contains("created_at", capture.Body, StringComparison.Ordinal);
        Assert.Contains("name=\"expires_after[seconds]\"", capture.Body, StringComparison.Ordinal);
        Assert.Contains("3600", capture.Body, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/files/openai-files.test.ts::OpenAI Files - uploadFile::should set specificationVersion and provider", Coverage = UpstreamCoverage.Covered)]
    public void SetsFileProviderIdentity()
    {
        var store = new OpenAIFileStore(OpenAIUpstream.Provider(new OpenAICapture()));
        Assert.Equal("openai.files", store.Provider);
        Assert.Equal("v4", store.SpecificationVersion);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/files/openai-files.test.ts::OpenAI Files - uploadFile::should return providerReference with openai key", Coverage = UpstreamCoverage.Covered)]
    public async Task ReturnsUploadedFileId()
    {
        var capture = new OpenAICapture { ResponseBytes = System.Text.Encoding.UTF8.GetBytes("{\"id\":\"file-abc\",\"filename\":\"notes.txt\",\"purpose\":\"assistants\"}"), ResponseMediaType = "application/json" };
        var file = await new OpenAIFileStore(OpenAIUpstream.Provider(capture)).UploadFileAsync("notes.txt", new byte[] { 1 }, "text/plain", CancellationToken.None);
        Assert.Equal("file-abc", file.Id);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/files/openai-files.test.ts::OpenAI Files - getFileMetadata::should reject a blank openai file id (%j)", Coverage = UpstreamCoverage.Covered)]
    public void RejectsBlankFileIds()
    {
        foreach (var id in new[] { string.Empty, "   " })
        {
            var exception = Assert.Throws<ArgumentException>(() => OpenAIFileStore.RequireId(new OpenAIFileReference(id)));
            Assert.StartsWith("file reference is missing an 'openai' file id.", exception.Message, StringComparison.Ordinal);
            Assert.Equal("file", exception.ParamName);
        }
    }

    [Fact]
    [UpstreamTest("packages/openai/src/files/openai-files.test.ts::OpenAI Files - getFileMetadata::should reject a reference without an openai file id", Coverage = UpstreamCoverage.Covered)]
    public void RejectsMissingFileId()
    {
        var exception = Assert.Throws<ArgumentException>(() => OpenAIFileStore.RequireId(new OpenAIFileReference(null)));
        Assert.Equal("file", exception.ParamName);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/files/openai-files.test.ts::OpenAI Files - getFileMetadata::should preserve dot-segment file id $fileId as a URL path segment", Coverage = UpstreamCoverage.Covered)]
    public async Task PreservesDotSegmentFileIds()
    {
        Assert.Equal("%252E", OpenAIJson.EncodePathSegment("."));
        Assert.Equal("%252E%252E", OpenAIJson.EncodePathSegment(".."));
        foreach (var pair in new[] { (".", "%252E"), ("..", "%252E%252E") })
        {
            var capture = new OpenAICapture { ResponseJson = "{\"id\":\"file\"}" };
            await new OpenAIFileStore(OpenAIUpstream.Provider(capture)).GetMetadataAsync(new OpenAIFileReference(pair.Item1), CancellationToken.None);
            Assert.Contains("/files/" + pair.Item2, capture.Uri, StringComparison.Ordinal);
        }
    }

    [Fact]
    [UpstreamTest("packages/openai/src/files/openai-files.test.ts::OpenAI Files - getFileMetadata::should omit expiresAt when the provider reports none", Coverage = UpstreamCoverage.Covered)]
    public async Task OmitsMissingExpiry()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"id\":\"file-abc\",\"filename\":\"notes.txt\",\"bytes\":4,\"created_at\":1700000000,\"status\":\"processed\",\"expires_at\":0}" };
        var metadata = await new OpenAIFileStore(OpenAIUpstream.Provider(capture)).GetMetadataAsync(new OpenAIFileReference("file-abc"), CancellationToken.None);
        Assert.Null(metadata.ExpiresAt);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1700000000), metadata.CreatedAt);
        Assert.Equal(4, metadata.Bytes);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/files/openai-files.test.ts::OpenAI Files - downloadFile::should expose the response content type as mediaType (parameters stripped)", Coverage = UpstreamCoverage.Covered)]
    public async Task StripsContentTypeParameters()
    {
        var capture = new OpenAICapture { ResponseBytes = new byte[] { 9 }, ResponseMediaType = "application/octet-stream; charset=utf-8" };
        var download = await new OpenAIFileStore(OpenAIUpstream.Provider(capture)).DownloadAsync(new OpenAIFileReference("file-abc"), CancellationToken.None);
        Assert.Equal("application/octet-stream", download.MediaType);
        Assert.Equal(new byte[] { 9 }, download.Data!);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/files/openai-files.test.ts::OpenAI Files - deleteFile::should delete a file via DELETE", Coverage = UpstreamCoverage.Covered)]
    public async Task DeletesFile()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"id\":\"file-abc\",\"deleted\":true}" };
        await new OpenAIFileStore(OpenAIUpstream.Provider(capture)).DeleteAsync(new OpenAIFileReference("file-abc"), CancellationToken.None);
        Assert.Equal("DELETE", capture.Method);
        Assert.EndsWith("/files/file-abc", capture.Uri, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/files/openai-files.test.ts::OpenAI Files - uploadFile::should send correct multipart request with purpose", Coverage = UpstreamCoverage.Covered)]
    public async Task SendsFileMultipart()
    {
        var capture = await UploadBytes("assistants", null);
        Assert.Equal("assistants", PartText(capture, "purpose"));
        var file = Assert.Single(capture.Parts, part => part.Name == "file");
        Assert.Equal(new byte[] { 1, 2, 3 }, file.Data);
        Assert.Equal("application/octet-stream", file.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/files/openai-files.test.ts::OpenAI Files - uploadFile::should return providerMetadata from response", Coverage = UpstreamCoverage.Covered)]
    public async Task ReturnsUploadMetadata()
    {
        var capture = new OpenAICapture { ResponseJson = UploadResponse };
        var metadata = await new OpenAIFileStore(OpenAIUpstream.Provider(capture)).UploadAsync("data.bin", new byte[] { 1, 2, 3 }, "application/octet-stream", "assistants", null, CancellationToken.None);
        Assert.Equal("file-abc123", metadata.Id);
        Assert.Equal("test.csv", metadata.Filename);
        Assert.Equal("assistants", metadata.Purpose);
        Assert.Equal(1024, metadata.Bytes);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1700000000), metadata.CreatedAt);
        Assert.Equal("processed", metadata.Status);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/files/openai-files.test.ts::OpenAI Files - uploadFile::should omit expires_after fields when no expiry is requested", Coverage = UpstreamCoverage.Covered)]
    public async Task OmitsFileExpiryFields()
    {
        var capture = await UploadBytes("assistants", null);
        Assert.DoesNotContain(capture.Parts, part => part.Name.StartsWith("expires_after", StringComparison.Ordinal));
    }

    [Fact]
    [UpstreamTest("packages/openai/src/files/openai-files.test.ts::OpenAI Files - uploadFile::should pass auth headers", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesFileAuthHeaders()
    {
        var capture = new OpenAICapture { ResponseJson = UploadResponse };
        var provider = OpenAIUpstream.Provider(capture, options =>
        {
            options.Organization = "test-org";
            options.Project = "test-project";
            options.Headers["Custom-Header"] = "custom-value";
        });
        await new OpenAIFileStore(provider).UploadAsync("data.bin", new byte[] { 1, 2, 3 }, "application/octet-stream", "assistants", null, CancellationToken.None);
        Assert.Equal("Bearer test-api-key", OpenAIUpstream.Header(capture, "Authorization"));
        Assert.Equal("test-org", OpenAIUpstream.Header(capture, "OpenAI-Organization"));
        Assert.Equal("test-project", OpenAIUpstream.Header(capture, "OpenAI-Project"));
        Assert.Equal("custom-value", OpenAIUpstream.Header(capture, "Custom-Header"));
    }

    [Fact]
    [UpstreamTest("packages/openai/src/files/openai-files.test.ts::OpenAI Files - uploadFile (stream data)::should stream a multipart upload with fields preceding the file part", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamsFileUploadAfterFields()
    {
        var capture = new OpenAICapture { ResponseJson = UploadResponse.Replace("file-abc123", "file-stream1") };
        using var data = new MemoryStream(Encoding.UTF8.GetBytes("{\"a\":1}\n{\"b\":2}\n"));
        var metadata = await new OpenAIFileStore(OpenAIUpstream.Provider(capture)).UploadAsync(data, "batch.jsonl", "application/jsonl", "batch", 172800, CancellationToken.None);
        Assert.Equal("file-stream1", metadata.Id);
        Assert.StartsWith("multipart/form-data; boundary=", OpenAIUpstream.Header(capture, "Content-Type"), StringComparison.Ordinal);
        Assert.Contains("boundary=\"ai-sdk-multipart-", OpenAIUpstream.Header(capture, "Content-Type"), StringComparison.Ordinal);
        Assert.Equal(new[] { "purpose", "expires_after[anchor]", "expires_after[seconds]", "file" }, capture.Parts.Select(part => part.Name));
        Assert.Equal("batch", PartText(capture, "purpose"));
        Assert.Equal("created_at", PartText(capture, "expires_after[anchor]"));
        Assert.Equal("172800", PartText(capture, "expires_after[seconds]"));
        Assert.Equal("batch.jsonl", capture.Parts[3].FileName);
        Assert.Equal("{\"a\":1}\n{\"b\":2}\n", Encoding.UTF8.GetString(capture.Parts[3].Data));
    }

    [Fact]
    [UpstreamTest("packages/openai/src/files/openai-files.test.ts::OpenAI Files - uploadFile (stream data)::should default the filename to \"blob\" on filename-less stream uploads", Coverage = UpstreamCoverage.Covered)]
    public async Task DefaultsStreamFileNameToBlob()
    {
        var capture = await UploadStream(null);
        Assert.Equal("blob", Assert.Single(capture.Parts, part => part.Name == "file").FileName);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/files/openai-files.test.ts::OpenAI Files - uploadFile (stream data)::should omit expiry fields on stream uploads without expiresAfter", Coverage = UpstreamCoverage.Covered)]
    public async Task OmitsStreamFileExpiryFields()
    {
        var capture = await UploadStream("batch.jsonl");
        Assert.DoesNotContain(capture.Parts, part => part.Name.StartsWith("expires_after", StringComparison.Ordinal));
    }

    [Fact]
    [UpstreamTest("packages/openai/src/files/openai-files.test.ts::OpenAI Files - uploadFile (result fields)::should expose byteSize/createdAt/expiresAt from the upload response", Coverage = UpstreamCoverage.Covered)]
    public async Task ExposesUploadResultFields()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"id\":\"file-exp1\",\"object\":\"file\",\"bytes\":2048,\"created_at\":1700000000,\"filename\":\"batch.jsonl\",\"purpose\":\"batch\",\"status\":\"processed\",\"expires_at\":1700172800}" };
        var metadata = await new OpenAIFileStore(OpenAIUpstream.Provider(capture)).UploadAsync("batch.jsonl", new byte[] { 1, 2, 3 }, "application/jsonl", "batch", 172800, CancellationToken.None);
        Assert.Equal(2048, metadata.Bytes);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1700000000), metadata.CreatedAt);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1700172800), metadata.ExpiresAt);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/files/openai-files.test.ts::OpenAI Files - downloadFile::should download file content as a stream", Coverage = UpstreamCoverage.Partial, Note = "The GET request and the content match. DownloadAsync returns the bytes, not a stream.")]
    public async Task DownloadsFileContent()
    {
        var capture = new OpenAICapture { ResponseBytes = Encoding.UTF8.GetBytes("{\"result\":\"ok\"}\n"), ResponseMediaType = "application/octet-stream" };
        var download = await new OpenAIFileStore(OpenAIUpstream.Provider(capture)).DownloadAsync(new OpenAIFileReference("file-abc123"), CancellationToken.None);
        Assert.Equal("GET", capture.Method);
        Assert.EndsWith("/files/file-abc123/content", capture.Uri, StringComparison.Ordinal);
        Assert.Equal("{\"result\":\"ok\"}\n", Encoding.UTF8.GetString(download.Data));
    }

    [Fact]
    [UpstreamTest("packages/openai/src/skills/openai-skills.test.ts::OpenAISkills > uploadSkill::should send files as multipart form data", Coverage = UpstreamCoverage.Covered)]
    public async Task UploadsSkillFiles()
    {
        var capture = new OpenAICapture { ResponseBytes = System.Text.Encoding.UTF8.GetBytes("{\"id\":\"skill_1\",\"name\":\"test\"}"), ResponseMediaType = "application/json" };
        await new OpenAISkillStore(OpenAIUpstream.Provider(capture)).UploadAsync(new[] { new OpenAISkillFile("SKILL.md", System.Text.Encoding.UTF8.GetBytes("# Skill"), "text/markdown") }, null, CancellationToken.None);
        Assert.Contains("/skills", capture.Uri, StringComparison.Ordinal);
        Assert.Equal("SKILL.md", Assert.Single(capture.Parts, part => part.Name == "files[]").FileName);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/skills/openai-skills.test.ts::OpenAISkills > uploadSkill::should emit unsupported warning for displayTitle", Coverage = UpstreamCoverage.Covered)]
    public async Task WarnsForSkillDisplayTitle()
    {
        var capture = SkillFixture();
        var result = await new OpenAISkillStore(OpenAIUpstream.Provider(capture)).UploadAsync(new[] { new OpenAISkillFile("SKILL.md", new byte[] { 1 }, "text/markdown") }, "Title", CancellationToken.None);
        Assert.Equal("displayTitle", result.Warnings[0].Feature);
        Assert.Equal("skill_699fc58f408c8191825d8d06ae75fd5c06de7b381a5db7f5", result.Id);
        Assert.Equal("1", result.LatestVersion);
        Assert.Equal(1772078479, result.CreatedAt);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/skills/openai-skills.test.ts::OpenAISkills > uploadSkill::should return no warnings when displayTitle is not set", Coverage = UpstreamCoverage.Covered)]
    public async Task OmitsSkillWarnings()
    {
        var capture = new OpenAICapture { ResponseBytes = System.Text.Encoding.UTF8.GetBytes("{\"id\":\"skill_1\"}"), ResponseMediaType = "application/json" };
        var result = await new OpenAISkillStore(OpenAIUpstream.Provider(capture)).UploadAsync(new[] { new OpenAISkillFile("SKILL.md", new byte[] { 1 }, "text/markdown") }, null, CancellationToken.None);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/skills/openai-skills.test.ts::OpenAISkills > uploadSkill::should pass authorization headers", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesSkillAuthorization()
    {
        var capture = SkillFixture();
        await new OpenAISkillStore(OpenAIUpstream.Provider(capture)).UploadAsync(new[] { SkillSourceFile() }, null, CancellationToken.None);
        Assert.Equal("Bearer test-api-key", OpenAIUpstream.Header(capture, "Authorization"));
    }

    [Fact]
    [UpstreamTest("packages/openai/src/skills/openai-skills.test.ts::OpenAISkills > uploadSkill::should map response to providerReference", Coverage = UpstreamCoverage.Covered)]
    public async Task MapsSkillUploadResponse()
    {
        var result = await new OpenAISkillStore(OpenAIUpstream.Provider(SkillFixture())).UploadAsync(new[] { SkillSourceFile() }, null, CancellationToken.None);
        Assert.Equal("skill_699fc58f408c8191825d8d06ae75fd5c06de7b381a5db7f5", result.Id);
        Assert.Equal("test-capture-skill", result.Name);
        Assert.Equal("A test skill for fixture capture", result.Description);
        Assert.Equal("1", result.LatestVersion);
        Assert.Equal("1", result.DefaultVersion);
        Assert.Equal(1772078479, result.CreatedAt);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/skills/openai-skills.test.ts::OpenAISkills > uploadSkill::should handle Uint8Array file content", Coverage = UpstreamCoverage.Covered)]
    public async Task UploadsBinarySkillFiles()
    {
        var capture = SkillFixture();
        var result = await new OpenAISkillStore(OpenAIUpstream.Provider(capture)).UploadAsync(
            new[] { new OpenAISkillFile("data.bin", new byte[] { 0x48, 0x65, 0x6c, 0x6c, 0x6f }, "application/octet-stream") },
            null,
            CancellationToken.None);
        Assert.Equal("skill_699fc58f408c8191825d8d06ae75fd5c06de7b381a5db7f5", result.Id);
        var part = Assert.Single(capture.Parts, part => part.Name == "files[]");
        Assert.Equal("data.bin", part.FileName);
        Assert.Equal(new byte[] { 0x48, 0x65, 0x6c, 0x6c, 0x6f }, part.Data);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/completion/openai-completion-language-model.test.ts::doGenerate::should pass the model and the prompt", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesCompletionPrompt()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"id\":\"cmpl\",\"created\":1,\"model\":\"gpt-3.5-turbo-instruct\",\"choices\":[{\"text\":\"ok\",\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":1,\"completion_tokens\":1,\"total_tokens\":2}}" };
        await OpenAIUpstream.Provider(capture).CompletionModel("gpt-3.5-turbo-instruct").DoGenerateAsync(OpenAIUpstream.Hello(), CancellationToken.None);
        var body = JsonNode.Parse(capture.Body)!;
        Assert.Equal("gpt-3.5-turbo-instruct", body["model"]!.GetValue<string>());
        Assert.Equal("user:\nHello\n\nassistant:\n", body["prompt"]!.GetValue<string>());
        Assert.Equal("\nuser:", body["stop"]![0]!.GetValue<string>());
        Assert.EndsWith("/completions", capture.Uri, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/completion/openai-completion-language-model.test.ts::doGenerate::should extract text response", Coverage = UpstreamCoverage.Covered)]
    public async Task ExtractsCompletionText()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"choices\":[{\"text\":\"Hello, World!\",\"finish_reason\":\"stop\"}]}" };
        var result = await OpenAIUpstream.Provider(capture).CompletionModel("gpt-3.5-turbo-instruct").DoGenerateAsync(OpenAIUpstream.Hello(), CancellationToken.None);
        Assert.Equal("Hello, World!", result.Text);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/completion/openai-completion-language-model.test.ts::doGenerate::should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task ExtractsCompletionUsage()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"choices\":[{\"text\":\"\",\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":20,\"completion_tokens\":5,\"total_tokens\":25}}" };
        var usage = (await OpenAIUpstream.Provider(capture).CompletionModel("gpt-3.5-turbo-instruct").DoGenerateAsync(OpenAIUpstream.Hello(), CancellationToken.None)).Usage;
        Assert.Equal(20, usage.InputTokens);
        Assert.Equal(20, usage.NoCacheInputTokens);
        Assert.Null(usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(5, usage.OutputTokens);
        Assert.Equal(5, usage.TextTokens);
        Assert.Null(usage.ReasoningTokens);
        Assert.Equal("{\"prompt_tokens\":20,\"completion_tokens\":5,\"total_tokens\":25}", usage.Raw!.Value.GetRawText());
    }

    [Fact]
    [UpstreamTest("packages/openai/src/completion/openai-completion-language-model.test.ts::doGenerate::should support unknown finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task SupportsUnknownCompletionFinish()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"choices\":[{\"text\":\"\",\"finish_reason\":\"eos\"}]}" };
        var result = await OpenAIUpstream.Provider(capture).CompletionModel("gpt-3.5-turbo-instruct").DoGenerateAsync(OpenAIUpstream.Hello(), CancellationToken.None);
        Assert.Equal(FinishReason.Other, result.FinishReason);
        Assert.Equal("eos", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/realtime/openai-realtime-model.test.ts::OpenAIRealtimeModel > doCreateClientSecret::omits expires_after when no ttl is requested", Coverage = UpstreamCoverage.Covered)]
    public async Task OmitsRealtimeExpiry()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"value\":\"ek_test\",\"expires_at\":1700000000}" };
        var secret = await new OpenAIRealtimeModel(OpenAIUpstream.Provider(capture), "gpt-4o-realtime-preview").CreateClientSecretAsync(null, CancellationToken.None);
        OpenAIUpstream.Equal(JsonNode.Parse(capture.Body), "{\"session\":{\"type\":\"realtime\",\"model\":\"gpt-4o-realtime-preview\"}}");
        Assert.Equal("ek_test", secret.Token);
        Assert.Equal("wss://api.openai.com/v1/realtime?model=gpt-4o-realtime-preview", secret.Url);
        Assert.DoesNotContain("expires_after", capture.Body, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/realtime/openai-realtime-model.test.ts::OpenAIRealtimeModel > doCreateClientSecret::includes the required anchor with expires_after", Coverage = UpstreamCoverage.Covered)]
    public async Task IncludesRealtimeExpiryAnchor()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"value\":\"ek_test\",\"expires_at\":1700000060}" };
        await new OpenAIRealtimeModel(OpenAIUpstream.Provider(capture), "gpt-4o-realtime-preview").CreateClientSecretAsync(60, CancellationToken.None);
        OpenAIUpstream.Equal(JsonNode.Parse(capture.Body)!["expires_after"], "{\"anchor\":\"created_at\",\"seconds\":60}");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/embedding/openai-embedding-model.test.ts::doEmbed::should expose the raw response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task ExposesEmbeddingResponseHeaders()
    {
        var capture = EmbeddingFixture();
        capture.ResponseHeaders["test-header"] = "test-value";
        var result = await new OpenAIEmbeddingModel(OpenAIUpstream.Provider(capture), "text-embedding-3-large").EmbedAsync(EmbeddingValues, null, null, null, CancellationToken.None);
        Assert.Equal(
            new Dictionary<string, string> { ["content-length"] = "327", ["content-type"] = "application/json", ["test-header"] = "test-value" },
            result.Headers.ToDictionary(pair => pair.Key.ToLowerInvariant(), pair => pair.Value));
    }

    [Fact]
    [UpstreamTest("packages/openai/src/embedding/openai-embedding-model.test.ts::doEmbed::should expose the raw response body", Coverage = UpstreamCoverage.Covered)]
    public async Task ExposesEmbeddingResponseBody()
    {
        var result = await new OpenAIEmbeddingModel(OpenAIUpstream.Provider(EmbeddingFixture()), "text-embedding-3-large").EmbedAsync(EmbeddingValues, null, null, null, CancellationToken.None);
        OpenAIUpstream.Equal(JsonNode.Parse(result.RawBody), EmbeddingFixtureJson);
        Assert.Equal(
            new Dictionary<string, string> { ["content-length"] = "327", ["content-type"] = "application/json" },
            result.Headers.ToDictionary(pair => pair.Key.ToLowerInvariant(), pair => pair.Value));
    }

    [Fact]
    [UpstreamTest("packages/openai/src/embedding/openai-embedding-model.test.ts::doEmbed::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesEmbeddingHeaders()
    {
        var capture = EmbeddingFixture();
        await new OpenAIEmbeddingModel(StandardProvider(capture), "text-embedding-3-large").EmbedAsync(
            EmbeddingValues,
            null,
            null,
            new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" },
            CancellationToken.None);
        // HttpClient adds Content-Length and a UTF-8 charset; the upstream test server reports neither.
        var headers = capture.Headers
            .Where(pair => !pair.Key.Equals("User-Agent", StringComparison.OrdinalIgnoreCase) && !pair.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(pair => pair.Key.ToLowerInvariant(), pair => pair.Value);
        headers["content-type"] = MediaTypeHeaderValue.Parse(headers["content-type"]).MediaType!;
        Assert.Equal(
            new Dictionary<string, string>
            {
                ["authorization"] = "Bearer test-api-key",
                ["content-type"] = "application/json",
                ["custom-provider-header"] = "provider-header-value",
                ["custom-request-header"] = "request-header-value",
                ["openai-organization"] = "test-organization",
                ["openai-project"] = "test-project",
            },
            headers);
        Assert.Contains(OpenAIProvider.UserAgentSuffix, OpenAIUpstream.Header(capture, "user-agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/image/openai-image-model.test.ts::doGenerate::should not include response_format for future gpt-image models", Coverage = UpstreamCoverage.Covered)]
    public async Task OmitsResponseFormatForFutureGptImage()
    {
        var capture = await Image("gpt-image-99", new OpenAIImageCall("A cute baby sea otter") { Count = 1, Size = "1024x1024" });
        OpenAIUpstream.Equal(JsonNode.Parse(capture.Body), "{\"model\":\"gpt-image-99\",\"prompt\":\"A cute baby sea otter\",\"n\":1,\"size\":\"1024x1024\"}");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/image/openai-image-model.test.ts::doGenerate::should not include response_format for date-suffixed gpt-image model IDs (Azure deployment names)", Coverage = UpstreamCoverage.Covered)]
    public async Task OmitsResponseFormatForDatedGptImage()
    {
        var capture = await Image("gpt-image-1.5-2025-12-16", new OpenAIImageCall("A cute baby sea otter") { Count = 1, Size = "1024x1024" });
        OpenAIUpstream.Equal(JsonNode.Parse(capture.Body), "{\"model\":\"gpt-image-1.5-2025-12-16\",\"prompt\":\"A cute baby sea otter\",\"n\":1,\"size\":\"1024x1024\"}");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/image/openai-image-model.test.ts::doGenerate::should include response data with timestamp, modelId and headers", Coverage = UpstreamCoverage.Partial, Note = "The injected clock is stored on the result. The image result does not carry model id or response headers.")]
    public async Task RecordsImageTimestamp()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"data\":[{\"b64_json\":\"AQID\"}]}" };
        var stamp = new DateTimeOffset(2024, 3, 15, 12, 0, 0, TimeSpan.Zero);
        var model = new OpenAIImageModel(OpenAIUpstream.Provider(capture), "dall-e-3", () => stamp);
        var result = await model.GenerateAsync(new OpenAIImageCall("A cute baby sea otter") { Count = 1, Size = "1024x1024" }, CancellationToken.None);
        Assert.Equal(stamp, result.Timestamp);
        Assert.Equal("dall-e-3", model.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/speech/openai-speech-model.test.ts::doGenerate::should pass provider options", Coverage = UpstreamCoverage.Covered)]
    public void PassesSpeechProviderOptions()
    {
        var prepared = OpenAISpeechModel.Prepare("tts-1", new OpenAISpeechCall("Hello from the AI SDK!")
        {
            Speed = 1.5,
            ProviderOptions = OpenAIUpstream.Json("{\"speed\":0.75,\"instructions\":\"Speak slowly.\"}"),
        });
        Assert.Equal(0.75, prepared.Body["speed"]!.GetValue<double>());
        Assert.Equal("Speak slowly.", prepared.Body["instructions"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/openai/src/speech/openai-speech-model.test.ts::doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesSpeechHeaders()
    {
        var capture = new OpenAICapture { ResponseBytes = new byte[] { 1 } };
        await new OpenAISpeechModel(StandardProvider(capture), "tts-1").GenerateAsync(new OpenAISpeechCall("Hello from the AI SDK!")
        {
            Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" },
        }, CancellationToken.None);
        OpenAIUpstream.AssertStandardHeaders(capture);
        Assert.Contains(OpenAIProvider.UserAgentSuffix, OpenAIUpstream.Header(capture, "user-agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/speech/openai-speech-model.test.ts::doGenerate::should include response data with timestamp, modelId and headers", Coverage = UpstreamCoverage.Covered)]
    public async Task IncludesSpeechResponseData()
    {
        var stamp = DateTimeOffset.UnixEpoch;
        var capture = new OpenAICapture { ResponseBytes = new byte[100], ResponseMediaType = "audio/mp3" };
        capture.ResponseHeaders["x-request-id"] = "test-request-id";
        capture.ResponseHeaders["x-ratelimit-remaining"] = "123";
        var result = await new OpenAISpeechModel(OpenAIUpstream.Provider(capture), "tts-1", () => stamp).GenerateAsync(new OpenAISpeechCall("Hello from the AI SDK!"), CancellationToken.None);
        Assert.Equal(stamp, result.Timestamp);
        Assert.Equal("tts-1", result.ModelId);
        Assert.Equal("test-request-id", result.Headers["x-request-id"]);
        Assert.Equal("123", result.Headers["x-ratelimit-remaining"]);
        Assert.Equal("audio/mp3", result.Headers["Content-Type"]);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/speech/openai-speech-model.test.ts::doGenerate::should use real date when no custom date provider is specified", Coverage = UpstreamCoverage.Covered)]
    public async Task UsesInjectedSpeechClock()
    {
        var stamp = DateTimeOffset.UnixEpoch;
        var capture = new OpenAICapture { ResponseBytes = new byte[1] };
        var result = await new OpenAISpeechModel(OpenAIUpstream.Provider(capture), "tts-1", () => stamp).GenerateAsync(new OpenAISpeechCall("Hello from the AI SDK!"), CancellationToken.None);
        Assert.Equal(stamp, result.Timestamp);
        Assert.Equal("tts-1", result.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/speech/openai-speech-model.test.ts::doGenerate::should handle different audio formats", Coverage = UpstreamCoverage.Covered)]
    public async Task ReturnsAudioForEachFormat()
    {
        foreach (var format in new[] { "mp3", "opus", "aac", "flac", "wav", "pcm" })
        {
            var audio = new byte[100];
            var capture = new OpenAICapture { ResponseBytes = audio, ResponseMediaType = "audio/" + format };
            var result = await new OpenAISpeechModel(OpenAIUpstream.Provider(capture), "tts-1").GenerateAsync(new OpenAISpeechCall("Hello from the AI SDK!")
            {
                ProviderOptions = OpenAIUpstream.Json("{\"response_format\":\"" + format + "\"}"),
            }, CancellationToken.None);
            Assert.Equal(audio, result.Audio);
        }
    }

    [Fact]
    [UpstreamTest("packages/openai/src/transcription/openai-transcription-model.test.ts::doGenerate::should pass the model", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesTranscriptionModel()
    {
        var fields = await TranscriptionFields("whisper-1", null);
        Assert.Equal("whisper-1", fields["model"][0]);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/files/openai-files.test.ts::OpenAI Files - getFileMetadata::should retrieve file metadata via GET", Coverage = UpstreamCoverage.Covered)]
    public async Task RetrievesFileMetadata()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"id\":\"file-abc123\",\"bytes\":1024,\"created_at\":1700000000,\"filename\":\"test.jsonl\",\"purpose\":\"batch\",\"status\":\"processed\",\"expires_at\":1700172800}" };
        var metadata = await new OpenAIFileStore(OpenAIUpstream.Provider(capture)).GetMetadataAsync(new OpenAIFileReference("file-abc123"), CancellationToken.None);
        Assert.Equal("GET", capture.Method);
        Assert.EndsWith("/files/file-abc123", capture.Uri, StringComparison.Ordinal);
        Assert.Equal("file-abc123", metadata.Id);
        Assert.Equal("test.jsonl", metadata.Filename);
        Assert.Equal(1024, metadata.Bytes);
        Assert.Equal("batch", metadata.Purpose);
        Assert.Equal("processed", metadata.Status);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1700000000), metadata.CreatedAt);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1700172800), metadata.ExpiresAt);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/completion/openai-completion-language-model.test.ts::doGenerate::should send request body", Coverage = UpstreamCoverage.Covered)]
    public void SendsCompletionRequestBody()
    {
        var prepared = OpenAICompletionLanguageModel.Prepare("gpt-3.5-turbo-instruct", OpenAIUpstream.Hello(), false);
        OpenAIUpstream.Equal(prepared.Body, "{\"model\":\"gpt-3.5-turbo-instruct\",\"prompt\":\"user:\\nHello\\n\\nassistant:\\n\",\"stop\":[\"\\nuser:\"]}");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/completion/openai-completion-language-model.test.ts::doGenerate::should send additional response information", Coverage = UpstreamCoverage.Covered)]
    public async Task SendsCompletionResponseInformation()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"id\":\"test-id\",\"created\":123,\"model\":\"test-model\",\"choices\":[{\"text\":\"\",\"finish_reason\":\"stop\"}]}" };
        var result = await OpenAIUpstream.Provider(capture).CompletionModel("gpt-3.5-turbo-instruct").DoGenerateAsync(OpenAIUpstream.Hello(), CancellationToken.None);
        Assert.Equal("test-id", result.ResponseId);
        Assert.Equal("test-model", result.ResponseModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(123), result.ResponseTimestamp);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/completion/openai-completion-language-model.test.ts::doGenerate::should extract finish reason", Coverage = UpstreamCoverage.Covered)]
    public async Task ExtractsCompletionFinishReason()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"choices\":[{\"text\":\"\",\"finish_reason\":\"stop\"}]}" };
        var result = await OpenAIUpstream.Provider(capture).CompletionModel("gpt-3.5-turbo-instruct").DoGenerateAsync(OpenAIUpstream.Hello(), CancellationToken.None);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("stop", result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/completion/openai-completion-language-model.test.ts::doGenerate::should extract logprobs", Coverage = UpstreamCoverage.Covered)]
    public async Task ExtractsCompletionLogprobs()
    {
        const string logprobs = "{\"tokens\":[\"Hi\"],\"token_logprobs\":[-0.1]}";
        var capture = new OpenAICapture { ResponseJson = "{\"choices\":[{\"text\":\"Hi\",\"logprobs\":" + logprobs + ",\"finish_reason\":\"stop\"}]}" };
        var result = await OpenAIUpstream.Provider(capture).CompletionModel("gpt-3.5-turbo-instruct").DoGenerateAsync(
            new LanguageModelCallOptions
            {
                Prompt = OpenAIUpstream.Hello().Prompt,
                ProviderOptions = OpenAIUpstream.OpenAIOptionsJson("{\"logprobs\":1}"),
            },
            CancellationToken.None);
        OpenAIUpstream.Equal(JsonNode.Parse(result.ProviderMetadata!.Value.GetProperty("openai").GetProperty("logprobs").GetRawText()), logprobs);
        Assert.Equal(1, JsonNode.Parse(capture.Body)!["logprobs"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/openai/src/completion/openai-completion-language-model.test.ts::doGenerate::should expose the raw response headers", Coverage = UpstreamCoverage.Covered)]
    public async Task ExposesCompletionResponseHeaders()
    {
        const string json = "{\"id\":\"cmpl-96cAM1v77r4jXa4qb2NSmRREV5oWB\",\"object\":\"text_completion\",\"created\":1711363706,\"model\":\"gpt-3.5-turbo-instruct\",\"choices\":[{\"text\":\"\",\"index\":0,\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":4,\"total_tokens\":34,\"completion_tokens\":30}}";
        var capture = new OpenAICapture { ResponseBytes = System.Text.Encoding.UTF8.GetBytes(json) };
        capture.ResponseHeaders["test-header"] = "test-value";
        var result = await OpenAIUpstream.Provider(capture).CompletionModel("gpt-3.5-turbo-instruct").DoGenerateAsync(OpenAIUpstream.Hello(), CancellationToken.None);
        Assert.Equal(
            new Dictionary<string, string> { ["content-length"] = "250", ["content-type"] = "application/json", ["test-header"] = "test-value" },
            result.ResponseHeaders.ToDictionary(pair => pair.Key.ToLowerInvariant(), pair => pair.Value));
    }

    [Fact]
    [UpstreamTest("packages/openai/src/completion/openai-completion-language-model.test.ts::doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesCompletionHeaders()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"choices\":[{\"text\":\"\",\"finish_reason\":\"stop\"}]}" };
        await StandardProvider(capture).CompletionModel("gpt-3.5-turbo-instruct").DoGenerateAsync(CompletionCall(), CancellationToken.None);
        AssertCompletionHeaders(capture);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/completion/openai-completion-language-model.test.ts::doStream::should stream text deltas", Coverage = UpstreamCoverage.Partial, Note = "Parts, metadata, logprobs, and token totals match. Usage noCache and text stay null because the completion API reports no cache or reasoning tokens.")]
    public async Task StreamsCompletionTextDeltas()
    {
        const string logprobs = "{\"tokens\":[\" ever\",\" after\",\".\\n\\n\",\"The\",\" end\",\".\"],\"token_logprobs\":[-0.0664508,-0.014520033,-1.3820221,-0.7890417,-0.5323165,-0.10247037],\"top_logprobs\":[{\" ever\":-0.0664508},{\" after\":-0.014520033},{\".\\n\\n\":-1.3820221},{\"The\":-0.7890417},{\" end\":-0.5323165},{\".\":-0.10247037}]}";
        var parts = await StreamCompletion(
            "data: {\"id\":\"cmpl-96c64EdfhOw8pjFFgVpLuT8k2MtdT\",\"object\":\"text_completion\",\"created\":1711363440,\"choices\":[{\"text\":\"Hello\",\"index\":0,\"logprobs\":" + logprobs + ",\"finish_reason\":null}],\"model\":\"gpt-3.5-turbo-instruct\"}\n\n"
            + "data: {\"id\":\"cmpl-96c64EdfhOw8pjFFgVpLuT8k2MtdT\",\"object\":\"text_completion\",\"created\":1711363440,\"choices\":[{\"text\":\", \",\"index\":0,\"logprobs\":" + logprobs + ",\"finish_reason\":null}],\"model\":\"gpt-3.5-turbo-instruct\"}\n\n"
            + "data: {\"id\":\"cmpl-96c64EdfhOw8pjFFgVpLuT8k2MtdT\",\"object\":\"text_completion\",\"created\":1711363440,\"choices\":[{\"text\":\"World!\",\"index\":0,\"logprobs\":" + logprobs + ",\"finish_reason\":null}],\"model\":\"gpt-3.5-turbo-instruct\"}\n\n"
            + "data: {\"id\":\"cmpl-96c3yLQE1TtZCd6n6OILVmzev8M8H\",\"object\":\"text_completion\",\"created\":1711363310,\"choices\":[{\"text\":\"\",\"index\":0,\"logprobs\":" + logprobs + ",\"finish_reason\":\"stop\"}],\"model\":\"gpt-3.5-turbo-instruct\"}\n\n"
            + "data: {\"id\":\"cmpl-96c3yLQE1TtZCd6n6OILVmzev8M8H\",\"object\":\"text_completion\",\"created\":1711363310,\"model\":\"gpt-3.5-turbo-instruct\",\"usage\":{\"prompt_tokens\":10,\"total_tokens\":372,\"completion_tokens\":362},\"choices\":[]}\n\n"
            + "data: [DONE]\n\n");
        Assert.Equal(new[] { "stream-start", "response-metadata", "text-start", "text-delta", "text-delta", "text-delta", "text-end", "finish" }, parts.Select(part => part.Type));
        Assert.Empty(Assert.IsType<StreamStartStreamPart>(parts[0]).Warnings);
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(parts[1]);
        Assert.Equal("cmpl-96c64EdfhOw8pjFFgVpLuT8k2MtdT", metadata.Id);
        Assert.Equal("gpt-3.5-turbo-instruct", metadata.ModelId);
        Assert.Equal(DateTimeOffset.Parse("2024-03-25T10:44:00Z", System.Globalization.CultureInfo.InvariantCulture), metadata.Timestamp);
        Assert.Equal(new[] { "Hello", ", ", "World!" }, parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta));
        Assert.All(parts.OfType<TextDeltaStreamPart>(), part => Assert.Equal("0", part.Id));
        var finish = Assert.IsType<FinishStreamPart>(parts[7]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal("stop", finish.RawFinishReason);
        OpenAIUpstream.Equal(JsonNode.Parse(finish.ProviderMetadata!.Value.GetRawText()), "{\"openai\":{\"logprobs\":" + logprobs + "}}");
        Assert.Equal(10, finish.Usage.InputTokens);
        Assert.Equal(362, finish.Usage.OutputTokens);
        Assert.Null(finish.Usage.CacheReadTokens);
        Assert.Null(finish.Usage.CacheWriteTokens);
        Assert.Null(finish.Usage.ReasoningTokens);
        OpenAIUpstream.Equal(JsonNode.Parse(finish.Usage.Raw!.Value.GetRawText()), "{\"prompt_tokens\":10,\"total_tokens\":372,\"completion_tokens\":362}");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/completion/openai-completion-language-model.test.ts::doStream::should throw an api error when the first stream chunk is an error", Coverage = UpstreamCoverage.Covered)]
    public async Task ThrowsEarlyCompletionStreamError()
    {
        const string message = "The server had an error processing your request. Sorry about that! You can retry your request, or contact us through our help center at help.openai.com if you keep seeing this error.";
        var exception = await Assert.ThrowsAsync<InternalServerException>(() => StreamCompletion("data: {\"error\":{\"message\":\"" + message + "\",\"type\":\"server_error\",\"param\":null,\"code\":null}}\n\ndata: [DONE]\n\n"));
        Assert.Equal(message, exception.Message);
        Assert.Equal(500, exception.StatusCode);
        Assert.True(ProviderHttp.IsRetryable(exception.StatusCode));
    }

    [Fact]
    [UpstreamTest("packages/openai/src/completion/openai-completion-language-model.test.ts::doStream::should forward error stream parts after output has started", Coverage = UpstreamCoverage.Partial, Note = "The error part carries the message string. Upstream wraps a structured error object.")]
    public async Task ForwardsCompletionErrorAfterOutput()
    {
        var parts = await StreamCompletion(
            "data: {\"id\":\"cmpl-error-after-output\",\"object\":\"text_completion\",\"created\":1711363440,\"choices\":[{\"text\":\"Hello\",\"index\":0,\"logprobs\":null,\"finish_reason\":null}],\"model\":\"gpt-3.5-turbo-instruct\"}\n\n"
            + "data: {\"error\":{\"message\":\"stream failed after output\",\"type\":\"server_error\",\"param\":null,\"code\":null}}\n\n"
            + "data: [DONE]\n\n");
        Assert.Equal(new[] { "stream-start", "response-metadata", "text-start", "text-delta", "error", "text-end", "finish" }, parts.Select(part => part.Type));
        Assert.Equal("cmpl-error-after-output", Assert.IsType<ResponseMetadataStreamPart>(parts[1]).Id);
        Assert.Equal("Hello", Assert.IsType<TextDeltaStreamPart>(parts[3]).Delta);
        Assert.Equal("stream failed after output", Assert.IsType<ErrorStreamPart>(parts[4]).Message);
        AssertCompletionErrorFinish(parts[6]);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/completion/openai-completion-language-model.test.ts::doStream::should handle unparsable stream parts", Coverage = UpstreamCoverage.Partial, Note = "The error message is JSON parsing failed: Text: {data}. Upstream also appends a SyntaxError.")]
    public async Task HandlesUnparsableCompletionStreamParts()
    {
        var parts = await StreamCompletion("data: {unparsable}\n\ndata: [DONE]\n\n");
        Assert.Equal(new[] { "stream-start", "error", "finish" }, parts.Select(part => part.Type));
        Assert.Equal("JSON parsing failed: Text: {unparsable}.", Assert.IsType<ErrorStreamPart>(parts[1]).Message);
        AssertCompletionErrorFinish(parts[2]);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/completion/openai-completion-language-model.test.ts::doStream::should send request body", Coverage = UpstreamCoverage.Covered)]
    public void SendsCompletionStreamRequestBody()
    {
        var prepared = OpenAICompletionLanguageModel.Prepare("gpt-3.5-turbo-instruct", OpenAIUpstream.Hello(), true);
        OpenAIUpstream.Equal(prepared.Body, "{\"model\":\"gpt-3.5-turbo-instruct\",\"prompt\":\"user:\\nHello\\n\\nassistant:\\n\",\"stop\":[\"\\nuser:\"],\"stream\":true,\"stream_options\":{\"include_usage\":true}}");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/completion/openai-completion-language-model.test.ts::doStream::should pass the model and the prompt", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesCompletionStreamPrompt()
    {
        var capture = new OpenAICapture { ServerSentEvents = CompletionStreamOk };
        await OpenAIUpstream.Read(OpenAIUpstream.Provider(capture).CompletionModel("gpt-3.5-turbo-instruct").DoStreamAsync(OpenAIUpstream.Hello(), CancellationToken.None));
        OpenAIUpstream.Equal(JsonNode.Parse(capture.Body), "{\"model\":\"gpt-3.5-turbo-instruct\",\"prompt\":\"user:\\nHello\\n\\nassistant:\\n\",\"stop\":[\"\\nuser:\"],\"stream\":true,\"stream_options\":{\"include_usage\":true}}");
        Assert.EndsWith("/completions", capture.Uri, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/completion/openai-completion-language-model.test.ts::doStream::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesCompletionStreamHeaders()
    {
        var capture = new OpenAICapture { ServerSentEvents = CompletionStreamOk };
        await OpenAIUpstream.Read(StandardProvider(capture).CompletionModel("gpt-3.5-turbo-instruct").DoStreamAsync(CompletionCall(), CancellationToken.None));
        AssertCompletionHeaders(capture);
        Assert.Contains(OpenAIProvider.UserAgentSuffix, OpenAIUpstream.Header(capture, "user-agent"), StringComparison.Ordinal);
    }

    private static OpenAICapture EmbeddingFixture()
    {
        return new OpenAICapture { ResponseBytes = Encoding.UTF8.GetBytes(EmbeddingFixtureJson), ResponseMediaType = "application/json" };
    }

    private static OpenAICapture SkillFixture()
    {
        return new OpenAICapture { ResponseBytes = Encoding.UTF8.GetBytes(SkillCreateJson), ResponseMediaType = "application/json" };
    }

    private static OpenAISkillFile SkillSourceFile()
    {
        return new OpenAISkillFile("index.ts", Encoding.UTF8.GetBytes("console.log(\"hello\")"), "application/octet-stream");
    }

    private static OpenAIProvider StandardProvider(OpenAICapture capture)
    {
        return OpenAIUpstream.Provider(capture, options =>
        {
            options.Organization = "test-organization";
            options.Project = "test-project";
            options.Headers["Custom-Provider-Header"] = "provider-header-value";
        });
    }

    private const string CompletionStreamOk =
        "data: {\"id\":\"cmpl\",\"created\":1,\"model\":\"gpt-3.5-turbo-instruct\",\"choices\":[{\"text\":\"ok\",\"finish_reason\":\"stop\"}]}\n\ndata: [DONE]\n\n";

    private static LanguageModelCallOptions CompletionCall()
    {
        var call = OpenAIUpstream.Hello();
        call.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" };
        return call;
    }

    private static void AssertCompletionHeaders(OpenAICapture capture)
    {
        // Upstream reads the user agent separately, and HttpClient adds Content-Length at the transport.
        var headers = capture.Headers
            .Where(pair => pair.Key is not ("User-Agent" or "Content-Length"))
            .ToDictionary(pair => pair.Key.ToLowerInvariant(), pair => pair.Value);
        headers["content-type"] = headers["content-type"].Split(';')[0];
        Assert.Equal(
            new Dictionary<string, string>
            {
                ["authorization"] = "Bearer test-api-key",
                ["content-type"] = "application/json",
                ["custom-provider-header"] = "provider-header-value",
                ["custom-request-header"] = "request-header-value",
                ["openai-organization"] = "test-organization",
                ["openai-project"] = "test-project",
            },
            headers);
    }

    private static void AssertCompletionErrorFinish(LanguageModelStreamPart part)
    {
        var finish = Assert.IsType<FinishStreamPart>(part);
        Assert.Equal(FinishReason.Error, finish.FinishReason);
        Assert.Null(finish.RawFinishReason);
        OpenAIUpstream.Equal(JsonNode.Parse(finish.ProviderMetadata!.Value.GetRawText()), "{\"openai\":{}}");
        Assert.Null(finish.Usage.InputTokens);
        Assert.Null(finish.Usage.OutputTokens);
        Assert.Null(finish.Usage.Raw);
    }

    private static async Task<List<LanguageModelStreamPart>> StreamCompletion(string events)
    {
        var capture = new OpenAICapture { ServerSentEvents = events };
        return await OpenAIUpstream.Read(OpenAIUpstream.Provider(capture).CompletionModel("gpt-3.5-turbo-instruct").DoStreamAsync(OpenAIUpstream.Hello(), CancellationToken.None)).ConfigureAwait(false);
    }

    private static async Task<OpenAIEmbeddingCallResult> Embed(string json, string modelId, IReadOnlyList<string> values)
    {
        var capture = new OpenAICapture { ResponseJson = json };
        return await new OpenAIEmbeddingModel(OpenAIUpstream.Provider(capture), modelId).EmbedAsync(values, null, null, null, CancellationToken.None).ConfigureAwait(false);
    }

    private static async Task<OpenAICapture> Image(string modelId, OpenAIImageCall call)
    {
        var capture = new OpenAICapture { ResponseJson = "{\"data\":[{\"b64_json\":\"AQID\"}]}" };
        await new OpenAIImageModel(OpenAIUpstream.Provider(capture), modelId).GenerateAsync(call, CancellationToken.None).ConfigureAwait(false);
        return capture;
    }

    private static async Task AssertNoResponseFormat(string modelId)
    {
        var capture = await Image(modelId, new OpenAIImageCall("A cute baby sea otter") { Count = 1, Size = "1024x1024" });
        Assert.Null(JsonNode.Parse(capture.Body)!["response_format"]);
    }

    private static async Task<Dictionary<string, List<string>>> TranscriptionFields(string modelId, string? openai)
    {
        var model = new OpenAITranscriptionModel(OpenAIUpstream.Provider(new OpenAICapture()), modelId);
        var call = new OpenAITranscriptionCall(new AudioInput(new byte[] { 1, 2, 3 }, "audio/wav", "audio.wav"));
        if (openai != null)
        {
            call.ProviderOptions = OpenAIUpstream.Json("{\"openai\":" + openai + "}");
        }

        return await OpenAIUpstream.Fields(model.Build(call)).ConfigureAwait(false);
    }

    private const string UploadResponse =
        "{\"id\":\"file-abc123\",\"object\":\"file\",\"bytes\":1024,\"created_at\":1700000000,\"filename\":\"test.csv\",\"purpose\":\"assistants\",\"status\":\"processed\",\"expires_at\":null}";

    private static string PartText(OpenAICapture capture, string name)
    {
        return Encoding.UTF8.GetString(Assert.Single(capture.Parts, part => part.Name == name).Data);
    }

    private static async Task<OpenAICapture> UploadBytes(string? purpose, int? seconds)
    {
        var capture = new OpenAICapture { ResponseJson = UploadResponse };
        await new OpenAIFileStore(OpenAIUpstream.Provider(capture)).UploadAsync("data.bin", new byte[] { 1, 2, 3 }, "application/octet-stream", purpose, seconds, CancellationToken.None).ConfigureAwait(false);
        return capture;
    }

    private static async Task<OpenAICapture> UploadStream(string? fileName)
    {
        var capture = new OpenAICapture { ResponseJson = UploadResponse };
        using var data = new MemoryStream(Encoding.UTF8.GetBytes("x"));
        await new OpenAIFileStore(OpenAIUpstream.Provider(capture)).UploadAsync(data, fileName, "application/jsonl", "batch", null, CancellationToken.None).ConfigureAwait(false);
        return capture;
    }

    private static async Task<OpenAICapture> Upload(string? purpose, int? seconds)
    {
        var capture = new OpenAICapture { ResponseBytes = System.Text.Encoding.UTF8.GetBytes("{\"id\":\"file-abc\"}"), ResponseMediaType = "application/json" };
        await new OpenAIFileStore(OpenAIUpstream.Provider(capture)).UploadAsync("notes.txt", new byte[] { 1, 2, 3, 4 }, "text/plain", purpose, seconds, CancellationToken.None).ConfigureAwait(false);
        return capture;
    }
}
