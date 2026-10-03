// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Vercel.AI.GenerateText;

/// <summary>HMAC approval signatures. Maps to <c>signToolApproval</c> and <c>verifyToolApprovalSignature</c>.</summary>
public static class ToolApprovalSignatures
{
    private const string Version = "ai-sdk-tool-approval-v1";

    /// <summary>Signs a tool approval. Equivalent inputs with different key order produce the same signature.</summary>
    public static string SignToolApproval(string secret, string approvalId, string toolCallId, string toolName, JsonElement input)
    {
        if (secret is null)
        {
            throw new ArgumentNullException(nameof(secret));
        }

        return Sign(Encoding.UTF8.GetBytes(secret), approvalId, toolCallId, toolName, input);
    }

    /// <summary>Signs a tool approval with a raw secret.</summary>
    public static string SignToolApproval(byte[] secret, string approvalId, string toolCallId, string toolName, JsonElement input)
    {
        if (secret is null)
        {
            throw new ArgumentNullException(nameof(secret));
        }

        return Sign(secret, approvalId, toolCallId, toolName, input);
    }

    /// <summary>Returns a signature when <paramref name="secret"/> is set, and null when it is not.</summary>
    public static string? MaybeSignApproval(string? secret, string approvalId, string toolCallId, string toolName, JsonElement input)
    {
        if (secret is null)
        {
            return null;
        }

        return SignToolApproval(secret, approvalId, toolCallId, toolName, input);
    }

    /// <summary>
    /// Verifies a signature. Accepts the current JSON payload and, when no field contains a newline,
    /// a legacy newline-joined payload.
    /// </summary>
    public static bool VerifyToolApprovalSignature(string secret, string signature, string approvalId, string toolCallId, string toolName, JsonElement input)
    {
        if (secret is null)
        {
            throw new ArgumentNullException(nameof(secret));
        }

        return Verify(Encoding.UTF8.GetBytes(secret), signature, approvalId, toolCallId, toolName, input);
    }

    /// <summary>Verifies a signature with a raw secret.</summary>
    public static bool VerifyToolApprovalSignature(byte[] secret, string signature, string approvalId, string toolCallId, string toolName, JsonElement input)
    {
        if (secret is null)
        {
            throw new ArgumentNullException(nameof(secret));
        }

        return Verify(secret, signature, approvalId, toolCallId, toolName, input);
    }

    private static string Sign(byte[] secret, string approvalId, string toolCallId, string toolName, JsonElement input)
    {
        var payload = BuildPayload(approvalId, toolCallId, toolName, CanonicalHash.HashCanonical(input));
        using var hmac = new HMACSHA256(secret);
        return CanonicalHash.ToBase64Url(hmac.ComputeHash(payload));
    }

    private static bool Verify(byte[] secret, string signature, string approvalId, string toolCallId, string toolName, JsonElement input)
    {
        byte[] provided;
        try
        {
            provided = CanonicalHash.FromBase64Url(signature);
        }
        catch (FormatException)
        {
            return false;
        }

        var digest = CanonicalHash.HashCanonical(input);
        using var hmac = new HMACSHA256(secret);
        if (FixedEquals(hmac.ComputeHash(BuildPayload(approvalId, toolCallId, toolName, digest)), provided))
        {
            return true;
        }

        if (approvalId.IndexOf('\n') >= 0 || toolCallId.IndexOf('\n') >= 0 || toolName.IndexOf('\n') >= 0)
        {
            return false;
        }

        var legacy = Encoding.UTF8.GetBytes(approvalId + "\n" + toolCallId + "\n" + toolName + "\n" + digest);
        return FixedEquals(hmac.ComputeHash(legacy), provided);
    }

    private static byte[] BuildPayload(string approvalId, string toolCallId, string toolName, string inputDigest)
    {
        var json = JsonSerializer.Serialize(new[] { Version, approvalId, toolCallId, toolName, inputDigest });
        return Encoding.UTF8.GetBytes(json);
    }

    private static bool FixedEquals(byte[] left, byte[] right)
    {
        if (left.Length != right.Length)
        {
            return false;
        }

        var difference = 0;
        for (var i = 0; i < left.Length; i++)
        {
            difference |= left[i] ^ right[i];
        }

        return difference == 0;
    }
}
