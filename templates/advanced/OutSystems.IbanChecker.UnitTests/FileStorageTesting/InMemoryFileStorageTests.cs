using System.Text;
using NUnit.Framework;
using OutSystems.ExternalLibraries.SDK.FileStorage;
using OutSystems.ExternalLibraries.SDK.FileStorage.Exceptions;

namespace OutSystems.IbanChecker.UnitTests.FileStorageTesting;

/// <summary>
/// Tests for <see cref="InMemoryFileStorage"/> itself, so that anyone copying the fake into their
/// own test project gets coverage of its behaviour too.
/// </summary>
public class InMemoryFileStorageTests {

    /// <summary>
    /// Verifies a file created from a byte array can be read back unchanged.
    /// </summary>
    [Test]
    public async Task CreateFileAsyncStoresContentThatReadFileAsyncReturns() {
        // Setup: Create an empty storage and some content.
        var storage = new InMemoryFileStorage();
        byte[] content = Encoding.UTF8.GetBytes("hello");

        // Act: Create the file and read it back.
        var file = await storage.CreateFileAsync("a.txt", "text/plain", content);
        byte[] read = await storage.ReadFileAsync(file);

        // Assert: The content and the file information match.
        Assert.Multiple(() => {
            Assert.That(read, Is.EqualTo(content));
            Assert.That(file.ContentType, Is.EqualTo("text/plain"));
            Assert.That(file.Size, Is.EqualTo(content.Length));
            Assert.That(file.Key, Is.Not.Empty);
        });
    }

    /// <summary>
    /// Verifies a file created from a stream can be read back unchanged.
    /// </summary>
    [Test]
    public async Task CreateFileFromStreamAsyncStoresStreamContent() {
        // Setup: Create an empty storage and a source stream.
        var storage = new InMemoryFileStorage();
        using var source = new MemoryStream(Encoding.UTF8.GetBytes("streamed"));

        // Act: Create the file from the stream and read it back.
        var file = await storage.CreateFileFromStreamAsync("b.txt", "text/plain", source);
        byte[] read = await storage.ReadFileAsync(file);

        // Assert: The stored content is the stream content.
        Assert.That(Encoding.UTF8.GetString(read), Is.EqualTo("streamed"));
    }

    /// <summary>
    /// Verifies the writer callback content is stored, even when the callback disposes the stream
    /// (as a StreamWriter without leaveOpen does).
    /// </summary>
    [Test]
    public async Task CreateFileFromWriterAsyncStoresContentWhenWriterDisposesStream() {
        // Setup: Create an empty storage.
        var storage = new InMemoryFileStorage();

        // Act: Create a file whose writer callback disposes the destination stream.
        var file = await storage.CreateFileFromWriterAsync("c.txt", "text/plain",
            async (destination, token) => {
                await using var writer = new StreamWriter(destination);
                await writer.WriteAsync("written");
            });
        byte[] read = await storage.ReadFileAsync(file);

        // Assert: The written content was stored.
        Assert.That(Encoding.UTF8.GetString(read), Is.EqualTo("written"));
    }

    /// <summary>
    /// Verifies ReadFileAsStreamAsync returns the content as a read-only stream.
    /// </summary>
    [Test]
    public async Task ReadFileAsStreamAsyncReturnsReadOnlyStreamWithContent() {
        // Setup: Store a file.
        var storage = new InMemoryFileStorage();
        var file = await storage.CreateFileAsync("d.txt", "text/plain", Encoding.UTF8.GetBytes("hello"));

        // Act: Read the file as a stream.
        await using var stream = await storage.ReadFileAsStreamAsync(file);
        using var reader = new StreamReader(stream);

        // Assert: The stream is read-only and has the file content.
        Assert.Multiple(() => {
            Assert.That(stream.CanWrite, Is.False);
            Assert.That(reader.ReadToEnd(), Is.EqualTo("hello"));
        });
    }

