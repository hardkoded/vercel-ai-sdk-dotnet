// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net.Http;
using System.Net.Sockets;
using Vercel.AI.Util;

namespace Vercel.AI.ProviderUtils;

/// <summary>Turns transport failures into retryable <see cref="ApiCallError"/>s. Maps to <c>handleFetchError</c>.</summary>
public static class FetchErrors
{
    private static readonly HashSet<SocketError> RetryableSocketErrors = new HashSet<SocketError>
    {
        SocketError.ConnectionRefused,
        SocketError.ConnectionReset,
        SocketError.ConnectionAborted,
        SocketError.Shutdown,
        SocketError.TimedOut,
        SocketError.HostUnreachable,
        SocketError.NetworkUnreachable,
    };

    /// <summary>
    /// Returns abort errors unchanged. An <see cref="HttpRequestException"/> with an inner cause, or any error caused by a
    /// connection-level <see cref="SocketException"/>, becomes a retryable <see cref="ApiCallError"/>. Other errors are returned unchanged.
    /// </summary>
    public static Exception HandleFetchError(Exception error, string url, object requestBodyValues)
    {
        if (AbortErrors.IsAbortError(error))
        {
            return error;
        }

        if (error is HttpRequestException && error.InnerException != null)
        {
            return new ApiCallError("Cannot connect to API: " + error.InnerException.Message, url, requestBodyValues, cause: error.InnerException, isRetryable: true);
        }

        if (!HasNetworkError(error))
        {
            return error;
        }

        if (error is ApiCallError apiCallError)
        {
            return new ApiCallError(
                apiCallError.Message,
                apiCallError.Url,
                apiCallError.RequestBodyValues,
                apiCallError.StatusCode,
                apiCallError.ResponseHeaders,
                apiCallError.ResponseBody,
                apiCallError.Cause,
                isRetryable: true,
                apiCallError.Data);
        }

        return new ApiCallError("Cannot connect to API: " + error.Message, url, requestBodyValues, cause: error, isRetryable: true);
    }

    private static bool HasNetworkError(Exception? error)
    {
        for (var current = error; current != null; current = current.InnerException)
        {
            if (current is SocketException socket && RetryableSocketErrors.Contains(socket.SocketErrorCode))
            {
                return true;
            }
        }

        return false;
    }
}
