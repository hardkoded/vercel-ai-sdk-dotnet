// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Vercel.AI.Fal;
using Vercel.AI.Mcp;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.GuardedFetch;

/// <summary>Port of <c>guarded-fetch.test.ts</c> &gt; <c>AI SDK with standalone guarded-fetch</c>.</summary>
public sealed class AiSdkWithStandaloneGuardedFetchTests
{
    private const string Prefix =
        "examples/ai-functions/src/reproduction/guarded-fetch.test.ts::AI SDK with standalone guarded-fetch::";

    private static readonly Regex UrlLiteral = new("\"(https?://[^\"]+)\"", RegexOptions.Compiled);

    // Each answer is "address" (family taken from the text) or "address/family" like upstream's { address, family }.
    public static TheoryData<string[]> UnsafeAddresses =>
        new()
        {
            { ["127.0.0.1"] },
            { ["8.8.8.8", "169.254.169.254"] },
            { ["8.8.8.8", "::1"] },
            { ["::1"] },
            { [] },
            { ["8.8.8.8/6"] },
        };

    private static Func<string, CancellationToken, Task<(string Address, int Family)[]>> ResolveTo(
        params string[] addresses
    )
    {
        return ResolveTo(null, addresses);
    }

    private static Func<string, CancellationToken, Task<(string Address, int Family)[]>> ResolveTo(
        List<string>? hosts,
        params string[] addresses
    )
    {
        var answers = addresses
            .Select(entry =>
            {
                var parts = entry.Split('/');
                var family =
                    parts.Length > 1 ? int.Parse(parts[1])
                    : parts[0].Contains(':') ? 6
                    : 4;
                return (parts[0], family);
            })
            .ToArray();
        return (host, _) =>
        {
            hosts?.Add(host);
            return Task.FromResult(answers);
        };
    }

