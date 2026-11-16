// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using static Examples.Lib.ApplyDiffHelper;

namespace Examples.Lib;

/// <summary>An apply_patch operation. Port of the ApplyPatchOperation union.</summary>
public abstract record ApplyPatchOperation(string Path)
{
    /// <summary>Gets the operation type (create_file, delete_file or update_file).</summary>
    public abstract string Type { get; }
}

/// <summary>The create_file operation.</summary>
public sealed record CreateFileOperation(string Path, string Diff) : ApplyPatchOperation(Path)
{
    /// <inheritdoc />
    public override string Type => "create_file";
}

/// <summary>The delete_file operation.</summary>
public sealed record DeleteFileOperation(string Path) : ApplyPatchOperation(Path)
{
    /// <inheritdoc />
    public override string Type => "delete_file";
}

/// <summary>The update_file operation.</summary>
public sealed record UpdateFileOperation(string Path, string Diff) : ApplyPatchOperation(Path)
{
    /// <inheritdoc />
    public override string Type => "update_file";
}

/// <summary>Input of the executor returned by <see cref="ApplyPatchFileEditor.CreateApplyPatchExecutor"/>.</summary>
public sealed record ApplyPatchExecutorInput(string CallId, ApplyPatchOperation Operation);

/// <summary>Status of an apply_patch result.</summary>
public enum ApplyPatchStatus
{
    /// <summary>The operation succeeded.</summary>
    Completed,

    /// <summary>The operation failed.</summary>
    Failed,
}

/// <summary>Result of an apply_patch operation.</summary>
public sealed record ApplyPatchResult(ApplyPatchStatus Status, string? Output = null);

/// <summary>Port of examples/ai-functions/src/lib/apply-patch-file-editor.ts.</summary>
public static class ApplyPatchFileEditor
{
    /// <summary>Creates an executor that applies operations inside <paramref name="workspaceRoot"/>.</summary>
    public static Func<ApplyPatchExecutorInput, Task<ApplyPatchResult>> CreateApplyPatchExecutor(string workspaceRoot)
    {
        var editor = new WorkspaceEditor(workspaceRoot);

        return input =>
        {
            Console.WriteLine($"[{input.CallId}] Applying {input.Operation.Type} to {input.Operation.Path}");

            return input.Operation switch
            {
                CreateFileOperation create => editor.CreateFileAsync(create),
                UpdateFileOperation update => editor.UpdateFileAsync(update),
                DeleteFileOperation delete => editor.DeleteFileAsync(delete),
                _ => throw new ArgumentException("Unknown operation.", nameof(input)),
            };
        };
    }
}

/// <summary>Applies apply_patch operations to files under a workspace root.</summary>
public sealed class WorkspaceEditor(string root)
{
    private static readonly UTF8Encoding Utf8 = new(false);

    /// <summary>Creates a file from a create diff.</summary>
    public async Task<ApplyPatchResult> CreateFileAsync(CreateFileOperation operation)
    {
        try
        {
            var targetPath = Resolve(operation.Path);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            var content = ApplyDiff(string.Empty, operation.Diff, ApplyDiffMode.Create);
            await File.WriteAllTextAsync(targetPath, content, Utf8).ConfigureAwait(false);
            return new ApplyPatchResult(ApplyPatchStatus.Completed, $"Created {operation.Path}");
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return new ApplyPatchResult(ApplyPatchStatus.Failed, $"Error creating file: {error.Message}");
        }
    }

    /// <summary>Updates a file from an update diff.</summary>
    public async Task<ApplyPatchResult> UpdateFileAsync(UpdateFileOperation operation)
    {
        try
        {
            var targetPath = Resolve(operation.Path);
            string original;
            try
            {
                original = await File.ReadAllTextAsync(targetPath, Encoding.UTF8).ConfigureAwait(false);
            }
            catch (Exception error) when (error is FileNotFoundException or DirectoryNotFoundException)
            {
                throw new InvalidOperationException($"Cannot update missing file: {operation.Path}", error);
            }

            var patched = ApplyDiff(original, operation.Diff);
            await File.WriteAllTextAsync(targetPath, patched, Utf8).ConfigureAwait(false);
            return new ApplyPatchResult(ApplyPatchStatus.Completed, $"Updated {operation.Path}");
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return new ApplyPatchResult(ApplyPatchStatus.Failed, $"Error updating file: {error.Message}");
        }
    }

    /// <summary>Deletes a file. A missing file is not an error.</summary>
    public Task<ApplyPatchResult> DeleteFileAsync(DeleteFileOperation operation)
    {
        try
        {
            var targetPath = Resolve(operation.Path);
            File.Delete(targetPath);
            return Task.FromResult(new ApplyPatchResult(ApplyPatchStatus.Completed, $"Deleted {operation.Path}"));
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return Task.FromResult(new ApplyPatchResult(ApplyPatchStatus.Failed, $"Error deleting file: {error.Message}"));
        }
    }

    private string Resolve(string relativePath)
    {
        var relative = Path.GetRelativePath(
            Path.GetFullPath(root),
            Path.GetFullPath(relativePath, Path.GetFullPath(root)));
        if (relative.Length == 0
            || relative == ".."
            || relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            || Path.IsPathRooted(relative))
        {
            throw new InvalidOperationException($"Operation outside workspace: {relativePath}");
        }

        // Use the real workspace path and reject existing symlinks along the path.
        var realRoot = RealPath(root);
        var current = realRoot;
        foreach (var segment in relative.Split(Path.DirectorySeparatorChar))
        {
            current = Path.Combine(current, segment);
            FileAttributes attributes;
            try
            {
                attributes = File.GetAttributes(current);
            }
            catch (Exception error) when (error is FileNotFoundException or DirectoryNotFoundException)
            {
                break;
            }

            if ((attributes & FileAttributes.ReparsePoint) != 0)
            {
                throw new InvalidOperationException($"Operation through symlink: {relativePath}");
            }
        }

        return Path.Combine(realRoot, relative);
    }

    private static string RealPath(string path)
    {
        var full = Path.GetFullPath(path);
        var info = new DirectoryInfo(full);
        if (!info.Exists)
        {
            throw new DirectoryNotFoundException($"ENOENT: no such file or directory, realpath '{path}'");
        }

        var target = info.ResolveLinkTarget(true);
        return target?.FullName ?? info.FullName;
    }
}
