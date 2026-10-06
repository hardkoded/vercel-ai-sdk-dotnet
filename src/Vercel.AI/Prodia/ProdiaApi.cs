// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Operations;

namespace Vercel.AI.Prodia;

/// <summary>One part of a multipart response.</summary>
internal sealed class ProdiaMultipartPart
{
    public ProdiaMultipartPart(IReadOnlyDictionary<string, string> headers, byte[] body)
    {
        Headers = headers;
        Body = body;
    }

    /// <summary>Part headers with lower-case names.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    public byte[] Body { get; }

    public string ContentDisposition => Headers.TryGetValue("content-disposition", out var value) ? value : string.Empty;

    public string ContentType => Headers.TryGetValue("content-type", out var value) ? value : string.Empty;
}

/// <summary>Job submission and multipart response parsing shared by the Prodia models.</summary>
internal static class ProdiaApi
{
    private static readonly Regex Boundary = new Regex("boundary=([^\\s;]+)", RegexOptions.CultureInvariant);

    /// <summary>Posts a job to <c>/job?price=true</c> and splits the multipart response.</summary>
    public static async Task<(IReadOnlyList<ProdiaMultipartPart> Parts, IReadOnlyDictionary<string, string> Headers)> PostJobAsync(
        ProdiaProvider provider,
        HttpContent content,
        string accept,
        IEnumerable<KeyValuePair<string, string>>? callHeaders,
        CancellationToken cancellationToken)
    {
        var headers = ProviderExchange.Merge(provider.CreateHeaders(), callHeaders);
        headers["Accept"] = accept;
        var response = await ProviderExchange.SendAsync(provider.HttpClient, HttpMethod.Post, JobUri(provider), content, headers, cancellationToken).ConfigureAwait(false);
        var contentType = response.Headers.TryGetValue("Content-Type", out var value) ? value : string.Empty;
        var boundary = Boundary.Match(contentType);
        if (!boundary.Success)
        {
            throw new InvalidResponseDataException(contentType, "Prodia response missing multipart boundary in content-type: " + contentType);
        }

        return (ParseMultipart(response.Bytes, boundary.Groups[1].Value), response.Headers);
    }

    /// <summary>Serializes the job body the way the JSON and multipart requests send it.</summary>
    public static string Job(string modelId, JsonObject config)
    {
        return new JsonObject { ["type"] = modelId, ["config"] = config }.ToJsonString();
    }

    /// <summary>Reads the <c>job</c> part. Throws when it is missing.</summary>
    public static JsonElement ReadJob(IReadOnlyList<ProdiaMultipartPart> parts)
    {
        foreach (var part in parts)
        {
            if (part.ContentDisposition.Contains("name=\"job\""))
            {
                using var document = JsonDocument.Parse(part.Body);
                return document.RootElement.Clone();
            }
        }

        throw new InvalidResponseDataException(null, "Prodia multipart response missing job part");
    }

    /// <summary>Job id, seed, timing, timestamps, and price from a job result.</summary>
    public static JsonObject Metadata(JsonElement job)
    {
        var metadata = new JsonObject { ["jobId"] = job.GetProperty("id").GetString() };
        Copy(metadata, "seed", job, "config", "seed");
        Copy(metadata, "elapsed", job, "metrics", "elapsed");
        Copy(metadata, "iterationsPerSecond", job, "metrics", "ips");
        Copy(metadata, "createdAt", job, "created_at");
        Copy(metadata, "updatedAt", job, "updated_at");
        Copy(metadata, "dollars", job, "price", "dollars");
        return metadata;
    }

