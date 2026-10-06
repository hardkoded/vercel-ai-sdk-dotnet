// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Anthropic;

/// <summary>Anthropic Messages API settings.</summary>
public class AnthropicOptions
{
    /// <summary>API origin. The official host is normalized to <c>/v1</c>. Message calls append <c>/messages</c>.</summary>
    public string BaseUrl { get; set; } = "https://api.anthropic.com";

    /// <summary>Explicit API key. Mutually exclusive with <see cref="AuthToken"/>.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Bearer token. Mutually exclusive with <see cref="ApiKey"/>.</summary>
    public string? AuthToken { get; set; }

    /// <summary>Environment variable. Defaults to <c>ANTHROPIC_API_KEY</c>.</summary>
    public string ApiKeyEnvironmentVariable { get; set; } = "ANTHROPIC_API_KEY";

    /// <summary><c>anthropic-version</c> header.</summary>
    public string Version { get; set; } = "2023-06-01";

    /// <summary>Model provider id. Defaults to <c>anthropic.messages</c>.</summary>
    public string? Name { get; set; }

    /// <summary>Headers merged into every request.</summary>
    public Dictionary<string, string?> Headers { get; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    /// <summary>Optional rewrite of the JSON body before it is sent.</summary>
    public Func<JsonObject, JsonObject>? TransformRequestBody { get; set; }

    /// <summary>AWS region. Used when the AWS base URL is not set explicitly.</summary>
    public string? Region { get; set; }

    /// <summary>Claude Platform workspace id.</summary>
    public string? WorkspaceId { get; set; }

    /// <summary>AWS access key. Used when no Anthropic API key is configured.</summary>
    public string? AccessKeyId { get; set; }

    /// <summary>AWS secret key.</summary>
    public string? SecretAccessKey { get; set; }

    /// <summary>AWS session token.</summary>
    public string? SessionToken { get; set; }

    /// <summary>Supplies SigV4 credentials for one request.</summary>
    public Func<CancellationToken, Task<AnthropicAwsCredentials>>? CredentialProvider { get; set; }

    /// <summary>Clock used when signing. Tests pin this.</summary>
    public Func<DateTimeOffset>? UtcNow { get; set; }
}

/// <summary>Anthropic Messages provider.</summary>
public class AnthropicProvider : ProviderBase
{
    /// <summary>Provider id used for dependency injection.</summary>
    public const string ProviderName = "anthropic";

    /// <summary>User agent for the Messages API.</summary>
    public static readonly string UserAgent = "ai-sdk/anthropic/" + AiSdkVersion.Version;

    /// <summary>Creates a provider.</summary>
    public AnthropicProvider(HttpClient httpClient, AnthropicOptions? options = null)
        : base(ProviderName)
    {
        Options = options ?? new AnthropicOptions();
        if (string.IsNullOrWhiteSpace(Options.BaseUrl))
        {
            throw new ArgumentException("baseURL must be a non-empty string.", nameof(options));
        }

        if (!string.IsNullOrEmpty(Options.ApiKey) && !string.IsNullOrEmpty(Options.AuthToken))
        {
            throw new InvalidOperationException("Both apiKey and authToken were provided. Please use only one authentication method.");
        }

        Http = new ProviderHttp(httpClient ?? throw new ArgumentNullException(nameof(httpClient)));
    }

    /// <summary>Options.</summary>
    public AnthropicOptions Options { get; }

    /// <summary>HTTP helper.</summary>
    public ProviderHttp Http { get; }

    /// <summary>True for Claude Platform on AWS.</summary>
    protected bool Aws { get; set; }

    /// <summary>True when the caller supplied a base URL other than the public Anthropic host.</summary>
    protected bool ExplicitBaseUrl { get; set; }

    /// <summary>Provider id reported by language models.</summary>
    public string ModelProvider
    {
        get
        {
            if (!string.IsNullOrEmpty(Options.Name))
            {
                return Options.Name!;
            }

            return Aws ? "anthropic-aws.messages" : "anthropic.messages";
        }
    }

    /// <summary>Creates a provider.</summary>
    public static AnthropicProvider Create(AnthropicOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new AnthropicProvider(client, options);
    }

    /// <inheritdoc />
    public override ILanguageModel LanguageModel(string modelId)
    {
        if (Aws && !ExplicitBaseUrl)
        {
            ResolveRegion();
        }

        return new AnthropicLanguageModel(this, modelId);
    }

    /// <summary>Throws because the JavaScript provider function rejects <c>new</c>.</summary>
    public ILanguageModel New(string modelId)
    {
        throw new InvalidOperationException("The Anthropic model function cannot be called with the new keyword.");
    }

    /// <inheritdoc />
    public override IEmbeddingModel EmbeddingModel(string modelId)
    {
        throw new AiSdkException("no such embeddingModel: " + modelId);
    }

