using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using OutSystems.ExternalLibraries.SDK;
using OutSystems.ExternalLibraries.SDK.FileStorage;
using OutSystems.ExternalLibraries.SDK.FileStorage.Exceptions;
using OutSystems.IbanChecker.UnitTests.FileStorageTesting;

namespace OutSystems.IbanChecker.UnitTests;

/// <summary>
/// Tests for the IbanChecker actions that use the FileStorage SDK. FileStorage.GetInstance()
/// resolves to <see cref="InMemoryFileStorage"/> through the test-only Loader, so no ODC
/// environment is needed.
/// </summary>
public class IbanCheckerFileStorageTests {

    private const string ValidNlIban = "NL91ABNA0417164300";
    private const string ValidPtIban = "PT50000201231234567890154";

    private IFileStorage _storage = null!;
    private IbanChecker _checker = null!;

    [SetUp]
    public void SetUp() {
        // Drop any cached instance so every test gets a fresh, empty in-memory storage.
        FileStorage.Reset();
        _storage = FileStorage.GetInstance();
        _checker = new IbanChecker(new LoggerFactory().CreateLogger<IbanChecker>());
    }

    [TearDown]
    public void TearDown() {
        FileStorage.Reset();
    }

    // Test-only feature of InMemoryFileStorage, not part of IFileStorage.
    private void SetMaxAllowedFileSizeBytes(long maxBytes) {
        ((InMemoryFileStorage)_storage).MaxFileSizeBytes = maxBytes;
    }

    // Stores a text file in the in-memory storage, as an ODC app would have uploaded it.
    private IOSFile SeedTextFile(string content, IDictionary<string, string>? userData = null) {
        return _storage.CreateFileAsync("input.txt", "text/plain", Encoding.UTF8.GetBytes(content), userData)
            .GetAwaiter().GetResult();
    }

    private string ReadText(IOSFile file) {
        return Encoding.UTF8.GetString(_storage.ReadFileAsync(file).GetAwaiter().GetResult());
    }

