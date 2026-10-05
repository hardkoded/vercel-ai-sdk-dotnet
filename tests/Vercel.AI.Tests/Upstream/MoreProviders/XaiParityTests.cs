// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Xai;

namespace Vercel.AI.Tests;

/// <summary>xAI file and realtime requests matched to the upstream catalog.</summary>
public sealed class XaiParityTests
{
    private const string FileJson = "{\"id\":\"file-abc123\",\"object\":\"file\",\"bytes\":3,\"created_at\":1234567890,\"filename\":\"upload\"}";

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > uploadFile::should send a multipart POST to /v1/files", Coverage = UpstreamCoverage.Covered)]
    public async Task Upload_posts_multipart_to_files()
    {
        var (files, handler) = Files(FileJson);
        await files.UploadAsync(Bytes(new byte[] { 1, 2, 3 }), CancellationToken.None).ConfigureAwait(false);
        Assert.Single(handler.Calls);
        Assert.Equal("https://api.x.ai/v1/files", handler.Calls[0].Uri.AbsoluteUri);
        Assert.Equal(HttpMethod.Post, handler.Calls[0].Method);
        var file = Part(handler.Calls[0], "file");
        Assert.Equal(new byte[] { 1, 2, 3 }, file.Data);
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > uploadFile::should return providerReference with xai key set to id", Coverage = UpstreamCoverage.Covered)]
    public async Task Upload_returns_the_xai_file_id()
    {
        var (files, _) = Files("{\"id\":\"file-xyz789\",\"object\":\"file\",\"bytes\":3,\"created_at\":1234567890,\"filename\":\"upload\"}");
        var result = await files.UploadAsync(Bytes(new byte[] { 1, 2, 3 }), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("file-xyz789", result.ProviderReference["xai"]);
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > uploadFile::should include providerMetadata with response data", Coverage = UpstreamCoverage.Covered)]
    public async Task Upload_keeps_filename_bytes_and_created_at()
    {
        var (files, _) = Files("{\"id\":\"file-abc123\",\"object\":\"file\",\"bytes\":512,\"created_at\":1700000000,\"filename\":\"data.csv\"}");
        var result = await files.UploadAsync(Bytes(new byte[] { 1 }), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("data.csv", (string)result.Metadata.Values["filename"]);
        Assert.Equal(512L, (long)result.Metadata.Values["bytes"]);
        Assert.Equal(1700000000L, (long)result.Metadata.Values["createdAt"]);
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > uploadFile::should pass custom filename when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Upload_uses_the_supplied_filename()
    {
        var (files, handler) = Files(FileJson);
        var upload = Bytes(new byte[] { 1, 2, 3 });
        upload.FileName = "custom-name.pdf";
        await files.UploadAsync(upload, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("custom-name.pdf", Part(handler.Calls[0], "file").FileName);
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > uploadFile::should use default filename \"blob\" when not provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Upload_defaults_the_filename_to_blob()
    {
        var (files, handler) = Files(FileJson);
        await files.UploadAsync(Bytes(new byte[] { 1, 2, 3 }), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("blob", Part(handler.Calls[0], "file").FileName);
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > uploadFile::should pass teamId as team_id when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Upload_sends_team_id()
    {
        var (files, handler) = Files(FileJson);
        var upload = Bytes(new byte[] { 1 });
        upload.TeamId = "team-123";
        await files.UploadAsync(upload, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("team-123", Encoding.UTF8.GetString(Part(handler.Calls[0], "team_id").Data));
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > uploadFile::should not include team_id when not provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Upload_omits_team_id()
    {
        var (files, handler) = Files(FileJson);
        await files.UploadAsync(Bytes(new byte[] { 1 }), CancellationToken.None).ConfigureAwait(false);
        Assert.DoesNotContain(Parts(handler.Calls[0]), part => part.Name == "team_id");
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > uploadFile::should convert base64 string data to bytes", Coverage = UpstreamCoverage.Covered)]
    public async Task Upload_decodes_base64_before_sending()
    {
        var (files, handler) = Files(FileJson);
        await files.UploadAsync(new XaiFileUpload { Base64 = "dGVzdA==", MediaType = "application/octet-stream" }, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(new byte[] { 116, 101, 115, 116 }, Part(handler.Calls[0], "file").Data);
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > uploadFile::should omit null response fields from providerMetadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Upload_drops_null_metadata_fields()
    {
        var (files, _) = Files("{\"id\":\"file-abc123\",\"object\":\"file\",\"bytes\":null,\"created_at\":null,\"filename\":null}");
        var result = await files.UploadAsync(Bytes(new byte[] { 1 }), CancellationToken.None).ConfigureAwait(false);
        Assert.Empty(result.Metadata.Values);
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > uploadFile::should return empty warnings array", Coverage = UpstreamCoverage.Covered)]
    public async Task Upload_returns_no_warnings()
    {
        var (files, _) = Files(FileJson);
        var result = await files.UploadAsync(Bytes(new byte[] { 1 }), CancellationToken.None).ConfigureAwait(false);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > uploadFile::should have specificationVersion v4", Coverage = UpstreamCoverage.Covered)]
    public void Files_report_specification_version_v4()
    {
        Assert.Equal("v4", Files(FileJson).Files.SpecificationVersion);
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > uploadFile::should have the correct provider name", Coverage = UpstreamCoverage.Covered)]
    public void Files_report_the_xai_files_provider()
    {
        Assert.Equal("xai.files", Files(FileJson).Files.Provider);
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > uploadFile::should append expires_after before the file part", Coverage = UpstreamCoverage.Covered)]
    public async Task Upload_places_expires_after_before_the_file()
    {
        var (files, handler) = Files("{\"id\":\"file-abc123\",\"object\":\"file\",\"bytes\":3,\"created_at\":1234567890,\"filename\":\"upload\",\"expires_at\":1234740690}");
        var upload = Bytes(new byte[] { 1, 2, 3 });
        upload.ExpiresAfter = 172800;
        var result = await files.UploadAsync(upload, CancellationToken.None).ConfigureAwait(false);
        var names = new List<string>();
        foreach (var part in Parts(handler.Calls[0]))
        {
            names.Add(part.Name);
        }

        Assert.Equal("expires_after,file", string.Join(",", names));
        Assert.Equal("172800", Encoding.UTF8.GetString(Part(handler.Calls[0], "expires_after").Data));
        Assert.Equal(1234740690L, (long)result.Metadata.Values["expiresAt"]);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1234740690), result.ExpiresAt);
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > uploadFile::should omit expires_after when not requested", Coverage = UpstreamCoverage.Covered)]
    public async Task Upload_omits_expires_after()
    {
        var (files, handler) = Files(FileJson);
        await files.UploadAsync(Bytes(new byte[] { 1 }), CancellationToken.None).ConfigureAwait(false);
        Assert.DoesNotContain(Parts(handler.Calls[0]), part => part.Name == "expires_after");
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > uploadFile (stream data)::should stream multipart uploads with fields preceding the file part", Coverage = UpstreamCoverage.Covered)]
    public async Task Upload_writes_stream_fields_before_the_file()
    {
        var (files, handler) = Files("{\"id\":\"file-stream1\",\"object\":\"file\",\"bytes\":3,\"created_at\":1234567890,\"filename\":\"batch.jsonl\"}");
        var result = await files.UploadAsync(new XaiFileUpload
        {
            Stream = new MemoryStream(Encoding.UTF8.GetBytes("{\"a\":1}\n{\"b\":2}\n")),
            MediaType = "application/jsonl",
            FileName = "batch.jsonl",
            ExpiresAfter = 172800,
            TeamId = "team-1",
        }, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("file-stream1", result.ProviderReference["xai"]);
        Assert.Equal("https://api.x.ai/v1/files", handler.Calls[0].Uri.AbsoluteUri);
        Assert.Equal(HttpMethod.Post, handler.Calls[0].Method);
        Assert.Matches("^multipart/form-data; boundary=ai-sdk-multipart-", handler.Calls[0].Header("Content-Type") ?? string.Empty);
        var names = new List<string>();
        foreach (var part in Parts(handler.Calls[0]))
        {
            names.Add(part.Name);
        }

        Assert.Equal("expires_after,team_id,file", string.Join(",", names));
        Assert.Equal("172800", Encoding.UTF8.GetString(Part(handler.Calls[0], "expires_after").Data));
        Assert.Equal("team-1", Encoding.UTF8.GetString(Part(handler.Calls[0], "team_id").Data));
        var file = Part(handler.Calls[0], "file");
        Assert.Equal("batch.jsonl", file.FileName);
        Assert.Equal("{\"a\":1}\n{\"b\":2}\n", Encoding.UTF8.GetString(file.Data));
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > uploadFile (expiresAfter validation)::should cancel stream data when expiresAfter is invalid", Coverage = UpstreamCoverage.Covered)]
    public async Task Upload_cancels_the_stream_before_an_invalid_expiry_is_sent()
    {
        var (files, handler) = Files(FileJson);
        var cancelled = false;
        var error = await Assert.ThrowsAsync<ArgumentException>(() => files.UploadAsync(new XaiFileUpload
        {
            Stream = new MemoryStream(),
            MediaType = "application/jsonl",
            ExpiresAfter = 100,
            CancelStream = _ => cancelled = true,
        }, CancellationToken.None)).ConfigureAwait(false);
        Assert.True(cancelled);
        Assert.Empty(handler.Calls);
        Assert.False(string.IsNullOrEmpty(error.Message));
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > uploadFile (expiresAfter validation)::should reject invalid expiresAfter %j without a fetch call", Coverage = UpstreamCoverage.Covered)]
    public async Task Upload_rejects_expires_after_outside_the_accepted_window()
    {
        foreach (var expiresAfter in new double[] { 100, 0.5, 3599.5, 2592001 })
        {
            var (files, handler) = Files(FileJson);
            await Assert.ThrowsAsync<ArgumentException>(() => files.UploadAsync(new XaiFileUpload
            {
                Bytes = new byte[] { 1 },
                MediaType = "application/octet-stream",
                ExpiresAfter = expiresAfter,
            }, CancellationToken.None)).ConfigureAwait(false);
            Assert.Empty(handler.Calls);
        }
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > getFileMetadata::should retrieve file metadata via GET", Coverage = UpstreamCoverage.Covered)]
    public async Task Metadata_is_read_with_get()
    {
        var (files, handler) = Files("{\"id\":\"file-abc123\",\"object\":\"file\",\"bytes\":1024,\"created_at\":1700000000,\"expires_at\":1700172800,\"filename\":\"test.jsonl\"}");
        var result = await files.GetMetadataAsync(new Dictionary<string, string?> { ["xai"] = "file-abc123" }, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("https://api.x.ai/v1/files/file-abc123", handler.Calls[0].Uri.AbsoluteUri);
        Assert.Equal(HttpMethod.Get, handler.Calls[0].Method);
        Assert.Equal("file-abc123", result.ProviderReference["xai"]);
        Assert.Equal(1024L, result.ByteSize);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1700000000), result.CreatedAt);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1700172800), result.ExpiresAt);
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > getFileMetadata::should reject a blank xai file id (%j)", Coverage = UpstreamCoverage.Covered)]
    public async Task Metadata_rejects_a_blank_file_id()
    {
        foreach (var fileId in new[] { "", "   " })
        {
            var (files, _) = Files(FileJson);
            var error = await Assert.ThrowsAsync<ArgumentException>(() => files.GetMetadataAsync(new Dictionary<string, string?> { ["xai"] = fileId }, CancellationToken.None)).ConfigureAwait(false);
            Assert.Equal("file reference is missing an 'xai' file id.", error.Message);
        }
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > getFileMetadata::should encode a dot-segment file id (%j) so it cannot retarget the path", Coverage = UpstreamCoverage.Covered)]
    public async Task Metadata_double_encodes_dot_segments()
    {
        foreach (var pair in new[] { (".", "https://api.x.ai/v1/files/%252E"), ("..", "https://api.x.ai/v1/files/%252E%252E") })
        {
            var (files, handler) = Files(FileJson);
            await files.GetMetadataAsync(new Dictionary<string, string?> { ["xai"] = pair.Item1 }, CancellationToken.None).ConfigureAwait(false);
            Assert.Equal(pair.Item2, handler.Calls[0].Uri.AbsoluteUri);
        }
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > getFileMetadata::should reject a reference without an xai file id", Coverage = UpstreamCoverage.Covered)]
    public async Task Metadata_rejects_a_reference_that_has_no_xai_id()
    {
        var (files, _) = Files(FileJson);
        var error = await Assert.ThrowsAsync<ArgumentException>(() => files.GetMetadataAsync(new Dictionary<string, string?> { ["openai"] = "file-abc123" }, CancellationToken.None)).ConfigureAwait(false);
        Assert.Equal("file reference is missing an 'xai' file id.", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > downloadFile::should download file content as a stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Download_gets_the_content_route()
    {
        var payload = Encoding.UTF8.GetBytes("{\"result\":\"ok\"}\n");
        var handler = new ParityHandler(_ => ParityHandler.Bytes(payload, "application/octet-stream"));
        var files = Client(handler);
        var result = await files.DownloadAsync(new Dictionary<string, string?> { ["xai"] = "file-abc123" }, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("https://api.x.ai/v1/files/file-abc123/content", handler.Calls[0].Uri.AbsoluteUri);
        Assert.Equal(HttpMethod.Get, handler.Calls[0].Method);
        using var buffer = new MemoryStream();
        await result.Content.CopyToAsync(buffer).ConfigureAwait(false);
        Assert.Equal(payload, buffer.ToArray());
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > downloadFile::should expose the response content type as mediaType (parameters stripped)", Coverage = UpstreamCoverage.Covered)]
    public async Task Download_strips_content_type_parameters()
    {
        var handler = new ParityHandler(_ => ParityHandler.Bytes(Encoding.UTF8.GetBytes("{\"result\":\"ok\"}\n"), "application/jsonl; charset=utf-8"));
        var result = await Client(handler).DownloadAsync(new Dictionary<string, string?> { ["xai"] = "file-abc123" }, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("application/jsonl", result.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > downloadFile::should omit mediaType when the response has no content type", Coverage = UpstreamCoverage.Covered)]
    public async Task Download_omits_media_type_when_the_response_has_none()
    {
        var handler = new ParityHandler(_ => ParityHandler.Bytes(Encoding.UTF8.GetBytes("bytes"), null));
        var result = await Client(handler).DownloadAsync(new Dictionary<string, string?> { ["xai"] = "file-abc123" }, CancellationToken.None).ConfigureAwait(false);
        Assert.Null(result.MediaType);
    }

    [Fact]
    [UpstreamTest("packages/xai/src/files/xai-files.test.ts::XaiFiles > deleteFile::should delete a file via DELETE", Coverage = UpstreamCoverage.Covered)]
    public async Task Delete_sends_delete_and_returns_the_flag()
    {
        var (files, handler) = Files("{\"id\":\"file-abc123\",\"object\":\"file\",\"deleted\":true}");
        var result = await files.DeleteAsync(new Dictionary<string, string?> { ["xai"] = "file-abc123" }, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("https://api.x.ai/v1/files/file-abc123", handler.Calls[0].Uri.AbsoluteUri);
        Assert.Equal(HttpMethod.Delete, handler.Calls[0].Method);
        Assert.True(result.Deleted);
        Assert.Equal("file-abc123", result.ProviderReference["xai"]);
    }

    [Fact]
    [UpstreamTest("packages/xai/src/realtime/xai-realtime-model.test.ts::XaiRealtimeModel > doCreateClientSecret::includes the model as a query param on the WebSocket URL", Coverage = UpstreamCoverage.Covered)]
    public async Task Realtime_client_secret_url_includes_the_model()
    {
        var handler = new ParityHandler(_ => ParityHandler.Json("{\"value\":\"secret\",\"expires_at\":123}"));
        var model = new XaiRealtimeModel("grok-voice-latest", new HttpClient(handler, disposeHandler: false), "https://api.x.ai/v1", () => new Dictionary<string, string?> { ["Authorization"] = "Bearer test-key" });
        var result = await model.CreateClientSecretAsync(null, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("wss://api.x.ai/v1/realtime?model=grok-voice-latest", result.Url);
        Assert.Equal("https://api.x.ai/v1/realtime/client_secrets", handler.Calls[0].Uri.AbsoluteUri);
        Assert.Equal(HttpMethod.Post, handler.Calls[0].Method);
    }

    [Fact]
    [UpstreamTest("packages/xai/src/realtime/xai-realtime-model.test.ts::XaiRealtimeModel > serializeClientEvent::drops conversation-item-truncate (unsupported over WebSocket)", Coverage = UpstreamCoverage.Covered)]
    public void Realtime_drops_conversation_item_truncate()
    {
        var model = new XaiRealtimeModel("grok-voice-latest", new HttpClient(new ParityHandler(), disposeHandler: false), "https://api.x.ai/v1", null);
        Assert.Null(model.SerializeClientEvent("conversation-item-truncate"));
    }

    private static XaiFileUpload Bytes(byte[] data)
    {
        return new XaiFileUpload { Bytes = data, MediaType = "application/octet-stream" };
    }

    private static (XaiFiles Files, ParityHandler Handler) Files(string json)
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(json));
        return (Client(handler), handler);
    }

    private static XaiFiles Client(ParityHandler handler)
    {
        return new XaiFiles(new HttpClient(handler, disposeHandler: false), "https://api.x.ai/v1", () => new Dictionary<string, string?> { ["Authorization"] = "Bearer test-key" });
    }

    private static FormPart Part(ParityCall call, string name)
    {
        foreach (var part in Parts(call))
        {
            if (part.Name == name)
            {
                return part;
            }
        }

        throw new InvalidOperationException("Missing form part " + name + ".");
    }

    private static List<FormPart> Parts(ParityCall call)
    {
        var contentType = call.Header("Content-Type") ?? string.Empty;
        var marker = "boundary=";
        var start = contentType.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        var boundary = contentType.Substring(start + marker.Length).Trim().Trim('"');
        var separator = Encoding.ASCII.GetBytes("--" + boundary);
        var parts = new List<FormPart>();
        var cursor = 0;
        while (cursor < call.Body.Length)
        {
            var at = IndexOf(call.Body, separator, cursor);
            if (at < 0)
            {
                break;
            }

            var dataStart = at + separator.Length;
            if (dataStart + 1 < call.Body.Length && call.Body[dataStart] == (byte)'-' && call.Body[dataStart + 1] == (byte)'-')
            {
                break;
            }

            if (dataStart + 1 < call.Body.Length && call.Body[dataStart] == (byte)'\r')
            {
                dataStart += 2;
            }

            var next = IndexOf(call.Body, separator, dataStart);
            var end = next < 0 ? call.Body.Length : next;
            if (end >= 2 && call.Body[end - 2] == (byte)'\r')
            {
                end -= 2;
            }

            var section = new byte[end - dataStart];
            Buffer.BlockCopy(call.Body, dataStart, section, 0, section.Length);
            var split = IndexOf(section, Encoding.ASCII.GetBytes("\r\n\r\n"), 0);
            if (split >= 0)
            {
                var headers = Encoding.UTF8.GetString(section, 0, split);
                var payload = new byte[section.Length - split - 4];
                Buffer.BlockCopy(section, split + 4, payload, 0, payload.Length);
                parts.Add(new FormPart(Name(headers), FileName(headers), payload));
            }

            cursor = next < 0 ? call.Body.Length : next;
        }

        return parts;
    }

    private static string Name(string headers)
    {
        return Quoted(headers, "name=\"");
    }

    private static string? FileName(string headers)
    {
        var marker = "filename=\"";
        return headers.IndexOf(marker, StringComparison.Ordinal) < 0 ? null : Quoted(headers, marker);
    }

    private static string Quoted(string headers, string marker)
    {
        var start = headers.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        var end = headers.IndexOf('"', start);
        return headers.Substring(start, end - start);
    }

    private static int IndexOf(byte[] source, byte[] pattern, int start)
    {
        for (var index = start; index <= source.Length - pattern.Length; index++)
        {
            var match = true;
            for (var offset = 0; offset < pattern.Length; offset++)
            {
                if (source[index + offset] != pattern[offset])
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                return index;
            }
        }

        return -1;
    }

    private sealed class FormPart
    {
        public FormPart(string name, string? fileName, byte[] data)
        {
            Name = name;
            FileName = fileName;
            Data = data;
        }

        public string Name { get; }

        public string? FileName { get; }

        public byte[] Data { get; }
    }
}
