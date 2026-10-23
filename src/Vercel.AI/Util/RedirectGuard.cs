// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Util;

/// <summary>Turns off automatic redirects on a caller-supplied handler, so a provider that follows redirects by hand never sends its credentials to another host.</summary>
internal static class RedirectGuard
{
    /// <summary>
    /// Sets <c>AllowAutoRedirect = false</c> on <paramref name="handler"/> and on every handler it wraps.
    /// It covers <see cref="HttpClientHandler"/>, <c>SocketsHttpHandler</c> (.NET 5 and later), and any other handler with a public settable <c>bool AllowAutoRedirect</c> property.
    /// A <see cref="DelegatingHandler"/> chain is walked to the end. The handlers are changed in place.
    /// </summary>
    public static void DisableAutoRedirect(HttpMessageHandler? handler)
    {
        while (handler is not null)
        {
            switch (handler)
            {
                case HttpClientHandler clientHandler:
                    clientHandler.AllowAutoRedirect = false;
                    break;
#if NET5_0_OR_GREATER
                case SocketsHttpHandler socketsHandler:
                    socketsHandler.AllowAutoRedirect = false;
                    break;
#endif
                default:
                    // Covers SocketsHttpHandler on netstandard2.0 (a .NET runtime can still supply one) and custom handlers that expose the same property.
                    var property = handler.GetType().GetProperty("AllowAutoRedirect", typeof(bool));
                    if (property?.CanWrite == true && property.SetMethod!.IsPublic)
                    {
                        property.SetValue(handler, false);
                    }

                    break;
            }

            handler = (handler as DelegatingHandler)?.InnerHandler;
        }
    }
}
