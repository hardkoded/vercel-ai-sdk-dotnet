// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http;
using System.Net.Http.Headers;

namespace Vercel.AI.ProviderUtils;

/// <summary>Builds multipart parts whose names cannot break out of the <c>Content-Disposition</c> header.</summary>
internal static class MultipartForm
{
    /// <summary>
    /// Quotes a field or file name like the WHATWG <c>FormData</c> encoder that upstream uses:
    /// a double quote, CR, and LF become <c>%22</c>, <c>%0D</c>, and <c>%0A</c>.
    /// </summary>
    public static string Quote(string value)
    {
        var escaped = (value ?? string.Empty)
            .Replace("\"", "%22")
            .Replace("\r", "%0D")
            .Replace("\n", "%0A");
        return "\"" + escaped + "\"";
    }

    /// <summary>Adds a file part with a quoted <c>name</c> and <c>filename</c>.</summary>
    public static void AddFile(MultipartFormDataContent form, HttpContent content, string name, string fileName)
    {
        form.Add(content);
        content.Headers.TryAddWithoutValidation("Content-Disposition", "form-data; name=" + Quote(name) + "; filename=" + Quote(fileName));
    }
}
