// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.ProviderUtils;

/// <summary>Options for executing a command in the sandbox with <c>run</c> or <c>spawn</c>. Maps to <c>SandboxProcessOptions</c>.</summary>
public class SandboxProcessOptions
{
    /// <summary>Command to execute in the sandbox. Maps to <c>command</c>.</summary>
    public string Command { get; init; } = string.Empty;

    /// <summary>Working directory to execute the command in. Maps to <c>workingDirectory</c>.</summary>
    public string? WorkingDirectory { get; init; }

    /// <summary>
    /// Environment variables to set for this command. Merged with the default environment of the sandbox, and the values here take precedence. Maps to <c>env</c>.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Env { get; init; }

    /// <summary>Token that aborts the command. When it is cancelled, the running process is killed. Maps to <c>abortSignal</c>.</summary>
    public CancellationToken AbortSignal { get; init; }
}

/// <summary>Options of <see cref="ISandboxSession.RunAsync"/>. Maps to <c>SandboxRunOptions</c>.</summary>
public sealed class SandboxRunOptions : SandboxProcessOptions { }

/// <summary>Options of <see cref="ISandboxSession.SpawnAsync"/>. Maps to <c>SandboxSpawnOptions</c>.</summary>
public sealed class SandboxSpawnOptions : SandboxProcessOptions
{
    /// <summary>Whether to keep standard input open and expose a writable stream on the process. Maps to <c>stdin</c>.</summary>
    public bool Stdin { get; init; }
}

/// <summary>Options for reading, inspecting, or deleting one path in the sandbox. Maps to <c>PathOptions</c>.</summary>
public class SandboxPathOptions
{
    /// <summary>Path to operate on. Maps to <c>path</c>.</summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>Token that aborts the operation. Maps to <c>abortSignal</c>.</summary>
    public CancellationToken AbortSignal { get; init; }
}

/// <summary>Options of <see cref="ISandboxSession.ReadFileAsync"/>. Maps to <c>SandboxReadFileOptions</c>.</summary>
public sealed class SandboxReadFileOptions : SandboxPathOptions { }

/// <summary>Options of <see cref="ISandboxSession.ReadBinaryFileAsync"/>. Maps to <c>SandboxReadBinaryFileOptions</c>.</summary>
public sealed class SandboxReadBinaryFileOptions : SandboxPathOptions { }

/// <summary>Options of <see cref="ISandboxSession.ReadTextFileAsync"/>. Maps to <c>SandboxReadTextFileOptions</c>.</summary>
public sealed class SandboxReadTextFileOptions : SandboxPathOptions
{
    /// <summary>Text encoding used to decode the file bytes. Defaults to <c>utf-8</c>. Maps to <c>encoding</c>.</summary>
    public string? Encoding { get; init; }

    /// <summary>1-based inclusive start line. Defaults to 1. Maps to <c>startLine</c>.</summary>
    public int? StartLine { get; init; }

    /// <summary>1-based inclusive end line. When past the line count of the file, the read returns through the end of the file. Maps to <c>endLine</c>.</summary>
    public int? EndLine { get; init; }
}

/// <summary>Options of <see cref="ISandboxSession.StatAsync"/>. Maps to <c>SandboxStatOptions</c>.</summary>
public sealed class SandboxStatOptions : SandboxPathOptions { }

/// <summary>Options of <see cref="ISandboxSession.ListDirectoryAsync"/>. Maps to <c>SandboxListDirectoryOptions</c>.</summary>
public sealed class SandboxListDirectoryOptions : SandboxPathOptions { }

/// <summary>Options of <see cref="ISandboxSession.DeleteFileAsync"/>. Maps to <c>SandboxDeleteFileOptions</c>.</summary>
public sealed class SandboxDeleteFileOptions : SandboxPathOptions { }

/// <summary>Options of <see cref="ISandboxSession.WriteFileAsync"/>. Maps to <c>SandboxWriteFileOptions</c>.</summary>
public sealed class SandboxWriteFileOptions : SandboxPathOptions
{
    /// <summary>Content to write to the file. Maps to <c>content</c>.</summary>
    public Stream Content { get; init; } = Stream.Null;
}

/// <summary>Options of <see cref="ISandboxSession.WriteBinaryFileAsync"/>. Maps to <c>SandboxWriteBinaryFileOptions</c>.</summary>
public sealed class SandboxWriteBinaryFileOptions : SandboxPathOptions
{
    /// <summary>Content to write to the file. Maps to <c>content</c>.</summary>
    public byte[] Content { get; init; } = [];
}

/// <summary>Options of <see cref="ISandboxSession.WriteTextFileAsync"/>. Maps to <c>SandboxWriteTextFileOptions</c>.</summary>
public sealed class SandboxWriteTextFileOptions : SandboxPathOptions
{
    /// <summary>Content to write to the file. Maps to <c>content</c>.</summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>Text encoding used to encode the string to bytes. Defaults to <c>utf-8</c>. Maps to <c>encoding</c>.</summary>
    public string? Encoding { get; init; }
}

/// <summary>Type of a path returned by <see cref="ISandboxSession.StatAsync"/>. Maps to <c>SandboxFileStat['type']</c>.</summary>
public enum SandboxFileType
{
    /// <summary>A file.</summary>
    File,

    /// <summary>A directory.</summary>
    Directory,

    /// <summary>A device, socket, or named pipe.</summary>
    Other,
}

/// <summary>Information about a file or directory in the sandbox. Symbolic links are followed. Maps to <c>SandboxFileStat</c>.</summary>
public sealed class SandboxFileStat
{
    /// <summary>Type of the path. Maps to <c>type</c>.</summary>
    public SandboxFileType Type { get; init; }

