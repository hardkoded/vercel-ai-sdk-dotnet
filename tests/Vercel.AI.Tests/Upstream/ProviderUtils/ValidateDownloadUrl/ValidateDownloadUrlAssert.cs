// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.ValidateDownloadUrl;

/// <summary>Checks both validators, <see cref="DownloadUrls"/> and <see cref="Download"/>, against one URL.</summary>
internal static class ValidateDownloadUrlAssert
{
    public static void AssertAllowed(string url)
    {
        DownloadUrls.ValidateDownloadUrl(url);
        Download.ValidateDownloadUrl(url);
    }

    public static void AssertBlocked(string url)
    {
        Assert.Throws<DownloadException>(() => DownloadUrls.ValidateDownloadUrl(url));
        Assert.Throws<DownloadError>(() => Download.ValidateDownloadUrl(url));
    }
}
