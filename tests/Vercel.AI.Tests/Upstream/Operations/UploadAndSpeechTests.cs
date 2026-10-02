// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests;

public sealed class UploadAndSpeechTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/upload-file/upload-file.test.ts::uploadFile::should pass tagged data through to files.uploadFile", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_tagged_bytes_to_the_files_api()
    {
        var store = new RecordingFiles();
        var data = new byte[] { 1, 2, 3 };
        await UploadFile.UploadFileAsync(new UploadFileRequest { Api = store, Data = new UploadData("data", data), MediaType = "application/octet-stream" });
        Assert.Equal("data", store.Call!.Data.Type);
        Assert.Same(data, store.Call.Data.Data);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/upload-file/upload-file.test.ts::uploadFile::should pass tagged base64 string data through to files.uploadFile", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_tagged_base64_string_data()
    {
        var store = new RecordingFiles();
        await UploadFile.UploadFileAsync(new UploadFileRequest { Api = store, Data = new UploadData("data", "aGVsbG8="), MediaType = "text/plain" });
        Assert.Equal("aGVsbG8=", store.Call!.Data.Data);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/upload-file/upload-file.test.ts::uploadFile::should default mediaType to text/plain for tagged text data", Coverage = UpstreamCoverage.Covered)]
    public async Task Defaults_text_uploads_to_text_plain()
    {
        var store = new RecordingFiles();
        await UploadFile.UploadFileAsync(new UploadFileRequest { Api = store, Data = new UploadData("text", "hello") });
        Assert.Equal("text/plain", store.Call!.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/upload-file/upload-file.test.ts::uploadFile::should default mediaType to application/octet-stream for stream data", Coverage = UpstreamCoverage.Covered)]
    public async Task Defaults_stream_uploads_to_octet_stream()
    {
        var store = new RecordingFiles();
        await UploadFile.UploadFileAsync(new UploadFileRequest { Api = store, Data = new UploadData("stream", new UploadStream()) });
        Assert.Equal("application/octet-stream", store.Call!.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/upload-file/upload-file.test.ts::uploadFile::should cancel stream data when the api has no files() method", Coverage = UpstreamCoverage.Covered)]
    public async Task Cancels_the_stream_when_the_provider_has_no_files_api()
    {
        var stream = new UploadStream();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => UploadFile.UploadFileAsync(new UploadFileRequest { Api = new EmptyProvider(), Data = new UploadData("stream", stream) }));
        Assert.Contains("files()", error.Message);
        Assert.Same(error, stream.CancelError);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/upload-file/upload-file.test.ts::uploadFile::should cancel stream data when the provider upload rejects", Coverage = UpstreamCoverage.Covered)]
    public async Task Cancels_the_stream_when_the_upload_rejects()
    {
        var stream = new UploadStream();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => UploadFile.UploadFileAsync(new UploadFileRequest { Api = new RecordingFiles { Fail = true }, Data = new UploadData("stream", stream) }));
        Assert.Equal("upload failed", error.Message);
        Assert.Same(error, stream.CancelError);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/upload-file/upload-file.test.ts::uploadFile::should forward abortSignal and headers to files.uploadFile", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_the_cancellation_token_and_headers()
    {
        using var source = new CancellationTokenSource();
        var store = new RecordingFiles();
        await UploadFile.UploadFileAsync(new UploadFileRequest { Api = store, Data = new byte[] { 1 }, MediaType = "application/octet-stream", CancellationToken = source.Token, Headers = new Dictionary<string, string> { ["x-test"] = "1" } });
        Assert.Equal(source.Token, store.Call!.CancellationToken);
        Assert.Equal("1", store.Call.Headers!["x-test"]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/upload-file/upload-file.test.ts::uploadFile::should pass byteSize/createdAt/expiresAt through from the provider result", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_through_size_and_timestamps()
    {
        var created = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var expires = created.AddDays(1);
        var store = new RecordingFiles { Result = new UploadFileModelResult(new ProviderFileReference("file-1"), byteSize: 12, createdAt: created, expiresAt: expires) };
        var result = await UploadFile.UploadFileAsync(new UploadFileRequest { Api = store, Data = new byte[] { 1 }, MediaType = "application/octet-stream" });
        Assert.Equal((long?)12, result.ByteSize);
        Assert.Equal((DateTime?)created, result.CreatedAt);
        Assert.Equal((DateTime?)expires, result.ExpiresAt);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/upload-file/upload-file.test.ts::uploadFile::should forward providerOptions to files.uploadFile", Coverage = UpstreamCoverage.Covered)]
    public async Task Forwards_provider_options()
    {
        var store = new RecordingFiles();
        var options = OperationJson.Parse("{\"openai\":{\"purpose\":\"assistants\"}}");
        await UploadFile.UploadFileAsync(new UploadFileRequest { Api = store, Data = new byte[] { 1 }, MediaType = "application/octet-stream", ProviderOptions = options, ProviderOptionsSpecified = true });
        Assert.Equal("assistants", store.Call!.ProviderOptions!.Value.GetProperty("openai").GetProperty("purpose").GetString());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/upload-file/upload-file.test.ts::uploadFile::should pass undefined providerOptions when not provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Leaves_provider_options_unset_when_omitted()
    {
        var store = new RecordingFiles();
        await UploadFile.UploadFileAsync(new UploadFileRequest { Api = store, Data = new byte[] { 1 }, MediaType = "application/octet-stream" });
        Assert.Null(store.Call!.ProviderOptions);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/upload-file/upload-file.test.ts::uploadFile::should pass filename through to files.uploadFile", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_the_filename()
    {
        var store = new RecordingFiles();
        await UploadFile.UploadFileAsync(new UploadFileRequest { Api = store, Data = new byte[] { 1 }, MediaType = "application/octet-stream", Filename = "notes.txt" });
        Assert.Equal("notes.txt", store.Call!.Filename);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/upload-file/upload-file.test.ts::uploadFile::should pass warnings from provider result", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_provider_warnings()
    {
        var store = new RecordingFiles { Result = new UploadFileModelResult(new ProviderFileReference("file-1"), warnings: new[] { OperationWarning.Other("check") }) };
        var result = await UploadFile.UploadFileAsync(new UploadFileRequest { Api = store, Data = new byte[] { 1 }, MediaType = "application/octet-stream" });
        Assert.Equal("check", result.Warnings[0].Message);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/upload-file/upload-file.test.ts::uploadFile::should resolve FilesV4 from ProviderV4 with files() method", Coverage = UpstreamCoverage.Covered)]
    public async Task Resolves_the_files_api_from_the_provider()
    {
        var store = new RecordingFiles();
        var result = await UploadFile.UploadFileAsync(new UploadFileRequest { Api = new FilesProvider(store), Data = new byte[] { 1 }, MediaType = "application/octet-stream" });
        Assert.Equal("file-1", result.ProviderReference.Id);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/upload-file/upload-file.test.ts::uploadFile::should throw when ProviderV4 has no files() method", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_when_the_provider_has_no_files_api()
    {
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => UploadFile.UploadFileAsync(new UploadFileRequest { Api = new EmptyProvider(), Data = new byte[] { 1 } }));
        Assert.Equal("The provider does not support file uploads. Make sure it exposes a files() method.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/upload-file/upload-file.test.ts::uploadFile::should return result without providerMetadata when not provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Omits_provider_metadata_when_the_provider_does()
    {
        var result = await UploadFile.UploadFileAsync(new UploadFileRequest { Api = new RecordingFiles(), Data = new byte[] { 1 }, MediaType = "application/octet-stream" });
        Assert.Null(result.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/upload-skill/upload-skill.test.ts::uploadSkill::should delegate to api.uploadSkill", Coverage = UpstreamCoverage.Covered)]
    public async Task Delegates_skill_upload_to_the_skills_api()
    {
        var store = new RecordingSkills();
        await UploadSkill.UploadSkillAsync(new UploadSkillRequest { Api = store, Files = new[] { new SkillFile("SKILL.md", new byte[] { 1 }) }, DisplayTitle = "Demo" });
        Assert.Equal("Demo", store.Call!.DisplayTitle);
        Assert.Equal("data", store.Call.Files[0].Data.Type);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/upload-skill/upload-skill.test.ts::uploadSkill::should return providerReference and warnings from the skills", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_skill_reference_and_warnings()
    {
        var store = new RecordingSkills { Result = new UploadSkillResult(new ProviderFileReference("skill-1"), new[] { OperationWarning.Other("named") }) };
        var result = await UploadSkill.UploadSkillAsync(new UploadSkillRequest { Api = store, Files = new[] { new SkillFile("SKILL.md", "hello") } });
        Assert.Equal("skill-1", result.ProviderReference.Id);
        Assert.Equal("named", result.Warnings[0].Message);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/upload-skill/upload-skill.test.ts::uploadSkill::should resolve SkillsV4 from ProviderV4 with skills() method", Coverage = UpstreamCoverage.Covered)]
    public async Task Resolves_the_skills_api_from_the_provider()
    {
        var store = new RecordingSkills();
        var result = await UploadSkill.UploadSkillAsync(new UploadSkillRequest { Api = new SkillsProvider(store), Files = new[] { new SkillFile("SKILL.md", new byte[] { 1 }) } });
        Assert.Equal("skill-1", result.ProviderReference.Id);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/upload-skill/upload-skill.test.ts::uploadSkill::should throw when ProviderV4 has no skills() method", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_when_the_provider_has_no_skills_api()
    {
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => UploadSkill.UploadSkillAsync(new UploadSkillRequest { Api = new EmptyProvider(), Files = Array.Empty<SkillFile>() }));
        Assert.Equal("The provider does not support skills. Make sure it exposes a skills() method.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/upload-skill/upload-skill.test.ts::uploadSkill::should pass providerOptions to the skills", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_skill_provider_options()
    {
        var store = new RecordingSkills();
        var options = OperationJson.Parse("{\"anthropic\":{\"display\":true}}");
        await UploadSkill.UploadSkillAsync(new UploadSkillRequest { Api = store, Files = new[] { new SkillFile("SKILL.md", new byte[] { 1 }) }, ProviderOptions = options });
        Assert.True(store.Call!.ProviderOptions!.Value.GetProperty("anthropic").GetProperty("display").GetBoolean());
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-speech/generate-speech.test.ts::generateSpeech::should send args to doGenerate", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_speech_arguments_with_a_user_agent()
    {
        var model = new SpeechFake();
        await GenerateSpeech.GenerateSpeechAsync(new GenerateSpeechRequest
        {
            Model = model,
            Text = "Hello",
            Voice = "alloy",
            OutputFormat = "mp3",
            Instructions = "slow",
            Speed = 0.8,
            Language = "en",
            Headers = new Dictionary<string, string> { ["X-Test"] = "yes" },
            ProviderOptions = OperationJson.Parse("{\"openai\":{}}"),
        });
        Assert.Equal("Hello", model.Call!.Text);
        Assert.Equal("alloy", model.Call.Voice);
        Assert.Equal((double?)0.8, model.Call.Speed);
        Assert.Contains("ai/0.0.0-test", model.Call.Headers["user-agent"]);
        Assert.Equal("yes", model.Call.Headers["x-test"]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-speech/generate-speech.test.ts::generateSpeech::should return warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_speech_warnings()
    {
        var result = await GenerateSpeech.GenerateSpeechAsync(new GenerateSpeechRequest { Model = new SpeechFake { Warnings = new[] { OperationWarning.Other("listen") } }, Text = "Hello" });
        Assert.Equal("listen", result.Warnings[0].Message);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-speech/generate-speech.test.ts::generateSpeech::should call logWarnings with the correct warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Logs_speech_warnings()
    {
        var seen = new List<WarningLogContext>();
        WarningLog.Observer = context => seen.Add(context);
        try
        {
            await GenerateSpeech.GenerateSpeechAsync(new GenerateSpeechRequest { Model = new SpeechFake { Warnings = new[] { OperationWarning.Other("listen") } }, Text = "Hello" });
        }
        finally
        {
            WarningLog.Observer = null;
        }

        Assert.Equal("listen", seen[0].Warnings[0].Message);
        Assert.Equal("mock-provider", seen[0].Provider);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-speech/generate-speech.test.ts::generateSpeech::should call logWarnings with empty array when no warnings are present", Coverage = UpstreamCoverage.Covered)]
    public async Task Logs_an_empty_speech_warning_list()
    {
        var seen = new List<int>();
        WarningLog.Observer = context => seen.Add(context.Warnings.Count);
        try
        {
            await GenerateSpeech.GenerateSpeechAsync(new GenerateSpeechRequest { Model = new SpeechFake(), Text = "Hello" });
        }
        finally
        {
            WarningLog.Observer = null;
        }

        Assert.Equal(new[] { 0 }, seen);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-speech/generate-speech.test.ts::generateSpeech::should return the audio data", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_mp3_audio_bytes()
    {
        var audio = new byte[] { 1, 2, 3, 4 };
        var result = await GenerateSpeech.GenerateSpeechAsync(new GenerateSpeechRequest { Model = new SpeechFake { Audio = audio }, Text = "Hello" });
        Assert.Equal(audio, result.Audio.Data);
        Assert.Equal("audio/mp3", result.Audio.MediaType);
        Assert.Equal("mp3", result.Audio.Format);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-speech/generate-speech.test.ts::generateSpeech::should return ADTS AAC audio with AAC metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Detects_adts_aac_audio()
    {
        var audio = new byte[] { 0xFF, 0xF1, 0, 0 };
        var result = await GenerateSpeech.GenerateSpeechAsync(new GenerateSpeechRequest { Model = new SpeechFake { Audio = audio }, Text = "Hello" });
        Assert.Equal("audio/aac", result.Audio.MediaType);
        Assert.Equal("aac", result.Audio.Format);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-speech/generate-speech.test.ts::generateSpeech > audio metadata::should identify headerless PCM returned as $label from the requested format", Coverage = UpstreamCoverage.Covered)]
    public async Task Identifies_headerless_pcm_from_the_requested_format()
    {
        foreach (var format in new[] { "pcm", "audio/pcm" })
        {
            var result = await GenerateSpeech.GenerateSpeechAsync(new GenerateSpeechRequest { Model = new SpeechFake { Audio = new byte[] { 1, 2 } }, Text = "Hello", OutputFormat = format });
            Assert.Equal("audio/pcm", result.Audio.MediaType);
            Assert.Equal("pcm", result.Audio.Format);
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-speech/generate-speech.test.ts::generateSpeech > audio metadata::should identify headerless %s audio", Coverage = UpstreamCoverage.Covered)]
    public async Task Identifies_headerless_mulaw_alaw_and_l16_audio()
    {
        foreach (var pair in new[] { ("mulaw", "audio/mulaw"), ("audio/mulaw", "audio/mulaw"), ("alaw", "audio/alaw"), ("audio/l16", "audio/l16") })
        {
            var result = await GenerateSpeech.GenerateSpeechAsync(new GenerateSpeechRequest { Model = new SpeechFake { Audio = new byte[] { 1 } }, Text = "Hello", OutputFormat = pair.Item1 });
            Assert.Equal(pair.Item2, result.Audio.MediaType);
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-speech/generate-speech.test.ts::generateSpeech > audio metadata::should identify headerless audio from the response content type", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_the_audio_content_type_header()
    {
        var result = await GenerateSpeech.GenerateSpeechAsync(new GenerateSpeechRequest
        {
            Model = new SpeechFake { Audio = new byte[] { 1 }, Headers = new Dictionary<string, string> { ["Content-Type"] = "audio/wav; charset=binary" } },
            Text = "Hello",
        });
        Assert.Equal("audio/wav", result.Audio.MediaType);
        Assert.Equal("wav", result.Audio.Format);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-speech/generate-speech.test.ts::generateSpeech > audio metadata::should prefer the detected format over response and request metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Prefers_a_detected_audio_signature()
    {
        var result = await GenerateSpeech.GenerateSpeechAsync(new GenerateSpeechRequest
        {
            Model = new SpeechFake { Audio = new byte[] { 0xFF, 0xFB, 0, 0 }, Headers = new Dictionary<string, string> { ["content-type"] = "audio/wav" } },
            Text = "Hello",
            OutputFormat = "pcm",
        });
        Assert.Equal("audio/mpeg", result.Audio.MediaType);
        Assert.Equal("mp3", result.Audio.Format);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-speech/generate-speech.test.ts::generateSpeech > error handling::should throw NoSpeechGeneratedError when no audio is returned", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_when_no_speech_audio_is_returned()
    {
        var error = await Assert.ThrowsAsync<NoSpeechGeneratedException>(() => GenerateSpeech.GenerateSpeechAsync(new GenerateSpeechRequest { Model = new SpeechFake { Audio = Array.Empty<byte>() }, Text = "Hello", MaxRetries = 0 }));
        Assert.Equal("No speech audio generated.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-speech/generate-speech.test.ts::generateSpeech > error handling::should include response headers in error when no audio generated", Coverage = UpstreamCoverage.Covered)]
    public async Task Includes_response_headers_when_speech_is_empty()
    {
        var error = await Assert.ThrowsAsync<NoSpeechGeneratedException>(() => GenerateSpeech.GenerateSpeechAsync(new GenerateSpeechRequest
        {
            Model = new SpeechFake { Audio = Array.Empty<byte>(), Headers = new Dictionary<string, string> { ["x-request-id"] = "req" } },
            Text = "Hello",
            MaxRetries = 0,
        }));
        Assert.Equal("req", error.Responses[0].Headers!["x-request-id"]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-speech/generate-speech.test.ts::generateSpeech::should return response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_speech_response_metadata()
    {
        var stamp = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var result = await GenerateSpeech.GenerateSpeechAsync(new GenerateSpeechRequest { Model = new SpeechFake { Timestamp = stamp, ModelIdOnResponse = "speech-model" }, Text = "Hello" });
        Assert.Equal(stamp, result.Responses[0].Timestamp);
        Assert.Equal("speech-model", result.Responses[0].ModelId);
    }

    private sealed class EmptyProvider : IFileStoreProvider, ISkillStoreProvider
    {
        public IFileStore? Files()
        {
            return null;
        }

        public ISkillStore? Skills()
        {
            return null;
        }
    }

    private sealed class FilesProvider : IFileStoreProvider
    {
        private readonly IFileStore _store;

        public FilesProvider(IFileStore store)
        {
            _store = store;
        }

        public IFileStore? Files()
        {
            return _store;
        }
    }

    private sealed class SkillsProvider : ISkillStoreProvider
    {
        private readonly ISkillStore _store;

        public SkillsProvider(ISkillStore store)
        {
            _store = store;
        }

        public ISkillStore? Skills()
        {
            return _store;
        }
    }

    private sealed class RecordingFiles : IFileStore
    {
        public UploadFileCall? Call { get; private set; }

        public bool Fail { get; set; }

        public UploadFileModelResult Result { get; set; } = new UploadFileModelResult(new ProviderFileReference("file-1"));

        public Task<UploadFileModelResult> UploadFileAsync(UploadFileCall call, CancellationToken cancellationToken)
        {
            Call = call;
            if (Fail)
            {
                throw new InvalidOperationException("upload failed");
            }

            return Task.FromResult(Result);
        }
    }

    private sealed class RecordingSkills : ISkillStore
    {
        public UploadSkillCall? Call { get; private set; }

        public UploadSkillResult Result { get; set; } = new UploadSkillResult(new ProviderFileReference("skill-1"));

        public Task<UploadSkillResult> UploadSkillAsync(UploadSkillCall call, CancellationToken cancellationToken)
        {
            Call = call;
            return Task.FromResult(Result);
        }
    }

    private sealed class SpeechFake : ISpeechCaller
    {
        public byte[] Audio { get; set; } = new byte[] { 9 };

        public IReadOnlyList<OperationWarning> Warnings { get; set; } = Array.Empty<OperationWarning>();

        public IReadOnlyDictionary<string, string>? Headers { get; set; }

        public DateTime? Timestamp { get; set; }

        public string? ModelIdOnResponse { get; set; }

        public SpeechModelCall? Call { get; private set; }

        public string Provider => "mock-provider";

        public string ModelId => "mock-model-id";

        public Task<SpeechModelResult> DoGenerateAsync(SpeechModelCall call, CancellationToken cancellationToken)
        {
            Call = call;
            return Task.FromResult(new SpeechModelResult(Audio, Warnings, response: new ProviderResponse(Headers, timestamp: Timestamp, modelId: ModelIdOnResponse)));
        }
    }
}