    private static async Task<byte[]> GenerateFalImageAsync(
        string providerOrigin,
        Vercel.AI.Util.GuardedFetch fetch
    )
    {
        var provider = new FalProvider(
            fetch.Client,
            new OpenAICompatibleOptions
            {
                BaseUrl = providerOrigin,
                ApiKey = "test-secret",
                DownloadFetch = fetch,
            }
        );
        var result = await provider
            .ImageModel("model")
            .DoGenerateAsync(new ImageCallOptions("test"), CancellationToken.None);
        return Assert.Single(result.Images).Data!;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves every IP literal block in the SDK URL validation corpus",
        Coverage = UpstreamCoverage.Covered,
        Commit = "2ebc78b65435926718879f85a1a85687b053c542",
        Note = "The corpus is the C# port of validate-download-url.test.ts in this repository."
    )]
    public void Preserves_every_IP_literal_block_in_the_SDK_URL_validation_corpus()
    {
        var directory = AppContext.BaseDirectory;
        while (!Directory.Exists(Path.Combine(directory, "tests", "Vercel.AI.Tests")))
        {
            directory = Path.GetDirectoryName(directory)!;
        }

        var compared = 0;
        foreach (
            var file in Directory.EnumerateFiles(
                Path.Combine(
                    directory,
                    "tests",
                    "Vercel.AI.Tests",
                    "Upstream",
                    "ProviderUtils",
                    "ValidateDownloadUrl"
                ),
                "*.cs"
            )
        )
        {
            foreach (Match match in UrlLiteral.Matches(File.ReadAllText(file)))
            {
                var address = new Uri(match.Groups[1].Value).Host.Trim('[', ']');
                if (!IPAddress.TryParse(address, out _))
                {
                    continue;
                }

                var blocked = false;
                try
                {
                    Download.ValidateDownloadUrl(match.Groups[1].Value);
                }
                catch (DownloadError)
                {
                    blocked = true;
                }

                if (blocked)
                {
                    Assert.False(GuardedFetch_IsSafe(address), address);
                    compared++;
                }
            }
        }

        Assert.True(compared > 40, "compared " + compared);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves inline data URL downloads",
        Coverage = UpstreamCoverage.Covered,
        Commit = "2ebc78b65435926718879f85a1a85687b053c542"
    )]
    public async Task Preserves_inline_data_URL_downloads()
    {
        using var fetch = new Vercel.AI.Util.GuardedFetch();
        var result = await Download
            .CreateDownload(fetch: fetch)
            .InvokeAsync(
                new DownloadRequestItem(new Uri("data:text/plain;base64,aGVsbG8="), false)
            );
        Assert.Equal("hello", Encoding.UTF8.GetString(result.Data));
    }

    [Theory]
    [MemberData(nameof(UnsafeAddresses))]
    [UpstreamTest(
        Prefix + "blocks unsafe DNS results before core downloads connect: $addresses",
        Coverage = UpstreamCoverage.Partial,
        Commit = "2ebc78b65435926718879f85a1a85687b053c542",
        Note = "Missing: the resolve4/resolve6 call assertions; the injected resolver replaces c-ares, so the test asserts the single resolver call for the host instead."
    )]
    public async Task Blocks_unsafe_DNS_results_before_core_downloads_connect(string[] addresses)
    {
        using var server = new LocalServer(_ => (200, null, "content"));
        var hosts = new List<string>();
        using var fetch = new Vercel.AI.Util.GuardedFetch(null, ResolveTo(hosts, addresses));
        var download = Download.CreateDownload(fetch: fetch);
        await Assert.ThrowsAnyAsync<Exception>(() =>
            download.InvokeAsync(
                new DownloadRequestItem(
                    new Uri($"http://files.example.com:{server.Port}/file"),
                    false
                )
            )
        );
        Assert.Equal("files.example.com", Assert.Single(hosts));
        Assert.Empty(server.Received);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "uses the injected transport for core downloads independently of global fetch",
        Coverage = UpstreamCoverage.Covered,
        Commit = "2ebc78b65435926718879f85a1a85687b053c542",
        Note = "A .NET download never uses a global fetch; it uses the injected DownloadFetch or the default fetch."
    )]
    public async Task Uses_the_injected_transport_for_core_downloads_independently_of_global_fetch()
    {
        using var server = new LocalServer(_ => (200, null, "content"));
        var fileOrigin = $"http://files.example.com:{server.Port}";
        // These exact origins are trusted solely to reach this test's local server.
        using var fetch = new Vercel.AI.Util.GuardedFetch([fileOrigin], ResolveTo("127.0.0.1"));
        var globalCalls = 0;
        var previous = Download.Fetch;
        Download.Fetch = delegate
        {
            globalCalls++;
            throw new InvalidOperationException("Global fetch must not run");
        };
        try
        {
            var result = await Download
                .CreateDownload(fetch: fetch)
                .InvokeAsync(new DownloadRequestItem(new Uri(fileOrigin + "/file"), false));
            Assert.Equal("content", Encoding.UTF8.GetString(result.Data));
            Assert.Equal(0, globalCalls);
        }
        finally
        {
            Download.Fetch = previous;
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps SDK redirect credential isolation across origins and on return",
        Coverage = UpstreamCoverage.Covered,
        Commit = "2ebc78b65435926718879f85a1a85687b053c542"
    )]
    public async Task Keeps_SDK_redirect_credential_isolation_across_origins_and_on_return()
    {
        LocalServer? holder = null;
        using var server = holder = new LocalServer(path =>
            path switch
            {
                "/start" => (302, $"http://files.example.com:{holder!.Port}/foreign", ""),
                "/foreign" => (302, $"http://provider.example.com:{holder!.Port}/end", ""),
                _ => (200, null, "done"),
            }
        );
        var providerOrigin = $"http://provider.example.com:{server.Port}";
        var fileOrigin = $"http://files.example.com:{server.Port}";
        using var fetch = new Vercel.AI.Util.GuardedFetch(
            [providerOrigin, fileOrigin],
            ResolveTo("127.0.0.1")
        );
        var headers = new HeaderCollection();
        headers.Add("authorization", "Bearer test-secret");
        headers.Add("x-vendor-key", "test-secret");
        headers.Add("user-agent", "test-client");

        var response = await Download.FetchWithValidatedRedirectsAsync(
            providerOrigin + "/start",
            headers,
            fetch: fetch,
            trustedOrigin: providerOrigin
        );

        Assert.Equal("done", Encoding.UTF8.GetString(response.Body!));
        Assert.Equal("Bearer test-secret", server.Received[0].Headers["authorization"]);
        Assert.Equal("test-secret", server.Received[0].Headers["x-vendor-key"]);
        Assert.Equal(3, server.Received.Count);
        foreach (var (_, received) in server.Received.Skip(1))
        {
            Assert.False(received.ContainsKey("authorization"));
            Assert.False(received.ContainsKey("x-vendor-key"));
            Assert.Equal("test-client", received["user-agent"]);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "withholds custom credentials from an untrusted first hop",
        Coverage = UpstreamCoverage.Covered,
        Commit = "2ebc78b65435926718879f85a1a85687b053c542"
    )]
    public async Task Withholds_custom_credentials_from_an_untrusted_first_hop()
    {
        using var server = new LocalServer(_ => (200, null, "content"));
        var providerOrigin = $"http://provider.example.com:{server.Port}";
        var fileOrigin = $"http://files.example.com:{server.Port}";
        using var fetch = new Vercel.AI.Util.GuardedFetch([fileOrigin], ResolveTo("127.0.0.1"));
        var headers = new HeaderCollection();
        headers.Add("authorization", "test-secret");
        headers.Add("x-vendor-key", "test-secret");
        headers.Add("accept", "text/plain");

        var response = await Download.FetchUntrustedUrlAsync(
            fileOrigin + "/file",
            headers,
            credentialedOrigin: providerOrigin,
            fetch: fetch
        );

        Assert.Equal("content", Encoding.UTF8.GetString(response.Body!));
        var received = server.Received[0].Headers;
        Assert.Equal("text/plain", received["accept"]);
        Assert.False(received.ContainsKey("authorization"));
        Assert.False(received.ContainsKey("x-vendor-key"));
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "uses the same transport for Fal generation and response URL downloads without disclosing its API key",
        Coverage = UpstreamCoverage.Covered,
        Commit = "2ebc78b65435926718879f85a1a85687b053c542",
        Note = "The port's Fal provider takes the transport as the HttpClient of its API calls (GuardedFetch.Client) and as DownloadFetch for the response URL."
    )]
    public async Task Uses_the_same_transport_for_Fal_generation_and_response_URL_downloads_without_disclosing_its_API_key()
    {
        var image = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+a7mUAAAAASUVORK5CYII="
        );
        LocalServer? holder = null;
        using var server = holder = new LocalServer(request =>
            request.Path == "/model"
                ? new LocalResponse(
                    200,
                    Body: $"{{\"images\":[{{\"url\":\"http://files.example.com:{holder!.Port}/image\",\"content_type\":\"image/png\"}}]}}",
                    ContentType: "application/json"
                )
                : new LocalResponse(200, ContentType: "image/png", Bytes: image)
        );
        var providerOrigin = $"http://provider.example.com:{server.Port}";
        var fileOrigin = $"http://files.example.com:{server.Port}";
        using var fetch = new Vercel.AI.Util.GuardedFetch(
            [providerOrigin, fileOrigin],
            ResolveTo("127.0.0.1")
        );

        var result = await GenerateFalImageAsync(providerOrigin, fetch);

        Assert.Equal(image, result);
        Assert.Equal("Key test-secret", server.Received[0].Headers["authorization"]);
        Assert.False(server.Received[1].Headers.ContainsKey("authorization"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "initializes MCP and lists tools through real HTTP event streams",
        Coverage = UpstreamCoverage.Covered,
        Commit = "2ebc78b65435926718879f85a1a85687b053c542",
        Note = "The port's HTTP MCP transport takes the transport as its HttpClient (GuardedFetch.Client)."
    )]
    public async Task Initializes_MCP_and_lists_tools_through_real_HTTP_event_streams()
    {
        using var server = new LocalServer(request =>
        {
            if (request.Method != "POST")
            {
                return new LocalResponse(405);
            }

            using var message = JsonDocument.Parse(request.Body);
            if (!message.RootElement.TryGetProperty("id", out var id))
            {
                return new LocalResponse(202);
            }

            var result =
                message.RootElement.GetProperty("method").GetString() == "initialize"
                    ? "{\"protocolVersion\":\"2025-11-25\",\"capabilities\":{\"tools\":{}},\"serverInfo\":{\"name\":\"local\",\"version\":\"1\"}}"
                    : "{\"tools\":[{\"name\":\"echo\",\"description\":\"Echo\",\"inputSchema\":{\"type\":\"object\",\"properties\":{}}}]}";
            return new LocalResponse(
                200,
                Body: $"data: {{\"jsonrpc\":\"2.0\",\"id\":{id.GetRawText()},\"result\":{result}}}\n\n",
                ContentType: "text/event-stream"
            );
        });
        var providerOrigin = $"http://provider.example.com:{server.Port}";
        using var fetch = new Vercel.AI.Util.GuardedFetch([providerOrigin], ResolveTo("127.0.0.1"));

        var client = await McpClient.CreateAsync(
            new HttpMcpTransport(fetch.Client, new Uri(providerOrigin + "/mcp")),
            new McpClientOptions { ProtocolVersionDiscovery = false }
        );

        var tools = await client.ListToolsAsync();
        Assert.Contains(tools, tool => tool.Name == "echo");
    }

    private static bool GuardedFetch_IsSafe(string address)
    {
        return Vercel.AI.Util.GuardedFetch.IsSafeIpAddress(address);
    }
}
