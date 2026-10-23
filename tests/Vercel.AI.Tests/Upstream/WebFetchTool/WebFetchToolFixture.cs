// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using Vercel.AI.WebFetch;

namespace Vercel.AI.Tests.Upstream.WebFetchTool;

/// <summary>Helpers shared by the ports of <c>web-fetch-tool.test.ts</c>.</summary>
internal static class WebFetchToolFixture
{
    public const string FilePath = "packages/tool-web-fetch/src/web-fetch-tool.test.ts";

    public const string ArticleHtml = """
        <!doctype html><html><head><title>Test Page | Example</title></head><body>
          <nav><a href="/">Home</a><a href="/about">About</a></nav>
          <main>
            <article>
              <h1>Test Page</h1>
              <p>The quick brown fox jumps over the lazy dog. This paragraph contains
              enough words to be recognized as the main content of the page by the
              content extractor, which removes navigation and footer elements.</p>
              <p>Read the <a href="/docs/guide">guide</a> for more details about the
              fox, the dog, and their long-standing rivalry in the meadow.</p>
            </article>
          </main>
          <footer>Copyright Example Inc.</footer>
        </body></html>
        """;

    public static string Html(string body, string head = "<title>Test Page | Example</title>")
    {
        return "<!doctype html><html><head>" + head + "</head><body>" + body + "</body></html>";
    }

    public static DownloadResponse HtmlResponse(string html, string contentType = "text/html")
    {
        return DownloadResponse.Bytes(Encoding.UTF8.GetBytes(html), contentType);
    }

    public static DownloadResponse Redirect(int status, string location)
    {
        var headers = new HeaderCollection();
        headers.Add("location", location);
        return new DownloadResponse(status, "Redirect", headers, null);
    }

    /// <summary>A fetch that records its calls, like <c>vi.fn</c>.</summary>
    public sealed class RecordingFetch
    {
        private readonly Func<string, DownloadRequest, Task<DownloadResponse>> _handler;

        public RecordingFetch(Func<string, DownloadRequest, DownloadResponse> handler)
            : this((url, request) => Task.FromResult(handler(url, request)))
        {
        }

        public RecordingFetch(Func<string, DownloadRequest, Task<DownloadResponse>> handler)
        {
            _handler = handler;
        }

        public List<(string Url, DownloadRequest Request)> Calls { get; } = new();

        public DownloadFetch Fetch => (url, request) =>
        {
            lock (Calls)
            {
                Calls.Add((url, request));
            }

            return _handler(url, request);
        };
    }

    public static async Task<JsonElement> ExecuteAsync(Tool webFetch, string url, int? offset = null, IReadOnlyList<ModelMessage>? messages = null, CancellationToken abortSignal = default)
    {
        var input = offset == null
            ? JsonSerializer.Serialize(new { url })
            : JsonSerializer.Serialize(new { url, offset });
        using var document = JsonDocument.Parse(input);
        var output = await webFetch.ExecuteWithOptions!(
            document.RootElement.Clone(),
            new ToolExecutionOptions("test-call", messages ?? Array.Empty<ModelMessage>(), abortSignal));
        using var parsed = JsonDocument.Parse(output);
        return parsed.RootElement.Clone();
    }

    public static Vercel.AI.Prompt.ToolModelOutput ToModelOutput(Tool webFetch, JsonElement output)
    {
        return webFetch.ToModelOutput!(new Vercel.AI.Prompt.ToolModelOutputCall("test-call", null, output.GetRawText()));
    }

    public static string Content(JsonElement output)
    {
        return output.GetProperty("content").GetString()!;
    }

    public static bool IsDocument(JsonElement output)
    {
        return output.GetProperty("type").GetString() == "document";
    }

    public static string ErrorCode(JsonElement output)
    {
        Assert.Equal("error", output.GetProperty("type").GetString());
        return output.GetProperty("errorCode").GetString()!;
    }
}