    /// <summary>
    /// Verifies ReadFileRangeAsync treats endByte as inclusive.
    /// </summary>
    [Test]
    public async Task ReadFileRangeAsyncTreatsEndByteAsInclusive() {
        // Setup: Store a 5-byte file.
        var storage = new InMemoryFileStorage();
        var file = await storage.CreateFileAsync("e.txt", "text/plain", Encoding.UTF8.GetBytes("hello"));

        // Act: Request bytes 1..2.
        await using var range = await storage.ReadFileRangeAsync(file, 1, 2);
        using var reader = new StreamReader(range);

        // Assert: Both the start and end bytes are returned.
        Assert.That(await reader.ReadToEndAsync(), Is.EqualTo("el"));
    }

    /// <summary>
    /// Verifies ReadFileRangeAsync clamps endByte to the last byte of the file.
    /// </summary>
    [Test]
    public async Task ReadFileRangeAsyncClampsEndByteToLastByte() {
        // Setup: Store a 5-byte file.
        var storage = new InMemoryFileStorage();
        var file = await storage.CreateFileAsync("f.txt", "text/plain", Encoding.UTF8.GetBytes("hello"));

        // Act: Request bytes 1..100 (end beyond the file).
        await using var range = await storage.ReadFileRangeAsync(file, 1, 100);
        using var reader = new StreamReader(range);

        // Assert: Only bytes 1..4 are returned.
        Assert.That(await reader.ReadToEndAsync(), Is.EqualTo("ello"));
    }

