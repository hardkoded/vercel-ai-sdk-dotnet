// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.OpenAI;

/// <summary>OpenAI provider settings.</summary>
public sealed class OpenAIOptions : OpenAICompatibleOptions
{
    /// <summary>Creates options aimed at <c>https://api.openai.com/v1</c>.</summary>
    public OpenAIOptions()
    {
        ProviderName = "openai";
        BaseUrl = "https://api.openai.com/v1";
        ApiKeyEnvironmentVariable = "OPENAI_API_KEY";
        SupportsEmbeddings = true;
        SupportsImages = true;
    }

    /// <summary>When true, <see cref="OpenAIProvider.LanguageModel"/> uses the Responses API.</summary>
    public bool UseResponsesApi { get; set; }
}

/// <summary>OpenAI provider: Chat Completions, Responses, embeddings, images, speech, transcription, files, and batches.</summary>
public sealed class OpenAIProvider : OpenAICompatibleProvider
{
    /// <summary>Provider id.</summary>
    public const string ProviderId = "openai";

    private readonly OpenAIOptions _openAI;

    /// <summary>Creates an OpenAI provider.</summary>
    public OpenAIProvider(HttpClient httpClient, OpenAIOptions? options = null)
        : base(options ?? new OpenAIOptions(), httpClient)
    {
        _openAI = options ?? (OpenAIOptions)Options;
    }

    /// <summary>Creates a provider. Pass a handler from tests.</summary>
    public static OpenAIProvider Create(OpenAIOptions? options = null, HttpMessageHandler? handler = null)
    {
        options ??= new OpenAIOptions();
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new OpenAIProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        if (_openAI.UseResponsesApi)
        {
            return new OpenAIResponsesLanguageModel(this, modelId);
        }

        return base.LanguageModel(modelId);
    }

    /// <inheritdoc />
    public override ISpeechModel SpeechModel(string modelId)
    {
        return new OpenAISpeechModel(this, modelId);
    }

    /// <inheritdoc />
    public override ITranscriptionModel TranscriptionModel(string modelId)
    {
        return new OpenAITranscriptionModel(this, modelId, "audio/transcriptions");
    }

    /// <inheritdoc />
    public override ISpeechTranslationModel SpeechTranslationModel(string modelId)
    {
        return new OpenAISpeechTranslationModel(this, modelId);
    }

    /// <inheritdoc />
    public override IBatchModel BatchModel()
    {
        return new OpenAIBatchModel(this);
    }

    /// <inheritdoc />
    public override IFileStore FileStore()
    {
        return new OpenAIFileStore(this);
    }

    /// <inheritdoc />
    public override ISkillStore SkillStore()
    {
        return new OpenAISkillStore(this);
    }

    /// <inheritdoc />
    public override IRealtimeModel RealtimeModel(string modelId)
    {
        return new OpenAIRealtimeModel(this, modelId);
    }
}

/// <summary>OpenAI Responses API language model.</summary>
public sealed class OpenAIResponsesLanguageModel : ILanguageModel
{
    private readonly OpenAIProvider _provider;

