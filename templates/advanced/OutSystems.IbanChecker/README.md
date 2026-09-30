## IBAN (International Bank Account Number) checker: Advanced version 

### Prerequisites

* .NET 8.0 or .NET 10.0 SDK installed.
* An IDE that supports building .NET 8 or .NET 10 projects. For example, Visual Studio, Visual Studio Code, and Jet Brains Rider.
* Basic knowledge of C# programming concepts.
* To use the FileStorage SDK (see [Using the FileStorage SDK](#using-the-filestorage-sdk)), the project needs `OutSystems.ExternalLibraries.SDK` 1.7.0 or later and the `OutSystems.ExternalLibraries.SDK.FileStorage` package. Both are already referenced by this template.

### Usage

1. Load the C# project file, `OutSystems.IbanChecker.csproj`, using a supported IDE.

    Files in the project:

     * **IIbanChecker.cs**: Defines a public interface named `IIbanChecker` decorated with the `OSInterface` attribute. The interface has seven methods:
    
        * `Parse`: Takes an IBAN string as input and returns an `Iban` struct.
        * `TryParse`: Attempts to parse an IBAN string as input and returns a boolean success indicator along with the parsed `Iban` struct.
        * `Validate`: Takes an IBAN string as input as checks it against a specific rule and a list of rejected countries.
        * `Format`: Takes an `Iban` struct and an optional format string as input and returns a formatted string representation of the IBAN.
        * `BulkValidate`: Takes a text file with one IBAN per line (and an optional list of rejected countries) and returns a CSV file with the validation result of each IBAN. It shows how to use the FileStorage SDK for file input and file output.
        * `CalculateSha256`: Takes a file and returns its SHA-256 hash. It shows how to read a file as a stream.
        * `GetFileMetadata`: Takes a file and returns a `FileDetails` struct with its name, content type, size and user metadata. It shows the FileStorage SDK metadata API.

        Each method is exposed as a server action to your ODC apps and libraries.

    * **IbanChecker.cs**: Defines a public class named `IbanChecker` that implements the `IIbanChecker` interface. The class is a convenient wrapper for the `IbanNet` library, an [open-source library](https://github.com/skwasjer/IbanNet) that provides functionality for parsing and validating IBANs. The class contains private fields `_parser` and `_validator`, which are instances of the `IIbanParser` and `IIbanValidator` interfaces. The constructor initializes these instances for use in the class methods. It also gets the FileStorage SDK instance, kept in the private field `_fileStorage`, that the file actions use.

    * **Structures/Iban.cs** Defines a struct named `Iban`, decorated with the `OSStructure` attribute. The struct has four public properties: `Country`, `Bban`, `BankIdentifier`, and `BranchIdentifier`. It's exposed as a structure to your ODC apps and libraries.

    * **Structures/IbanCountry.cs** Defines a struct named `IbanCountry`, decorated with the `OSStructure` attribute. The struct has five public properties: `TwoLetterISORegionName`, `DisplayName`, `NativeName`, `EnglishName`, and `DomesticAccountNumberExample`. It's exposed as a structure to your ODC apps and libraries.

    * **Structures/ValidationResult.cs** Defines a struct named `ValidationResult`, decorated with the `OSStructure` attribute. The struct has three public properties: `AttemptedValue`, `Country`, and `Error`. It's exposed as a structure to your ODC apps and libraries.

    * **Structures/FileDetails.cs** Defines a struct named `FileDetails`, decorated with the `OSStructure` attribute. The struct has four public properties: `FileName`, `ContentType`, `Size`, and `UserData` (a list of `MetadataEntry`). It's exposed as a structure to your ODC apps and libraries.

    * **Structures/MetadataEntry.cs** Defines a struct named `MetadataEntry`, decorated with the `OSStructure` attribute. The struct has two public properties: `Key` and `Value`. It's exposed as a structure to your ODC apps and libraries.

    * **CustomRules/RejectedCountriesRule.cs**: Defines a custom IBAN validation rule, `RejectCountryRule`, to reject specified country codes. It also defines an associated error result class, `CountryNotAcceptedError`, for handling rejected countries.

    Files in the unit test project (`../OutSystems.IbanChecker.UnitTests`):

    * **IbanCheckerTests.cs**: Unit tests for parsing, validating and the structures.

    * **IbanCheckerFileStorageTests.cs**: Unit tests for `BulkValidate`, `CalculateSha256` and `GetFileMetadata`, plus guard tests that keep the test-only `Loader` out of the library.

    * **FileStorageTesting/InMemoryFileStorage.cs**: A simplified in-memory implementation of `IFileStorage` that you can copy to your own test projects.

    * **FileStorageTesting/Loader.cs**: The test-only hook that makes `FileStorage.GetInstance()` return `InMemoryFileStorage`.

    * **FileStorageTesting/InMemoryFileStorageTests.cs**: Unit tests for `InMemoryFileStorage`.

    See [Unit testing code that uses FileStorage](#unit-testing-code-that-uses-filestorage).

1. Edit the code to meet your use case. If your project requires unit tests, modify the examples found in `../OutSystems.IbanChecker.UnitTests/IbanCheckerTests.cs` and `../OutSystems.IbanChecker.UnitTests/IbanCheckerFileStorageTests.cs` accordingly.

1. Run the Powershell script `generate_upload_package.ps1` to generate the upload packages. The script produces two ZIP files — `ExternalLibrary_net8.0.zip` and `ExternalLibrary.zip` — one for each supported framework. Upload the one matching your target runtime.

1. Upload the generated ZIP file to the ODC Portal. See the [External Logic feature documentation](https://www.outsystems.com/goto/external-logic-upload) for guidance on how to do this.

### Using the FileStorage SDK

External libraries receive and return files as `IOSFile` handles. The FileStorage SDK (`OutSystems.ExternalLibraries.SDK.FileStorage`) reads and creates the content of those files in ODC file storage. The same code runs on ODC Cloud and ODC Self-Hosted.

Get the SDK instance with `FileStorage.GetInstance()`. `IbanChecker` does this once, in its constructor:

```csharp
private readonly IFileStorage _fileStorage;

public IbanChecker(ILogger logger) {
    // ...
    _fileStorage = FileStorage.GetInstance();
}
```

OSActions are synchronous, so the file actions call one private `async` method and block on it once with `.GetAwaiter().GetResult()`.

#### Which method to pick

| Method | Use it when | Used by this template |
|--------|-------------|-----------------------|
| `CreateFileAsync(byte[])` / `ReadFileAsync` | The payload is small and already in memory. | Not used (snippets below) |
| `CreateFileFromStreamAsync` | You already have a `Stream`, for example from another SDK or an HTTP call. | Not used (snippets below) |
| `CreateFileFromWriterAsync` | You generate the content progressively, so it never needs to be fully in memory. | `BulkValidate` |
| `ReadFileAsStreamAsync` | You process a large file incrementally. | `BulkValidate`, `CalculateSha256` |
| `ReadFileRangeAsync` | You only need part of a file, such as a preview or a header. `endByte` is inclusive. | Not used (snippets below) |

`GetFileMetadataAsync` returns the metadata of a file, and is used by `GetFileMetadata`.

#### Snippets for the methods the actions do not call

Create a small file from a byte array and read it back:

```csharp
byte[] content = Encoding.UTF8.GetBytes("hello");
IOSFile file = await _fileStorage.CreateFileAsync("hello.txt", "text/plain", content);
byte[] sameContent = await _fileStorage.ReadFileAsync(file);
```

Create a file from a stream you already have:

```csharp
using Stream source = await httpClient.GetStreamAsync(url);
IOSFile file = await _fileStorage.CreateFileFromStreamAsync("download.bin", "application/octet-stream", source);
```

Read only the first kilobyte of a file. `endByte` is inclusive, so bytes 0 to 1023 are 1024 bytes:

```csharp
await using Stream preview = await _fileStorage.ReadFileRangeAsync(file, startByte: 0, endByte: 1023);
using var reader = new StreamReader(preview);
string firstKilobyte = await reader.ReadToEndAsync();
```

#### User metadata

Every create method accepts an optional `additionalUserData` dictionary that is stored with the file. `BulkValidate` uses it to record the key of the source file and the rejected countries:

```csharp
var userData = new Dictionary<string, string> { ["SourceFileKey"] = ibansFile.Key };
IOSFile output = await _fileStorage.CreateFileFromWriterAsync("validation_results.csv", "text/csv", writeAsync, userData);
```

Read it back with `GetFileMetadataAsync`. The SDK also stores the file name in the metadata, under `IFileMetadata.Keys.FileName`:

```csharp
IFileMetadata metadata = await _fileStorage.GetFileMetadataAsync(file);
string? fileName = metadata.GetMetadata(IFileMetadata.Keys.FileName);
string? sourceFileKey = metadata.GetMetadata("SourceFileKey");
```

#### Exceptions

The SDK throws typed exceptions, all deriving from `FileStorageException` (namespace `OutSystems.ExternalLibraries.SDK.FileStorage.Exceptions`):

* `FileSizeLimitExceededException`: Raised when a file is larger than the size limit of ODC file storage. `BulkValidate` logs a dedicated message for it, because the fix is to split the input file.
* `ConcurrentWriteException`: Signals a conflict caused by concurrent writes to the same file. This template does not handle it in code.
* `FileStorageException`: The base class. The file actions catch it, log the error together with the file key, and rethrow it so that the ODC app can handle it.

For more details, see the [main README of the External Libraries SDK](https://www.outsystems.com/goto/external-logic-readme).

### Unit testing code that uses FileStorage

Outside ODC there is no file storage to talk to. To solve that, the unit test project provides a hook in `FileStorageTesting/Loader.cs`. With this hook, `FileStorage.GetInstance()` returns `InMemoryFileStorage`, a simplified version of `IFileStorage`, instead of the real ODC file storage. This way the library code stays exactly as you would write it for ODC, with no constructor overloads or `InternalsVisibleTo` added for testing.

> [!WARNING]
> `Loader` must only exist in test projects. If it were added to the External Library project, the published library would store files in memory instead of in ODC. The test `LibraryAssemblyDoesNotContainFileStorageLoader` in `IbanCheckerFileStorageTests.cs` fails if that happens.

#### Reuse it in your own tests

Copy the `FileStorageTesting/` folder into your own test project. `InMemoryFileStorage.cs` and `Loader.cs` are required, and `InMemoryFileStorageTests.cs` is optional. Only the `namespace` needs to change. `InMemoryFileStorage` implements the full `IFileStorage` interface, including the methods this template does not use (`CreateFileAsync`, `CreateFileFromStreamAsync`, `ReadFileAsync` and `ReadFileRangeAsync`), and depends only on the two OutSystems SDK packages.

> [!WARNING]
> `InMemoryFileStorage` is a simplified version of `IFileStorage` and cannot mimic all the behavior of the ODC File Storage SDK. Tests that pass against it don't guarantee the same results in ODC, so always test your library in ODC before you release it.

Reset the SDK before and after each test so that every test starts with an empty storage, and seed input files with `_storage.CreateFileAsync`:

```csharp
private IFileStorage _storage = null!;
private IbanChecker _checker = null!;

[SetUp]
public void SetUp() {
    FileStorage.Reset();                    // fresh storage for each test
    _storage = FileStorage.GetInstance();   // InMemoryFileStorage, provided by the Loader
    _checker = new IbanChecker(new LoggerFactory().CreateLogger<IbanChecker>());
}

[TearDown]
public void TearDown() => FileStorage.Reset();

[Test]
public async Task CalculateSha256MatchesKnownDigest() {
    IOSFile file = await _storage.CreateFileAsync("input.txt", "text/plain", Encoding.UTF8.GetBytes("hello"));

    string hash = _checker.CalculateSha256(file);

    Assert.That(hash, Is.EqualTo("2CF24DBA5FB0A30E26E83B2AC5B9E29E1B161E5C1FA7425E73043362938B9824"));
}
```

To test how your code handles the size limit, set `MaxFileSizeBytes`. Creating a larger file then throws `FileSizeLimitExceededException`, as in `BulkValidateRethrowsFileSizeLimitExceeded`. `MaxFileSizeBytes` is a test-only feature of `InMemoryFileStorage` and is not part of `IFileStorage`, so set it through a small helper that does the cast:

```csharp
// Test-only feature of InMemoryFileStorage, not part of IFileStorage.
private void SetMaxAllowedFileSizeBytes(long maxBytes) {
    ((InMemoryFileStorage)_storage).MaxFileSizeBytes = maxBytes;
}

SetMaxAllowedFileSizeBytes(10);
Assert.Throws<FileSizeLimitExceededException>(() => _checker.BulkValidate(input));
```

_(Excerpted from the [main README of the External Libraries SDK](https://www.outsystems.com/goto/external-logic-readme), please refer to that document for additional guidance.)_