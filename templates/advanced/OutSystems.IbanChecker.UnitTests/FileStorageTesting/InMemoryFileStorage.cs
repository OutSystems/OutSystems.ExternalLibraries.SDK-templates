using System.Collections.Concurrent;
using OutSystems.ExternalLibraries.SDK;
using OutSystems.ExternalLibraries.SDK.FileStorage;
using OutSystems.ExternalLibraries.SDK.FileStorage.Exceptions;

namespace OutSystems.IbanChecker.UnitTests.FileStorageTesting;

/// <summary>
/// A complete, in-memory implementation of <see cref="IFileStorage"/> for unit tests.
/// Files live in a dictionary for the lifetime of the instance, so no ODC environment is needed.
/// Copy this file into your own test project to test any FileStorage-based code:
/// <list type="bullet">
/// <item>In SetUp, call <c>FileStorage.Reset()</c> and then register a new instance with
/// <c>FileStorage.Configure(() => storage)</c>.</item>
/// <item>In TearDown, call <c>FileStorage.Reset()</c>.</item>
/// <item>Mark the fixture <c>[NonParallelizable]</c>, because FileStorage is process-wide state.</item>
/// </list>
/// It does not validate metadata as the ODC File Storage SDK does, so always test your library in
/// ODC before you release it. For the metadata rules, see
/// https://success.outsystems.com/documentation/outsystems_developer_cloud/building_apps/data_management/file_data_type/working_with_file_values_in_external_libraries/
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

        // Like the real SDK, metadata keys are stored in lowercase and matched case-insensitively,
        // keys that differ only in casing throw an ArgumentException, and the file name is stored
        // last under Keys.FileName, so a FileName key in additionalUserData is ignored. No other
        // metadata rule is checked; for the full list, see
        // https://success.outsystems.com/documentation/outsystems_developer_cloud/building_apps/data_management/file_data_type/working_with_file_values_in_external_libraries/
        var userData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in additionalUserData ?? new Dictionary<string, string>()) {
            if (!userData.TryAdd(key.ToLowerInvariant(), value)) {
                throw new ArgumentException(
                    $"Metadata key '{key}' differs from another key only in casing.",
                    nameof(additionalUserData));
            }
        }
        userData[IFileMetadata.Keys.FileName.ToLowerInvariant()] = fileName;

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