    /// <summary>Creates a Responses model.</summary>
    public OpenAIResponsesLanguageModel(OpenAIProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => "openai";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "responses"),
            Build(options, false).ToJsonString(),
            _provider.CreateHeaders(),
            cancellationToken).ConfigureAwait(false);
        return Parse(document.RootElement);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var data in _provider.Http.SendSseAsync(
            ApiKeys.Combine(_provider.Options.BaseUrl, "responses"),
            Build(options, true).ToJsonString(),
            _provider.CreateHeaders(),
            cancellationToken).ConfigureAwait(false))
        {
            var node = JsonNode.Parse(data) as JsonObject;
            var type = node?["type"]?.ToString().Trim('"');
            if (type == "response.output_text.delta")
            {
                var delta = node?["delta"]?.ToString().Trim('"');
                if (!string.IsNullOrEmpty(delta))
                {
                    yield return new TextDeltaStreamPart("text", delta!);
                }
            }
            else if (type == "response.completed")
            {
                yield return new FinishStreamPart(FinishReason.Stop, LanguageModelUsage.Empty);
            }
        }
    }

    private JsonObject Build(LanguageModelCallOptions options, bool stream)
    {
        var input = new JsonArray();
        foreach (var message in options.Prompt)
        {
            if (message is SystemModelMessage system)
            {
                input.Add(new JsonObject { ["role"] = "system", ["content"] = system.Content });
            }
            else if (message is UserModelMessage user)
            {
                var text = new StringBuilder();
                foreach (var part in user.Content)
                {
                    if (part is TextContentPart textPart)
                    {
                        text.Append(textPart.Text);
                    }
                }

                input.Add(new JsonObject { ["role"] = "user", ["content"] = text.ToString() });
            }
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["input"] = input,
            ["stream"] = stream,
        };
        if (options.MaxOutputTokens is { } max)
        {
            body["max_output_tokens"] = max;
        }

        if (options.Temperature is { } temperature)
        {
            body["temperature"] = temperature;
        }

        if (options.Tools is { Count: > 0 })
        {
            var tools = new JsonArray();
            foreach (var tool in options.Tools)
            {
                tools.Add(new JsonObject
                {
                    ["type"] = "function",
                    ["name"] = tool.Name,
                    ["description"] = tool.Description,
                    ["parameters"] = JsonNode.Parse(tool.InputSchema.GetRawText()),
                });
            }

            body["tools"] = tools;
        }

        return body;
    }

    private static LanguageModelGenerateResult Parse(System.Text.Json.JsonElement root)
    {
        var content = new List<GeneratedContent>();
        if (root.TryGetProperty("output", out var output))
        {
            foreach (var item in output.EnumerateArray())
            {
                var type = item.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : null;
                if (type == "message" && item.TryGetProperty("content", out var parts))
                {
                    foreach (var part in parts.EnumerateArray())
                    {
                        if (part.TryGetProperty("text", out var text))
                        {
                            content.Add(new GeneratedText(text.GetString() ?? string.Empty));
                        }
                    }
                }
                else if (type == "function_call")
                {
                    content.Add(new GeneratedToolCall(
                        item.TryGetProperty("call_id", out var id) ? id.GetString() ?? "call" : "call",
                        item.GetProperty("name").GetString() ?? string.Empty,
                        item.TryGetProperty("arguments", out var args) ? args.GetString() ?? "{}" : "{}"));
                }
            }
        }

        var usage = LanguageModelUsage.Empty;
        if (root.TryGetProperty("usage", out var usageElement))
        {
            usage = new LanguageModelUsage(
                usageElement.TryGetProperty("input_tokens", out var input) ? input.GetInt32() : null,
                usageElement.TryGetProperty("output_tokens", out var outputTokens) ? outputTokens.GetInt32() : null,
                usageElement.TryGetProperty("total_tokens", out var total) ? total.GetInt32() : null);
        }

        return new LanguageModelGenerateResult(content, content.Exists(part => part is GeneratedToolCall) ? FinishReason.ToolCalls : FinishReason.Stop, usage);
    }
}

internal sealed class OpenAISpeechModel : ISpeechModel
{
    private readonly OpenAIProvider _provider;

    public OpenAISpeechModel(OpenAIProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    public string Provider => "openai";

    public string ModelId { get; }

    public async Task<SpeechResult> DoGenerateAsync(SpeechCallOptions options, CancellationToken cancellationToken)
    {
        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["input"] = options.Text,
            ["voice"] = options.Voice ?? "alloy",
        };
        var bytes = await _provider.Http.SendBytesAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "audio/speech"),
            new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json"),
            _provider.CreateHeaders(),
            cancellationToken).ConfigureAwait(false);
        return new SpeechResult(bytes, "audio/mpeg");
    }
}