    /// <summary>Size in bytes. The size of a directory is implementation-defined. Maps to <c>size</c>.</summary>
    public long Size { get; init; }
}

/// <summary>Type of an entry returned by <see cref="ISandboxSession.ListDirectoryAsync"/>. Maps to <c>SandboxDirectoryEntry['type']</c>.</summary>
public enum SandboxDirectoryEntryType
{
    /// <summary>A file.</summary>
    File,

    /// <summary>A directory.</summary>
    Directory,

    /// <summary>A symbolic link. It is not followed.</summary>
    Symlink,

    /// <summary>A device, socket, or named pipe.</summary>
    Other,
}

/// <summary>Entry in a directory listing. Maps to <c>SandboxDirectoryEntry</c>.</summary>
public sealed class SandboxDirectoryEntry
{
    /// <summary>Name of the entry, without the directory path. Maps to <c>name</c>.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Type of the entry. Maps to <c>type</c>.</summary>
    public SandboxDirectoryEntryType Type { get; init; }
}

/// <summary>The result of <see cref="ISandboxSession.RunAsync"/>. Maps to the result of <c>run</c>.</summary>
public sealed class SandboxRunResult
{
    /// <summary>Exit code returned by the command. Maps to <c>exitCode</c>.</summary>
    public int ExitCode { get; init; }

    /// <summary>Standard output produced by the command. Maps to <c>stdout</c>.</summary>
    public string Stdout { get; init; } = string.Empty;

    /// <summary>Standard error produced by the command. Maps to <c>stderr</c>.</summary>
    public string Stderr { get; init; } = string.Empty;
}

/// <summary>Handle to a long-running process started with <see cref="ISandboxSession.SpawnAsync"/>. Maps to <c>SandboxProcess</c>.</summary>
public interface ISandboxProcess
{
    /// <summary>Process identifier, if the sandbox implementation exposes one. Maps to <c>pid</c>.</summary>
    int? Pid { get; }

    /// <summary>Stream of bytes written by the process to standard output. Maps to <c>stdout</c>.</summary>
    Stream Stdout { get; }

    /// <summary>Stream of bytes written by the process to standard error. Maps to <c>stderr</c>.</summary>
    Stream Stderr { get; }

    /// <summary>Stream of bytes written to the standard input of the process, or null when it was not requested. Maps to <c>stdin</c> of <c>WritableSandboxProcess</c>.</summary>
    Stream? Stdin { get; }

    /// <summary>Completes when the process exits, yielding its exit code. Maps to <c>wait</c>.</summary>
    Task<int> WaitAsync(CancellationToken cancellationToken = default);

    /// <summary>Terminates the process. Idempotent. Maps to <c>kill</c>.</summary>
    Task KillAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Sandbox session that can execute commands and read and write files.
/// Relative paths in every method resolve against the same base directory: the directory that <see cref="RunAsync"/> and <see cref="SpawnAsync"/> start in when no
/// working directory is given. Absolute paths are used as is. Maps to <c>SandboxSession</c> (<c>Experimental_SandboxSession</c>).
/// </summary>
public interface ISandboxSession
{
    /// <summary>
    /// Description of the sandbox environment that can be added to the instructions of the agent, so that it knows relevant details such as the root directory.
    /// Maps to <c>description</c>.
    /// </summary>
    string Description { get; }

    /// <summary>Reads one file as a stream of bytes. Returns null when the file does not exist. Maps to <c>readFile</c>.</summary>
    Task<Stream?> ReadFileAsync(SandboxReadFileOptions options);

    /// <summary>Reads one file as raw bytes. Returns null when the file does not exist. Maps to <c>readBinaryFile</c>.</summary>
    Task<byte[]?> ReadBinaryFileAsync(SandboxReadBinaryFileOptions options);

    /// <summary>Reads one text file, decoded with the requested encoding. Returns null when the file does not exist. Maps to <c>readTextFile</c>.</summary>
    Task<string?> ReadTextFileAsync(SandboxReadTextFileOptions options);

    /// <summary>Writes one file from a stream of bytes. Creates parent directories and overwrites an existing file. Maps to <c>writeFile</c>.</summary>
    Task WriteFileAsync(SandboxWriteFileOptions options);

    /// <summary>Writes one file from raw bytes. Creates parent directories and overwrites an existing file. Maps to <c>writeBinaryFile</c>.</summary>
    Task WriteBinaryFileAsync(SandboxWriteBinaryFileOptions options);

    /// <summary>Writes one file from a string. Creates parent directories and overwrites an existing file. Maps to <c>writeTextFile</c>.</summary>
    Task WriteTextFileAsync(SandboxWriteTextFileOptions options);

    /// <summary>Gets information about a file or directory. Returns null when the path does not exist. Maps to <c>stat</c>.</summary>
    Task<SandboxFileStat?> StatAsync(SandboxStatOptions options);

    /// <summary>Lists the entries of one directory. Returns null when the directory does not exist. Maps to <c>listDirectory</c>.</summary>
    Task<IReadOnlyList<SandboxDirectoryEntry>?> ListDirectoryAsync(
        SandboxListDirectoryOptions options
    );

    /// <summary>Deletes one file. Returns true when the file was deleted and false when it does not exist. Maps to <c>deleteFile</c>.</summary>
    Task<bool> DeleteFileAsync(SandboxDeleteFileOptions options);

    /// <summary>Spawns a long-running process. Returns immediately with a handle. Maps to <c>spawn</c>.</summary>
    Task<ISandboxProcess> SpawnAsync(SandboxSpawnOptions options);

    /// <summary>Runs a command in the sandbox. Maps to <c>run</c>.</summary>
    Task<SandboxRunResult> RunAsync(SandboxRunOptions options);
}