    /// <summary>Splits a multipart body on <paramref name="boundary"/>. The closing boundary is skipped.</summary>
    public static IReadOnlyList<ProdiaMultipartPart> ParseMultipart(byte[] data, string boundary)
    {
        var parts = new List<ProdiaMultipartPart>();
        var boundaryBytes = Encoding.UTF8.GetBytes("--" + boundary);
        var endBoundaryBytes = Encoding.UTF8.GetBytes("--" + boundary + "--");
        var positions = new List<int>();
        for (var i = 0; i <= data.Length - boundaryBytes.Length; i++)
        {
            if (Matches(data, i, boundaryBytes))
            {
                positions.Add(i);
            }
        }

        for (var i = 0; i < positions.Count - 1; i++)
        {
            if (Matches(data, positions[i], endBoundaryBytes))
            {
                continue;
            }

            var partStart = positions[i] + boundaryBytes.Length;
            if (At(data, partStart) == 0x0d && At(data, partStart + 1) == 0x0a)
            {
                partStart += 2;
            }
            else if (At(data, partStart) == 0x0a)
            {
                partStart += 1;
            }

            var partEnd = positions[i + 1];
            if (At(data, partEnd - 2) == 0x0d && At(data, partEnd - 1) == 0x0a)
            {
                partEnd -= 2;
            }
            else if (At(data, partEnd - 1) == 0x0a)
            {
                partEnd -= 1;
            }

            var part = Slice(data, partStart, partEnd);
            var headerEnd = -1;
            for (var j = 0; j < part.Length - 3; j++)
            {
                if (part[j] == 0x0d && part[j + 1] == 0x0a && part[j + 2] == 0x0d && part[j + 3] == 0x0a)
                {
                    headerEnd = j;
                    break;
                }

                if (part[j] == 0x0a && part[j + 1] == 0x0a)
                {
                    headerEnd = j;
                    break;
                }
            }

            if (headerEnd == -1)
            {
                continue;
            }

            var headers = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var line in Encoding.UTF8.GetString(part, 0, headerEnd).Split('\n'))
            {
                var colon = line.IndexOf(':');
                if (colon > 0)
                {
                    headers[line.Substring(0, colon).Trim().ToLowerInvariant()] = line.Substring(colon + 1).Trim();
                }
            }

            var bodyStart = part[headerEnd] == 0x0d ? headerEnd + 4 : headerEnd + 2;
            parts.Add(new ProdiaMultipartPart(headers, Slice(part, bodyStart, part.Length)));
        }

        return parts;
    }

    /// <summary>Builds the multipart request with the <c>job</c> part and an optional <c>input</c> file.</summary>
    public static MultipartFormDataContent Form(string job, byte[]? input, string? inputMediaType, string inputExtension)
    {
        var form = new MultipartFormDataContent();
        var jobContent = new ByteArrayContent(Encoding.UTF8.GetBytes(job));
        jobContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        form.Add(jobContent, "job", "job.json");
        if (input != null)
        {
            var inputContent = new ByteArrayContent(input);
            inputContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(inputMediaType!);
            form.Add(inputContent, "input", "input" + inputExtension);
        }

        return form;
    }

    private static Uri JobUri(ProdiaProvider provider)
    {
        return new Uri(provider.Options.BaseUrl + "/job?price=true");
    }

    private static void Copy(JsonObject target, string name, JsonElement source, params string[] path)
    {
        var current = source;
        foreach (var segment in path)
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out current))
            {
                return;
            }
        }

        if (current.ValueKind != JsonValueKind.Null)
        {
            target[name] = JsonNode.Parse(current.GetRawText());
        }
    }

    private static bool Matches(byte[] data, int offset, byte[] pattern)
    {
        if (offset + pattern.Length > data.Length)
        {
            return false;
        }

        for (var j = 0; j < pattern.Length; j++)
        {
            if (data[offset + j] != pattern[j])
            {
                return false;
            }
        }

        return true;
    }

    private static int At(byte[] data, int index)
    {
        return index >= 0 && index < data.Length ? data[index] : -1;
    }

    private static byte[] Slice(byte[] data, int start, int end)
    {
        var length = Math.Max(0, end - start);
        var slice = new byte[length];
        Array.Copy(data, start, slice, 0, length);
        return slice;
    }
}
