using System.Collections.Concurrent;
using OutSystems.ExternalLibraries.SDK;
using OutSystems.ExternalLibraries.SDK.FileStorage;
using OutSystems.ExternalLibraries.SDK.FileStorage.Exceptions;

namespace OutSystems.IbanChecker.UnitTests.FileStorageTesting;

/// <summary>
/// A complete, in-memory implementation of <see cref="IFileStorage"/> for unit tests.
/// Files live in a dictionary for the lifetime of the instance, so no ODC environment is needed.
/// Copy this file (and Loader.cs) into your own test project to test any FileStorage-based code.
/// </summary>
public sealed class InMemoryFileStorage : IFileStorage {

    private readonly ConcurrentDictionary<string, StoredFile> _files = new();

    /// <summary>
    /// Optional maximum file size. When set, creating a larger file throws
    /// <see cref="FileSizeLimitExceededException"/>, which lets you test size-limit handling.
    /// </summary>
    public long? MaxFileSizeBytes { get; set; }

    public Task<IOSFile> CreateFileAsync(string fileName, string contentType, byte[] content,
        IDictionary<string, string>? additionalUserData = null,
        CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Store(fileName, contentType, content.ToArray(), additionalUserData));
    }

    public async Task<IOSFile> CreateFileFromStreamAsync(string fileName, string contentType,
        Stream contentStream, IDictionary<string, string>? additionalUserData = null,
        CancellationToken cancellationToken = default) {
        using var buffer = new MemoryStream();
        await contentStream.CopyToAsync(buffer, cancellationToken);
        return Store(fileName, contentType, buffer.ToArray(), additionalUserData);
    }

    public async Task<IOSFile> CreateFileFromWriterAsync(string fileName, string contentType,
        Func<Stream, CancellationToken, Task> writeAsync,
        IDictionary<string, string>? additionalUserData = null,
        CancellationToken cancellationToken = default) {
        var buffer = new MemoryStream();
        await writeAsync(buffer, cancellationToken);
        // MemoryStream.ToArray() still works if the writer disposed the stream.
        return Store(fileName, contentType, buffer.ToArray(), additionalUserData);
    }

    public Task<byte[]> ReadFileAsync(IOSFile file, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Get(file).Content.ToArray());
    }

    public Task<Stream> ReadFileAsStreamAsync(IOSFile file,
        CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<Stream>(new MemoryStream(Get(file).Content, writable: false));
    }

    public Task<Stream> ReadFileRangeAsync(IOSFile file, long startByte, long endByte,
        CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        byte[] content = Get(file).Content;
        if (startByte < 0 || startByte >= content.Length || endByte < startByte) {
            throw new ArgumentOutOfRangeException(nameof(startByte),
                $"Invalid range {startByte}-{endByte} for a file of {content.Length} bytes.");
        }
        // endByte is inclusive (HTTP Range semantics) and is clamped to the last byte.
        long lastByte = Math.Min(endByte, content.Length - 1);
        int length = (int)(lastByte - startByte + 1);
        return Task.FromResult<Stream>(
            new MemoryStream(content, (int)startByte, length, writable: false));
    }

    public Task<IFileMetadata> GetFileMetadataAsync(IOSFile file,
        CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IFileMetadata>(new InMemoryFileMetadata(Get(file).UserData));
    }

    private IOSFile Store(string fileName, string contentType, byte[] content,
        IDictionary<string, string>? additionalUserData) {
        if (MaxFileSizeBytes is long limit && content.LongLength > limit) {
            throw new FileSizeLimitExceededException(limit);
        }

        // Like the real SDK, the file name is stored as user metadata under Keys.FileName.
        var userData = additionalUserData is null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(additionalUserData);
        userData[IFileMetadata.Keys.FileName] = fileName;

        var file = new InMemoryFile(Guid.NewGuid().ToString("N"), "1", contentType, content.LongLength);
        _files[file.Key] = new StoredFile(content, userData);
        return file;
    }

    private StoredFile Get(IOSFile file) =>
        _files.TryGetValue(file.Key, out var stored)
            ? stored
            : throw new FileNotFoundException($"File '{file.Key}' does not exist in InMemoryFileStorage.");

    private sealed record StoredFile(byte[] Content, IReadOnlyDictionary<string, string> UserData);

    private sealed record InMemoryFile(string Key, string Version, string ContentType, long Size)
        : IOSFile;

    private sealed class InMemoryFileMetadata : IFileMetadata {
        public InMemoryFileMetadata(IReadOnlyDictionary<string, string> userData) {
            UserData = userData;
        }

        public IReadOnlyDictionary<string, string> UserData { get; }

        public string? GetMetadata(string key) =>
            UserData.TryGetValue(key, out var value) ? value : null;

        public bool TryGetMetadata(string key, out string? value) {
            bool found = UserData.TryGetValue(key, out var stored);
            value = stored;
            return found;
        }
    }
}