internal sealed class OpenAITranscriptionModel : ITranscriptionModel
{
    private readonly OpenAIProvider _provider;
    private readonly string _path;

    public OpenAITranscriptionModel(OpenAIProvider provider, string modelId, string path)
    {
        _provider = provider;
        ModelId = modelId;
        _path = path;
    }

    public string Provider => "openai";

    public string ModelId { get; }

    public async Task<TranscriptionResult> DoTranscribeAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        var file = new ByteArrayContent(audio.Data);
        file.Headers.ContentType = new MediaTypeHeaderValue(audio.MediaType);
        content.Add(file, "file", audio.FileName ?? "audio.mp3");
        content.Add(new StringContent(ModelId), "model");
        var bytes = await _provider.Http.SendBytesAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, _path),
            content,
            _provider.CreateHeaders(),
            cancellationToken).ConfigureAwait(false);
        using var document = System.Text.Json.JsonDocument.Parse(Encoding.UTF8.GetString(bytes));
        return new TranscriptionResult(document.RootElement.GetProperty("text").GetString() ?? string.Empty);
    }
}

internal sealed class OpenAISpeechTranslationModel : ISpeechTranslationModel
{
    private readonly OpenAITranscriptionModel _inner;

    public OpenAISpeechTranslationModel(OpenAIProvider provider, string modelId)
    {
        _inner = new OpenAITranscriptionModel(provider, modelId, "audio/translations");
        ModelId = modelId;
    }

    public string Provider => "openai";

    public string ModelId { get; }

    public Task<TranscriptionResult> DoTranslateAsync(AudioInput audio, CancellationToken cancellationToken)
    {
        return _inner.DoTranscribeAsync(audio, cancellationToken);
    }
}

internal sealed class OpenAIFileStore : IFileStore
{
    private readonly OpenAIProvider _provider;

    public OpenAIFileStore(OpenAIProvider provider)
    {
        _provider = provider;
    }

    public async Task<UploadedFile> UploadFileAsync(string fileName, byte[] data, string mediaType, CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        var file = new ByteArrayContent(data);
        file.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        content.Add(file, "file", fileName);
        content.Add(new StringContent("assistants"), "purpose");
        var bytes = await _provider.Http.SendBytesAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "files"),
            content,
            _provider.CreateHeaders(),
            cancellationToken).ConfigureAwait(false);
        using var document = System.Text.Json.JsonDocument.Parse(Encoding.UTF8.GetString(bytes));
        return new UploadedFile(document.RootElement.GetProperty("id").GetString() ?? string.Empty, fileName);
    }
}

internal sealed class OpenAISkillStore : ISkillStore
{
    private readonly OpenAIProvider _provider;

    public OpenAISkillStore(OpenAIProvider provider)
    {
        _provider = provider;
    }

    public async Task<UploadedSkill> UploadSkillAsync(string name, string instructions, CancellationToken cancellationToken)
    {
        var body = new JsonObject { ["name"] = name, ["instructions"] = instructions };
        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "skills"),
            body.ToJsonString(),
            _provider.CreateHeaders(),
            cancellationToken).ConfigureAwait(false);
        var id = document.RootElement.TryGetProperty("id", out var idElement) ? idElement.GetString() ?? name : name;
        return new UploadedSkill(id, name);
    }
}

internal sealed class OpenAIBatchModel : IBatchModel
{
    private readonly OpenAIProvider _provider;

    public OpenAIBatchModel(OpenAIProvider provider)
    {
        _provider = provider;
    }

    public string Provider => "openai";