    /// <summary>
    /// Verifies ReadFileRangeAsync rejects ranges that start outside the file or end before they start.
    /// </summary>
    [Test]
    public async Task ReadFileRangeAsyncThrowsWhenRangeIsInvalid() {
        // Setup: Store a 5-byte file.
        var storage = new InMemoryFileStorage();
        var file = await storage.CreateFileAsync("g.txt", "text/plain", Encoding.UTF8.GetBytes("hello"));

        // Act and Assert: Both a start past the end and an end before the start are rejected.
        Assert.Multiple(() => {
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => storage.ReadFileRangeAsync(file, 10, 12));
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => storage.ReadFileRangeAsync(file, 3, 1));
        });
    }

    /// <summary>
    /// Verifies the metadata contains the file name and the user data given at creation.
    /// </summary>
    [Test]
    public async Task GetFileMetadataAsyncReturnsFileNameAndUserData() {
        // Setup: Store a file with user data.
        var storage = new InMemoryFileStorage();
        var userData = new Dictionary<string, string> { ["Owner"] = "tests" };
        var file = await storage.CreateFileAsync("h.txt", "text/plain", new byte[] { 1 }, userData);

        // Act: Read the metadata.
        var metadata = await storage.GetFileMetadataAsync(file);

        // Assert: The file name and user data are present, and unknown keys are not.
        Assert.Multiple(() => {
            Assert.That(metadata.GetMetadata(IFileMetadata.Keys.FileName), Is.EqualTo("h.txt"));
            Assert.That(metadata.GetMetadata("Owner"), Is.EqualTo("tests"));
            Assert.That(metadata.GetMetadata("Missing"), Is.Null);
            Assert.That(metadata.TryGetMetadata("Owner", out var owner), Is.True);
            Assert.That(owner, Is.EqualTo("tests"));
            Assert.That(metadata.TryGetMetadata("Missing", out _), Is.False);
        });
    }

    /// <summary>
    /// Verifies metadata keys are exposed in lowercase and matched regardless of casing, as in the
    /// FileStorage SDK.
    /// </summary>
    [Test]
    public async Task GetFileMetadataAsyncExposesLowercaseKeysAndMatchesAnyCasing() {
        // Setup: Store a file with a PascalCase user data key.
        var storage = new InMemoryFileStorage();
        var userData = new Dictionary<string, string> { ["Owner"] = "x" };
        var file = await storage.CreateFileAsync("a.txt", "text/plain", new byte[] { 1 }, userData);

        // Act: Read the metadata.
        var metadata = await storage.GetFileMetadataAsync(file);

        // Assert: Keys come back lowercase, and every lookup ignores casing.
        Assert.Multiple(() => {
            Assert.That(metadata.UserData.Keys, Is.EquivalentTo(new[] { "owner", "filename" }));
            Assert.That(metadata.GetMetadata("OWNER"), Is.EqualTo("x"));
            Assert.That(metadata.TryGetMetadata("oWnEr", out var owner), Is.True);
            Assert.That(owner, Is.EqualTo("x"));
            Assert.That(metadata.UserData.ContainsKey("Owner"), Is.True);
            Assert.That(metadata.GetMetadata(IFileMetadata.Keys.FileName), Is.EqualTo("a.txt"));
        });
    }

    /// <summary>
    /// Verifies a FileName key in the user data is ignored: the file name always comes from the
    /// fileName argument.
    /// </summary>
    [Test]
    public async Task CreateFileAsyncIgnoresFileNameInUserData() {
        // Setup: User data that tries to override the file name.
        var storage = new InMemoryFileStorage();
        var userData = new Dictionary<string, string> { ["Owner"] = "x", ["FileName"] = "evil.txt" };

        // Act: Store the file and read its metadata.
        var file = await storage.CreateFileAsync("a.txt", "text/plain", new byte[] { 1 }, userData);
        var metadata = await storage.GetFileMetadataAsync(file);

        // Assert: The fileName argument wins and there is a single filename entry.
        Assert.Multiple(() => {
            Assert.That(metadata.GetMetadata(IFileMetadata.Keys.FileName), Is.EqualTo("a.txt"));
            Assert.That(metadata.UserData.Keys, Is.EquivalentTo(new[] { "owner", "filename" }));
        });
    }

    /// <summary>
    /// Verifies the size limit makes create methods throw FileSizeLimitExceededException.
    /// </summary>
    [Test]
    public void CreateFileAsyncThrowsWhenContentExceedsMaxFileSizeBytes() {
        // Setup: Create a storage limited to 3 bytes.
        var storage = new InMemoryFileStorage { MaxFileSizeBytes = 3 };

        // Act and Assert: Storing 5 bytes exceeds the limit.
        Assert.ThrowsAsync<FileSizeLimitExceededException>(
            () => storage.CreateFileAsync("i.txt", "text/plain", Encoding.UTF8.GetBytes("hello")));
    }

    /// <summary>
    /// Verifies reading a file that was never stored fails with FileNotFoundException.
    /// </summary>
    [Test]
    public async Task ReadFileAsyncThrowsWhenFileDoesNotExist() {
        // Setup: A file stored in one storage is unknown to another storage.
        var otherStorage = new InMemoryFileStorage();
        var unknownFile = await otherStorage.CreateFileAsync("j.txt", "text/plain", new byte[] { 1 });
        var storage = new InMemoryFileStorage();

        // Act and Assert: Reading the unknown file fails.
        Assert.ThrowsAsync<FileNotFoundException>(() => storage.ReadFileAsync(unknownFile));
    }

    /// <summary>
    /// Verifies FileStorage.GetInstance() returns the in-memory fake once it is registered with
    /// FileStorage.Configure, as the test fixtures do in their SetUp.
    /// </summary>
    [Test]
    public void FileStorageGetInstanceResolvesToInMemoryFileStorage() {
        // Setup: Register the in-memory fake.
        FileStorage.Reset();
        FileStorage.Configure(() => new InMemoryFileStorage());
        try {
            // Act: Ask the SDK for its file storage.
            var storage = FileStorage.GetInstance();

            // Assert: The registered in-memory fake is returned.
            Assert.That(storage, Is.TypeOf<InMemoryFileStorage>());
        } finally {
            FileStorage.Reset();
        }
    }
}
