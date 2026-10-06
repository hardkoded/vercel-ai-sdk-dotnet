// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Embeddings, images, speech, transcription, files, skills, completions, and realtime client secrets.</summary>
public sealed class OpenAIMediaUpstreamTests
{
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
        var capture = new OpenAICapture { ResponseBytes = System.Text.Encoding.UTF8.GetBytes("{\"id\":\"skill_699fc58f408c8191825d8d06ae75fd5c06de7b381a5db7f5\",\"name\":\"test-capture-skill\",\"description\":\"A test skill for fixture capture\",\"latest_version\":1,\"default_version\":1,\"created_at\":1772078479}"), ResponseMediaType = "application/json" };
        var result = await new OpenAISkillStore(OpenAIUpstream.Provider(capture)).UploadAsync(new[] { new OpenAISkillFile("SKILL.md", new byte[] { 1 }, "text/markdown") }, "Title", CancellationToken.None);
        Assert.Equal("displayTitle", result.Warnings[0].Feature);
        Assert.Equal("skill_699fc58f408c8191825d8d06ae75fd5c06de7b381a5db7f5", result.Id);
        Assert.Equal(1, result.LatestVersion);
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
    [UpstreamTest("packages/openai/src/embedding/openai-embedding-model.test.ts::doEmbed::should expose the raw response headers", Coverage = UpstreamCoverage.Partial, Note = "test-header is returned. Content-length follows the scripted body, not the upstream fixture length.")]
    public async Task ExposesEmbeddingResponseHeaders()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"data\":[{\"embedding\":[0.5]}],\"usage\":{\"prompt_tokens\":1}}" };
        capture.ResponseHeaders["test-header"] = "test-value";
        var result = await new OpenAIEmbeddingModel(OpenAIUpstream.Provider(capture), "text-embedding-3-large").EmbedAsync(new[] { "hello" }, null, null, null, CancellationToken.None);
        Assert.Equal("test-value", result.Headers["test-header"]);
        Assert.Contains("application/json", result.Headers["Content-Type"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/embedding/openai-embedding-model.test.ts::doEmbed::should pass headers", Coverage = UpstreamCoverage.Partial, Note = "Authorization, organization, project, and custom headers match. The user-agent suffix is ai-sdk/openai/4.0.73.")]
    public async Task PassesEmbeddingHeaders()
    {
        var capture = new OpenAICapture { ResponseJson = "{\"data\":[{\"embedding\":[0.5]}],\"usage\":{\"prompt_tokens\":1}}" };
        var provider = StandardProvider(capture);
        await new OpenAIEmbeddingModel(provider, "text-embedding-3-large").EmbedAsync(
            new[] { "hello" },
            null,
            null,
            new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" },
            CancellationToken.None);
        OpenAIUpstream.AssertStandardHeaders(capture);
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
    [UpstreamTest("packages/openai/src/speech/openai-speech-model.test.ts::doGenerate::should pass headers", Coverage = UpstreamCoverage.Partial, Note = "Request headers match. The user-agent suffix is ai-sdk/openai/4.0.73, and content-type includes a charset.")]
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

    private static OpenAIProvider StandardProvider(OpenAICapture capture)
    {
        return OpenAIUpstream.Provider(capture, options =>
        {
            options.Organization = "test-organization";
            options.Project = "test-project";
            options.Headers["Custom-Provider-Header"] = "provider-header-value";
        });
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

    private static async Task<OpenAICapture> Upload(string? purpose, int? seconds)
    {
        var capture = new OpenAICapture { ResponseBytes = System.Text.Encoding.UTF8.GetBytes("{\"id\":\"file-abc\"}"), ResponseMediaType = "application/json" };
        await new OpenAIFileStore(OpenAIUpstream.Provider(capture)).UploadAsync("notes.txt", new byte[] { 1, 2, 3, 4 }, "text/plain", purpose, seconds, CancellationToken.None).ConfigureAwait(false);
        return capture;
    }
}