    public async Task<BatchJob> SubmitAsync(string inputFileId, string endpoint, CancellationToken cancellationToken)
    {
        var body = new JsonObject
        {
            ["input_file_id"] = inputFileId,
            ["endpoint"] = endpoint,
            ["completion_window"] = "24h",
        };
        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "batches"),
            body.ToJsonString(),
            _provider.CreateHeaders(),
            cancellationToken).ConfigureAwait(false);
        return Read(document.RootElement);
    }

    public async Task<BatchJob> GetAsync(string id, CancellationToken cancellationToken)
    {
        using var document = await _provider.Http.SendJsonAsync(
            HttpMethod.Get,
            ApiKeys.Combine(_provider.Options.BaseUrl, "batches/" + id),
            null,
            _provider.CreateHeaders(),
            cancellationToken).ConfigureAwait(false);
        return Read(document.RootElement);
    }

    private static BatchJob Read(System.Text.Json.JsonElement root)
    {
        return new BatchJob(
            root.GetProperty("id").GetString() ?? string.Empty,
            root.TryGetProperty("status", out var status) ? status.GetString() ?? "unknown" : "unknown");
    }
}

/// <summary>OpenAI Realtime session URI builder and WebSocket client.</summary>
public sealed class OpenAIRealtimeModel : IRealtimeModel
{
    private readonly OpenAIProvider _provider;

    /// <summary>Creates a realtime model.</summary>
    public OpenAIRealtimeModel(OpenAIProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string Provider => "openai";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public Uri BuildUri()
    {
        var http = ApiKeys.Combine(_provider.Options.BaseUrl, "realtime");
        var builder = new UriBuilder(http) { Scheme = http.Scheme == "https" ? "wss" : "ws", Query = "model=" + Uri.EscapeDataString(ModelId) };
        return builder.Uri;
    }

    /// <inheritdoc />
    public Task<IRealtimeSession> ConnectAsync(CancellationToken cancellationToken)
    {
#if NETSTANDARD2_0
        throw new PlatformNotSupportedException("OpenAI Realtime WebSocket headers require .NET 10.");
#else
        return ConnectCoreAsync(cancellationToken);
#endif
    }

#if !NETSTANDARD2_0
    private async Task<IRealtimeSession> ConnectCoreAsync(CancellationToken cancellationToken)
    {
        var socket = new System.Net.WebSockets.ClientWebSocket();
        socket.Options.SetRequestHeader("Authorization", _provider.CreateHeaders()["Authorization"] ?? string.Empty);
        socket.Options.SetRequestHeader("OpenAI-Beta", "realtime=v1");
        await socket.ConnectAsync(BuildUri(), cancellationToken).ConfigureAwait(false);
        return new WebSocketRealtimeSession(socket);
    }
#endif
}

internal sealed class WebSocketRealtimeSession : IRealtimeSession
{
    private readonly System.Net.WebSockets.ClientWebSocket _socket;

    public WebSocketRealtimeSession(System.Net.WebSockets.ClientWebSocket socket)
    {
        _socket = socket;
    }

    public async IAsyncEnumerable<string> Events([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var buffer = new byte[8192];
        while (_socket.State == System.Net.WebSockets.WebSocketState.Open)
        {
            var builder = new StringBuilder();
            System.Net.WebSockets.WebSocketReceiveResult result;
            do
            {
                result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken).ConfigureAwait(false);
                if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                {
                    yield break;
                }

                builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            }
            while (!result.EndOfMessage);

            yield return builder.ToString();
        }
    }

    IAsyncEnumerable<string> IRealtimeSession.Events => Events();

    public Task SendAsync(string json, CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        return _socket.SendAsync(new ArraySegment<byte>(bytes), System.Net.WebSockets.WebSocketMessageType.Text, true, cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        _socket.Dispose();
        return default;
    }
}

/// <summary>Registers <see cref="OpenAIProvider"/>.</summary>
public static class OpenAIServiceCollectionExtensions
{
    /// <summary>Adds the OpenAI provider.</summary>
    public static IServiceCollection AddOpenAI(this IServiceCollection services, Action<OpenAIOptions>? configure = null)
    {
        services.AddHttpClient(OpenAIProvider.ProviderId);
        services.AddSingleton(sp =>
        {
            var options = new OpenAIOptions();
            configure?.Invoke(options);
            return new OpenAIProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(OpenAIProvider.ProviderId), options);
        });
        return services;
    }
}
