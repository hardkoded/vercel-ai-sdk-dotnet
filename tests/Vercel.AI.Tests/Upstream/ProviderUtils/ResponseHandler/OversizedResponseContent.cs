// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ResponseHandler;

/// <summary>Content that declares a Content-Length above the download limit and records whether it was read or released.</summary>
internal sealed class OversizedResponseContent : HttpContent
{
    private readonly byte[] _bytes;

    public OversizedResponseContent(string body)
    {
        _bytes = Encoding.UTF8.GetBytes(body);
        Headers.ContentLength = Download.DefaultMaxBytes + 1;
    }

    public bool BodyRead { get; private set; }

    public bool Cancelled { get; private set; }

    public static (HttpResponseMessage Response, OversizedResponseContent Content) Create(string body = "{}", HttpStatusCode status = HttpStatusCode.OK, string? statusText = null)
    {
        var content = new OversizedResponseContent(body);
        var response = new HttpResponseMessage(status) { Content = content, ReasonPhrase = statusText };
        return (response, content);
    }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        BodyRead = true;
        return stream.WriteAsync(_bytes, 0, _bytes.Length);
    }

    protected override bool TryComputeLength(out long length)
    {
        length = Download.DefaultMaxBytes + 1;
        return true;
    }

    protected override void Dispose(bool disposing)
    {
        Cancelled = true;
        base.Dispose(disposing);
    }
}