    /// <inheritdoc />
    public override IImageModel ImageModel(string modelId)
    {
        throw new AiSdkException("no such imageModel: " + modelId);
    }

    /// <inheritdoc />
    public override IEvaluationModel EvaluationModel(string modelId)
    {
        return new AnthropicEvaluationModel(this, modelId);
    }

    /// <summary>Files API client.</summary>
    public AnthropicFiles Files()
    {
        return new AnthropicFiles(Http, NormalizedBase, () => CreateHeaders(Array.Empty<string>(), null), ModelProvider);
    }

    /// <summary>Skills API client.</summary>
    public AnthropicSkills Skills()
    {
        return new AnthropicSkills(Http, NormalizedBase, () => CreateHeaders(Array.Empty<string>(), null), Aws ? "anthropic-aws.skills" : "anthropic.skills");
    }

    /// <summary>Messages URL for the resolved base.</summary>
    public Uri MessagesUri()
    {
        var baseUrl = NormalizedBase().TrimEnd('/');
        if (baseUrl.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
        {
            return new Uri(baseUrl + "/messages");
        }

        return new Uri(baseUrl + "/v1/messages");
    }

    /// <summary>URL for one model call. Defaults to <see cref="MessagesUri"/>.</summary>
    protected internal virtual Uri RequestUri(string modelId, bool streaming)
    {
        return MessagesUri();
    }

    /// <summary>False when the host does not accept the native output format. Structured output then uses the JSON tool.</summary>
    protected internal virtual bool SupportsNativeStructuredOutput => true;

    /// <summary>False when the host does not accept <c>strict</c> on tool definitions.</summary>
    protected internal virtual bool SupportsStrictTools => true;

    /// <summary>URL patterns that models of this provider accept for images and PDFs.</summary>
    protected internal virtual IReadOnlyDictionary<string, string> SupportedUrls { get; } = new Dictionary<string, string>
    {
        ["image/*"] = "^https?://",
        ["application/pdf"] = "^https?://",
    };

    /// <summary>Resolved API origin, including <c>/v1</c> when the host is the public Anthropic API or a regional AWS host.</summary>
    public string NormalizedBase()
    {
        var raw = Options.BaseUrl.Trim();
        if (Aws && !ExplicitBaseUrl)
        {
            raw = "https://aws-external-anthropic." + ResolveRegion() + ".api.aws/v1";
        }
        else if (!Aws && IsOfficial(raw))
        {
            var env = Environment.GetEnvironmentVariable("ANTHROPIC_BASE_URL");
            raw = string.IsNullOrWhiteSpace(env) ? "https://api.anthropic.com/v1" : env.Trim();
        }

        raw = raw.TrimEnd('/');
        if (IsOfficial(raw))
        {
            raw = "https://api.anthropic.com/v1";
        }

        return raw;
    }

    /// <summary>Headers for a Messages call that already has an API key or auth token.</summary>
    public Dictionary<string, string?> CreateHeaders(IReadOnlyList<string> betas, IReadOnlyDictionary<string, string?>? callHeaders)
    {
        return CreateHeadersAsync(betas, callHeaders, null, CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>Headers for a Messages call. Signs the body when AWS credentials are used.</summary>
    public virtual async Task<Dictionary<string, string?>> CreateHeadersAsync(IReadOnlyList<string> betas, IReadOnlyDictionary<string, string?>? callHeaders, string? body, CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in Options.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        headers["anthropic-version"] = Options.Version;
        headers["user-agent"] = Aws ? AnthropicAwsFetch.UserAgent : UserAgent;
        if (Aws)
        {
            var workspace = WorkspaceId();
            if (!ExplicitBaseUrl && string.IsNullOrEmpty(workspace))
            {
                throw new ArgumentException("workspaceId is missing. Pass it using the 'workspaceId' parameter or the ANTHROPIC_AWS_WORKSPACE_ID environment variable.");
            }

            if (!string.IsNullOrEmpty(workspace))
            {
                headers["anthropic-workspace-id"] = workspace;
            }
        }

        if (betas != null && betas.Count > 0)
        {
            headers["anthropic-beta"] = string.Join(",", betas);
        }

        if (!string.IsNullOrEmpty(Options.AuthToken))
        {
            headers["Authorization"] = "Bearer " + Options.AuthToken;
        }
        else
        {
            var apiKey = TryApiKey();
            if (apiKey != null)
            {
                headers["x-api-key"] = apiKey;
            }
            else if (Aws && body != null)
            {
                var credentials = await ResolveCredentialsAsync(cancellationToken).ConfigureAwait(false);
                var signed = AnthropicAwsFetch.Prepare(MessagesUri().AbsoluteUri, "POST", body, null, null, ToStringMap(headers), null, credentials, Options.UtcNow == null ? (DateTimeOffset?)null : Options.UtcNow());
                foreach (var pair in signed.Headers)
                {
                    headers[pair.Key] = pair.Value;
                }
            }
            else if (!Aws)
            {
                headers["x-api-key"] = ApiKeys.Require(Options.ApiKey, Options.ApiKeyEnvironmentVariable);
            }
            else
            {
                await ResolveCredentialsAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        if (callHeaders != null)
        {
            foreach (var pair in callHeaders)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        return headers;
    }

    internal static CallWarning ToCallWarning(AnthropicWarning warning)
    {
        return new CallWarning(warning.Type, warning.Details ?? warning.Message ?? warning.Feature ?? warning.Type);
    }

    private string? TryApiKey()
    {
        if (!string.IsNullOrEmpty(Options.ApiKey))
        {
            return Options.ApiKey;
        }

        var env = Environment.GetEnvironmentVariable(Options.ApiKeyEnvironmentVariable);
        return string.IsNullOrEmpty(env) ? null : env;
    }

    private string? WorkspaceId()
    {
        if (!string.IsNullOrEmpty(Options.WorkspaceId))
        {
            return Options.WorkspaceId;
        }

        var env = Environment.GetEnvironmentVariable("ANTHROPIC_AWS_WORKSPACE_ID");
        return string.IsNullOrEmpty(env) ? null : env;
    }

    private string ResolveRegion()
    {
        var region = Options.Region;
        if (string.IsNullOrEmpty(region))
        {
            region = Environment.GetEnvironmentVariable("AWS_REGION");
        }

        if (string.IsNullOrEmpty(region))
        {
            throw new ArgumentException("AWS region setting is missing. Pass it using the 'region' parameter or the AWS_REGION environment variable.");
        }

        if (!HostnameParts.IsValidHostnamePart(region))
        {
            throw new ArgumentException("region is not a valid DNS label.", nameof(region));
        }

        return region!;
    }

    private async Task<AnthropicAwsCredentials> ResolveCredentialsAsync(CancellationToken cancellationToken)
    {
        if (Options.CredentialProvider != null)
        {
            try
            {
                return await Options.CredentialProvider(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (!(exception is AiSdkException))
            {
                throw new AiSdkException("AWS credential provider failed: " + exception.Message, exception);
            }
        }

        var accessKey = string.IsNullOrEmpty(Options.AccessKeyId) ? Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") : Options.AccessKeyId;
        var secret = string.IsNullOrEmpty(Options.SecretAccessKey) ? Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") : Options.SecretAccessKey;
        var token = string.IsNullOrEmpty(Options.SessionToken) ? Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN") : Options.SessionToken;
        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secret))
        {
            throw new AiSdkException("AWS SigV4 authentication requires AWS credentials. Pass accessKeyId and secretAccessKey, or a credentialProvider.");
        }

        return new AnthropicAwsCredentials(ExplicitBaseUrl ? Options.Region ?? "us-east-1" : ResolveRegion(), accessKey!, secret!, token);
    }

    private static Dictionary<string, string?> ToStringMap(Dictionary<string, string?> headers)
    {
        return new Dictionary<string, string?>(headers, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsOfficial(string url)
    {
        var trimmed = url.Trim().TrimEnd('/');
        return trimmed.Equals("https://api.anthropic.com", StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals("https://api.anthropic.com/v1", StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>Anthropic Messages language model.</summary>
public sealed class AnthropicLanguageModel : ILanguageModel
{
    private readonly AnthropicProvider _provider;

    /// <summary>Creates a model.</summary>
    public AnthropicLanguageModel(AnthropicProvider provider, string modelId)
    {
        _provider = provider;
        ModelId = modelId;
    }

    /// <inheritdoc />
    public string SpecificationVersion => "V4";

    /// <inheritdoc />
    public string Provider => _provider.ModelProvider;

    /// <inheritdoc />
    public string ModelId { get; }

    /// <summary>URL patterns the model accepts for images and PDFs.</summary>
    public IReadOnlyDictionary<string, string> SupportedUrls => _provider.SupportedUrls;

    /// <summary>True when <paramref name="url"/> is an http or https URL for a supported media type.</summary>
    public bool SupportsUrl(string mediaType, string url)
    {
        return SupportedUrls.ContainsKey(mediaType)
            && (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("http://", StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public async Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        var prepared = Prepare(options, false);
        var body = prepared.Body.ToJsonString();
        var headers = await _provider.CreateHeadersAsync(prepared.Betas, options.Headers, body, cancellationToken).ConfigureAwait(false);
        var response = await _provider.Http.SendJsonStringAsync(HttpMethod.Post, _provider.RequestUri(ModelId, false), body, headers, cancellationToken).ConfigureAwait(false);
        var context = new AnthropicParseContext(prepared.UsesJsonResponseTool, prepared.ProviderOptionsName, prepared.UsedCustomProviderKey, Warnings(prepared));
        return AnthropicResponse.Parse(response.Body, context, response.Headers);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var prepared = Prepare(options, true);
        var body = prepared.Body.ToJsonString();
        var headers = await _provider.CreateHeadersAsync(prepared.Betas, options.Headers, body, cancellationToken).ConfigureAwait(false);
        yield return new StreamStartStreamPart(Warnings(prepared));
        var reader = new AnthropicStream(options.IncludeRawChunks, prepared.UsesJsonResponseTool);
        await foreach (var data in _provider.Http.SendSseAsync(_provider.RequestUri(ModelId, true), body, headers, cancellationToken).ConfigureAwait(false))
        {
            foreach (var part in reader.Push(data))
            {
                yield return part;
            }
        }
    }

    private AnthropicPreparedRequest Prepare(LanguageModelCallOptions options, bool stream)
    {
        var prepared = AnthropicMessagesRequest.Prepare(
            ModelId,
            options ?? new LanguageModelCallOptions(),
            stream,
            Provider,
            _provider.SupportsNativeStructuredOutput,
            _provider.SupportsStrictTools);
        if (_provider.Options.TransformRequestBody == null)
        {
            return prepared;
        }

        return new AnthropicPreparedRequest(
            _provider.Options.TransformRequestBody(prepared.Body),
            prepared.Warnings,
            prepared.Betas,
            prepared.UsesJsonResponseTool,
            prepared.ProviderOptionsName,
            prepared.UsedCustomProviderKey);
    }

    private static IReadOnlyList<CallWarning> Warnings(AnthropicPreparedRequest prepared)
    {
        var warnings = new List<CallWarning>(prepared.Warnings.Count);
        foreach (var warning in prepared.Warnings)
        {
            warnings.Add(AnthropicProvider.ToCallWarning(warning));
        }

        return warnings;
    }
}

/// <summary>Anthropic on AWS. Same Messages body, with regional hosts, workspace ids, and SigV4.</summary>
public sealed class AnthropicAwsProvider : AnthropicProvider
{
    /// <summary>Creates an Anthropic on AWS provider.</summary>
    public AnthropicAwsProvider(HttpClient httpClient, AnthropicOptions? options = null)
        : base(httpClient, options ?? new AnthropicOptions
        {
            BaseUrl = "https://aws-external-anthropic.us-east-1.api.aws",
            ApiKeyEnvironmentVariable = "ANTHROPIC_AWS_API_KEY",
        })
    {
        Aws = true;
        if (options == null)
        {
            ExplicitBaseUrl = true;
        }
        else
        {
            if (Options.ApiKeyEnvironmentVariable == "ANTHROPIC_API_KEY")
            {
                Options.ApiKeyEnvironmentVariable = "ANTHROPIC_AWS_API_KEY";
            }

            var trimmed = Options.BaseUrl.Trim().TrimEnd('/');
            ExplicitBaseUrl = !trimmed.Equals("https://api.anthropic.com", StringComparison.OrdinalIgnoreCase)
                && !trimmed.Equals("https://api.anthropic.com/v1", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>Creates a provider.</summary>
    public static new AnthropicAwsProvider Create(AnthropicOptions? options = null, HttpMessageHandler? handler = null)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        return new AnthropicAwsProvider(client, options);
    }
}

/// <summary>Registers Anthropic.</summary>
public static class AnthropicServiceCollectionExtensions
{
    /// <summary>Adds <see cref="AnthropicProvider"/>.</summary>
    public static IServiceCollection AddAnthropic(this IServiceCollection services, Action<AnthropicOptions>? configure = null)
    {
        services.AddHttpClient(AnthropicProvider.ProviderName);
        services.AddSingleton(sp =>
        {
            var options = new AnthropicOptions();
            configure?.Invoke(options);
            return new AnthropicProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient(AnthropicProvider.ProviderName), options);
        });
        return services;
    }

    /// <summary>Adds <see cref="AnthropicAwsProvider"/>.</summary>
    public static IServiceCollection AddAnthropicAws(this IServiceCollection services, Action<AnthropicOptions>? configure = null)
    {
        services.AddHttpClient("anthropic-aws");
        services.AddSingleton(sp =>
        {
            var options = new AnthropicOptions
            {
                BaseUrl = "https://aws-external-anthropic.us-east-1.api.aws",
                ApiKeyEnvironmentVariable = "ANTHROPIC_AWS_API_KEY",
            };
            configure?.Invoke(options);
            return new AnthropicAwsProvider(sp.GetRequiredService<IHttpClientFactory>().CreateClient("anthropic-aws"), options);
        });
        return services;
    }
}