    private string[] ReadLines(IOSFile file) {
        return ReadText(file).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Verifies FileStorage.GetInstance() returns the in-memory fake. If the Loader was not
    /// discovered, the tests would otherwise silently depend on real storage.
    /// </summary>
    [Test]
    public void FileStorageResolvesToInMemoryFileStorage() {
        // Setup: SetUp already reset the SDK and asked for its instance.

        // Act: Ask the SDK for its file storage again.
        var storage = FileStorage.GetInstance();

        // Assert: The Loader provided the in-memory fake, and it is the cached instance.
        Assert.That(storage, Is.InstanceOf<InMemoryFileStorage>());
        Assert.That(storage, Is.SameAs(_storage));
    }

    /// <summary>
    /// Guard test: the test-only Loader must never end up in the External Library, otherwise the
    /// published library would store files in memory instead of in ODC.
    /// </summary>
    [Test]
    public void LibraryAssemblyDoesNotContainFileStorageLoader() {
        // Setup: The Loader type name the FileStorage SDK looks for.
        const string loaderTypeName = "OutSystems.ExternalLibraries.SDK.FileStorage.Loader";

        // Act: Look for the Loader in the External Library assembly.
        var loaderType = typeof(IbanChecker).Assembly.GetType(loaderTypeName);

        // Assert: It is not there.
        Assert.That(loaderType, Is.Null);
    }

    /// <summary>
    /// Verifies IbanChecker has a single public constructor taking only an ILogger, so ODC can
    /// instantiate it and there is no test-only constructor in the library.
    /// </summary>
    [Test]
    public void IbanCheckerHasSinglePublicLoggerConstructor() {
        // Setup: The IbanChecker type.
        var type = typeof(IbanChecker);

        // Act: Get its public instance constructors.
        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        // Assert: There is exactly one, and its only parameter is an ILogger.
        Assert.That(constructors, Has.Length.EqualTo(1));
        Assert.That(constructors[0].GetParameters().Select(p => p.ParameterType),
            Is.EqualTo(new[] { typeof(ILogger) }));
    }

    /// <summary>
    /// Tests BulkValidate writes the header and one row per non-blank line, with the validity,
    /// country and error of valid, invalid and rejected-country IBANs.
    /// </summary>
    [Test]
    public void BulkValidateWritesHeaderAndOneRowPerIban() {
        // Setup: A file with a valid NL IBAN, an invalid text, a blank line and a PT IBAN that
        // will be rejected.
        var input = SeedTextFile($"{ValidNlIban}\nnot-an-iban\n   \n{ValidPtIban}\n");

        // Act: Bulk validate rejecting PT.
        var output = _checker.BulkValidate(input, new[] { "PT" });
        var lines = ReadLines(output);

        // Assert: Header plus 3 rows; the blank line is skipped and the country is kept for
        // the rejected row but blank when none is detected.
        Assert.That(lines, Has.Length.EqualTo(4));
        Assert.Multiple(() => {
            Assert.That(lines[0], Is.EqualTo("Iban,IsValid,Country,Error"));
            Assert.That(lines[1], Is.EqualTo($"{ValidNlIban},true,NL,"));
            Assert.That(lines[2], Does.StartWith("not-an-iban,false,,"));
            Assert.That(lines[2], Has.Length.GreaterThan("not-an-iban,false,,".Length));
            Assert.That(lines[3], Does.StartWith($"{ValidPtIban},false,PT,"));
            Assert.That(lines[3], Has.Length.GreaterThan($"{ValidPtIban},false,PT,".Length));
        });
    }

    /// <summary>
    /// Tests BulkValidate quotes CSV fields that contain a comma.
    /// </summary>
    [Test]
    public void BulkValidateQuotesValuesContainingCommas() {
        // Setup: A file whose only line contains a comma.
        var input = SeedTextFile("a,b\n");

        // Act: Bulk validate the file.
        var lines = ReadLines(_checker.BulkValidate(input));

        // Assert: The value is quoted so the row keeps its 4 columns.
        Assert.That(lines[1], Does.StartWith("\"a,b\",false,,"));
    }

    /// <summary>
    /// Tests BulkValidate attaches the source file key and the rejected countries as user
    /// metadata of the output file, and omits the rejected countries when there are none.
    /// </summary>
    [Test]
    public async Task BulkValidateAttachesUserMetadata() {
        // Setup: An input file.
        var input = SeedTextFile($"{ValidNlIban}\n");

        // Act: Bulk validate with and without rejected countries and read the output metadata.
        var withRejected = await _storage.GetFileMetadataAsync(_checker.BulkValidate(input, new[] { "PT", "ES" }));
        var withoutRejected = await _storage.GetFileMetadataAsync(_checker.BulkValidate(input));

        // Assert: RejectedCountries is only present when countries were given.
        Assert.Multiple(() => {
            Assert.That(withRejected.GetMetadata("SourceFileKey"), Is.EqualTo(input.Key));
            Assert.That(withRejected.GetMetadata("RejectedCountries"), Is.EqualTo("PT,ES"));
            Assert.That(withoutRejected.GetMetadata("SourceFileKey"), Is.EqualTo(input.Key));
            Assert.That(withoutRejected.UserData.ContainsKey("RejectedCountries"), Is.False);
        });
    }

    /// <summary>
    /// Tests BulkValidate creates a CSV file named validation_results.csv.
    /// </summary>
    [Test]
    public async Task BulkValidateCreatesCsvFile() {
        // Setup: An input file.
        var input = SeedTextFile($"{ValidNlIban}\n");

        // Act: Bulk validate the file.
        var output = _checker.BulkValidate(input);
        var metadata = await _storage.GetFileMetadataAsync(output);

        // Assert: The output has the CSV content type and file name.
        Assert.Multiple(() => {
            Assert.That(output.ContentType, Is.EqualTo("text/csv"));
            Assert.That(metadata.GetMetadata(IFileMetadata.Keys.FileName), Is.EqualTo("validation_results.csv"));
        });
    }

    /// <summary>
    /// Tests BulkValidate rethrows FileSizeLimitExceededException when the output is larger than
    /// the storage limit.
    /// </summary>
    [Test]
    public void BulkValidateRethrowsFileSizeLimitExceeded() {
        // Setup: An input file and a storage that only accepts files up to 10 bytes.
        var input = SeedTextFile($"{ValidNlIban}\n");
        SetMaxAllowedFileSizeBytes(10);

        // Act and Assert: The output does not fit, so the SDK exception reaches the caller.
        Assert.Throws<FileSizeLimitExceededException>(() => _checker.BulkValidate(input));
    }

    /// <summary>
    /// Tests GetFileMetadata does not swallow errors that are not FileStorageExceptions.
    /// </summary>
    [Test]
    public async Task GetFileMetadataThrowsWhenFileDoesNotExist() {
        // Setup: A file that only exists in another storage.
        var unknownFile = await new InMemoryFileStorage().CreateFileAsync("x.txt", "text/plain", new byte[] { 1 });

        // Act and Assert: The fake reports the missing file.
        Assert.Throws<FileNotFoundException>(() => _checker.GetFileMetadata(unknownFile));
    }

    /// <summary>
    /// Tests GetFileMetadata returns the name, content type, size and user data of a file, without
    /// the FileName entry.
    /// </summary>
    [Test]
    public void GetFileMetadataReturnsDetailsWithoutFileNameInUserData() {
        // Setup: A file with user data.
        var file = SeedTextFile("hello", new Dictionary<string, string> { ["Owner"] = "tests" });

        // Act: Get the file details.
        var details = _checker.GetFileMetadata(file);

        // Assert: The details are populated and FileName is not repeated in the user data.
        Assert.Multiple(() => {
            Assert.That(details.FileName, Is.EqualTo("input.txt"));
            Assert.That(details.ContentType, Is.EqualTo("text/plain"));
            Assert.That(details.Size, Is.EqualTo(5));
            Assert.That(details.UserData, Has.Count.EqualTo(1));
            Assert.That(details.UserData[0].Key, Is.EqualTo("Owner"));
            Assert.That(details.UserData[0].Value, Is.EqualTo("tests"));
        });
    }

    /// <summary>
    /// Tests the metadata of a BulkValidate output shows the source file it was created from.
    /// </summary>
    [Test]
    public void GetFileMetadataOfBulkValidateOutputShowsSourceFileKey() {
        // Setup: Bulk validate a file.
        var input = SeedTextFile($"{ValidNlIban}\n");
        var output = _checker.BulkValidate(input);

        // Act: Get the details of the output file.
        var details = _checker.GetFileMetadata(output);

        // Assert: The details describe the CSV and point back to the input file.
        Assert.Multiple(() => {
            Assert.That(details.FileName, Is.EqualTo("validation_results.csv"));
            Assert.That(details.ContentType, Is.EqualTo("text/csv"));
            Assert.That(details.UserData.Select(e => e.Key), Is.EqualTo(new[] { "SourceFileKey" }));
            Assert.That(details.UserData[0].Value, Is.EqualTo(input.Key));
        });
    }

    /// <summary>
    /// Verifies the new actions are decorated with OSActionAttribute, which is required to expose
    /// them as server actions to ODC apps and libraries.
    /// </summary>
    [TestCase("BulkValidate")]
    [TestCase("GetFileMetadata")]
    public void FileActionHasOSActionAttribute(string methodName) {
        // Setup: The interface method.
        var method = typeof(IIbanChecker).GetMethod(methodName);

        // Act: Read its OSAction attribute.
        var attribute = method?.GetCustomAttribute<OSActionAttribute>();

        // Assert: The attribute is present.
        Assert.That(attribute, Is.Not.Null);
    }

    /// <summary>
    /// Verifies BulkValidate returns a file to ODC, which requires ReturnType File.
    /// </summary>
    [Test]
    public void BulkValidateReturnsFile() {
        // Setup and Act: Read the OSAction attribute of BulkValidate.
        var attribute = typeof(IIbanChecker).GetMethod("BulkValidate")?.GetCustomAttribute<OSActionAttribute>();

        // Assert: The return type is File.
        Assert.That(attribute?.ReturnType, Is.EqualTo(OSDataType.File));
    }

    /// <summary>
    /// Verifies every IOSFile parameter of the new actions is declared as a File data type.
    /// </summary>
    [TestCase("BulkValidate")]
    [TestCase("GetFileMetadata")]
    public void FileActionFileParametersHaveFileDataType(string methodName) {
        // Setup: The IOSFile parameters of the interface method.
        var fileParameters = typeof(IIbanChecker).GetMethod(methodName)!.GetParameters()
            .Where(p => p.ParameterType == typeof(IOSFile)).ToList();

        // Act: Read their OSParameter data types.
        var dataTypes = fileParameters.Select(p => p.GetCustomAttribute<OSParameterAttribute>()?.DataType);

        // Assert: There is at least one, and all are File.
        Assert.That(fileParameters, Is.Not.Empty);
        Assert.That(dataTypes, Is.All.EqualTo(OSDataType.File));
    }

    /// <summary>
    /// Verifies the interface keeps the library name, so existing ODC apps keep working.
    /// </summary>
    [Test]
    public void IbanCheckerInterfaceKeepsLibraryName() {
        // Setup and Act: Read the OSInterface attribute.
        var attribute = typeof(IIbanChecker).GetCustomAttribute<OSInterfaceAttribute>();

        // Assert: The name is unchanged.
        Assert.That(attribute?.Name, Is.EqualTo("IbanChecker"));
    }

    /// <summary>
    /// Verifies the new structures are decorated with OSStructureAttribute, which is required to
    /// expose them as structures to ODC apps and libraries.
    /// </summary>
    [Test]
    public void FileStructuresHaveOSStructureAttribute() {
        Assert.Multiple(() => {
            Assert.That(Attribute.GetCustomAttribute(typeof(Structures.FileDetails), typeof(OSStructureAttribute)), Is.Not.Null);
            Assert.That(Attribute.GetCustomAttribute(typeof(Structures.MetadataEntry), typeof(OSStructureAttribute)), Is.Not.Null);
        });
    }
}
