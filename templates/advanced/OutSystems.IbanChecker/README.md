## IBAN (International Bank Account Number) checker: Advanced version 

### Prerequisites

* .NET 8.0 or .NET 10.0 SDK installed.
* An IDE that supports building .NET 8 or .NET 10 projects. For example, Visual Studio, Visual Studio Code, and Jet Brains Rider.
* Basic knowledge of C# programming concepts.
* To use the FileStorage SDK (see [Using the FileStorage SDK](#using-the-filestorage-sdk)), the project needs `OutSystems.ExternalLibraries.SDK` 1.7.0 or later and `OutSystems.ExternalLibraries.SDK.FileStorage` 1.2.1 or later. Both are already referenced by this template.

### Usage

1. Load the C# project file, `OutSystems.IbanChecker.csproj`, using a supported IDE.

    Files in the project:

     * **IIbanChecker.cs**: Defines a public interface named `IIbanChecker` decorated with the `OSInterface` attribute. The interface has six methods:
    
        * `Parse`: Takes an IBAN string as input and returns an `Iban` struct.
        * `TryParse`: Attempts to parse an IBAN string as input and returns a boolean success indicator along with the parsed `Iban` struct.
        * `Validate`: Takes an IBAN string as input as checks it against a specific rule and a list of rejected countries.
        * `Format`: Takes an `Iban` struct and an optional format string as input and returns a formatted string representation of the IBAN.
        * `BulkValidate`: Takes a text file with one IBAN per line (and an optional list of rejected countries) and returns a CSV file with the validation result of each IBAN. It shows how to use the FileStorage SDK for file input and file output.
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

    * **IbanCheckerFileStorageTests.cs**: Unit tests for `BulkValidate` and `GetFileMetadata`, and for how the fake is registered with `FileStorage.Configure`.

    * **FileStorageTesting/InMemoryFileStorage.cs**: A simplified in-memory implementation of `IFileStorage` that you can copy to your own test projects.

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

> [!NOTE]
> Because the constructor calls `GetInstance()`, creating an `IbanChecker` needs a registered file storage, even to run the actions that don't use files. That is why `IbanCheckerTests` also registers `InMemoryFileStorage`. If that matters for your library, get the instance only when a file action runs, for example with `private static IFileStorage Storage => FileStorage.GetInstance();`.

OSActions are synchronous, so the file actions call one private `async` method and block on it once with `.GetAwaiter().GetResult()`.

#### Which method to pick

| Method | Use it when | Used by this template |
|--------|-------------|-----------------------|
| `CreateFileAsync(byte[])` / `ReadFileAsync` | The payload is small and already in memory. | Not used (snippets below) |
| `CreateFileFromStreamAsync` | You already have a `Stream`, for example from another SDK or an HTTP call. | Not used (snippets below) |
| `CreateFileFromWriterAsync` | You generate the content progressively, so it never needs to be fully in memory. | `BulkValidate` |
| `ReadFileAsStreamAsync` | You process a large file incrementally. | `BulkValidate` |
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
var userData = new Dictionary<string, string> { ["sourcefilekey"] = ibansFile.Key };
IOSFile output = await _fileStorage.CreateFileFromWriterAsync("validation_results.csv", "text/csv", writeAsync, userData);
```

Read it back with `GetFileMetadataAsync`. The SDK also stores the file name in the metadata, under `IFileMetadata.Keys.FileName`:

```csharp
IFileMetadata metadata = await _fileStorage.GetFileMetadataAsync(file);
string? fileName = metadata.GetMetadata(IFileMetadata.Keys.FileName);
string? sourceFileKey = metadata.GetMetadata("sourcefilekey");
```

The SDK applies these rules to user metadata:

* Keys are case-insensitive and always read back in lowercase. `BulkValidate` writes `sourcefilekey` in lowercase and `RejectedCountries` in mixed case, and `UserData` lists them as `sourcefilekey` and `rejectedcountries`, with the file name as `filename`. Lookups ignore casing, so `GetMetadata("RejectedCountries")` still finds the `rejectedcountries` entry. That's why `GetFileMetadata` compares the key with `IFileMetadata.Keys.FileName` ignoring case, to leave the file name out of the user data it returns. Keys that differ only in casing are rejected.
* Keys may only contain ASCII letters, digits and a few symbols such as `-`, `_` and `.` (no spaces). Values, and the file name, must be printable ASCII. The metadata, file name included, is limited to about 1500 bytes. Invalid metadata throws `ArgumentException` before anything is stored.

#### Exceptions

Storage failures are raised as typed exceptions deriving from `FileStorageException` (namespace `OutSystems.ExternalLibraries.SDK.FileStorage.Exceptions`):

* `FileSizeLimitExceededException`: Raised when a file is larger than the size limit of ODC file storage. `BulkValidate` logs a dedicated message for it, because the fix is to split the input file.
* `ConcurrentWriteException`: Signals a conflict caused by concurrent writes to the same file. This template does not handle it in code.
* `FileStorageException`: The base class. The file actions catch it, log the error together with the file key, and rethrow it so that the ODC app can handle it.

`BulkValidate` can also throw an `ArgumentException`, when the rejected countries can't be stored as file metadata (see the rules above). The file actions don't catch it.

For more details, see the [main README of the External Libraries SDK](https://www.outsystems.com/goto/external-logic-readme).

### Unit testing code that uses FileStorage

`FileStorage.GetInstance()` returns whatever was registered with `FileStorage.Configure`. In ODC, the External Library runtime registers the real file storage before any action runs. Outside ODC there is no file storage to talk to, so the unit tests register `InMemoryFileStorage`, a simplified version of `IFileStorage`, instead. This way the library code stays exactly as you would write it for ODC, with no constructor overloads or `InternalsVisibleTo` added for testing.

> [!WARNING]
> **Never call `FileStorage.Configure` or `FileStorage.Reset` from library code**; only test projects call them. If the External Library called them, the published library would replace or drop the ODC file storage.

#### Reuse it in your own tests

Copy the `FileStorageTesting/` folder into your own test project. Only `InMemoryFileStorage.cs` is required, and `InMemoryFileStorageTests.cs` is optional. Only the `namespace` needs to change. `InMemoryFileStorage` implements the full `IFileStorage` interface, including the methods this template does not use (`CreateFileAsync`, `CreateFileFromStreamAsync`, `ReadFileAsync` and `ReadFileRangeAsync`), and depends only on the two OutSystems SDK packages.

> [!WARNING]
> `InMemoryFileStorage` is a simplified version of `IFileStorage`. It does not validate metadata as the ODC File Storage SDK does (see the [metadata rules](https://www.outsystems.com/tk/redirect?g=b2fb0646-b8e6-4098-b262-65d03ab3cf99)) and cannot mimic all of its behavior. Tests that pass against it don't guarantee the same results in ODC, so always test your library in ODC before you release it.

In every `SetUp`, call `FileStorage.Reset()` and then register a new fake with `FileStorage.Configure`, and call `Reset()` again in `TearDown`. `Reset()` drops both the cached instance and the registered factory, so every test starts with a new, empty storage. Seed input files with `_storage.CreateFileAsync`:

```csharp
private InMemoryFileStorage _storage = null!;
private IbanChecker _checker = null!;

[SetUp]
public void SetUp() {
    // Reset() first: Configure alone doesn't replace an instance that is already cached.
    FileStorage.Reset();
    _storage = new InMemoryFileStorage();
    FileStorage.Configure(() => _storage);
    _checker = new IbanChecker(new LoggerFactory().CreateLogger<IbanChecker>());
}

[TearDown]
public void TearDown() => FileStorage.Reset();

[Test]
public async Task GetFileMetadataReturnsFileDetails() {
    IOSFile file = await _storage.CreateFileAsync("input.txt", "text/plain", Encoding.UTF8.GetBytes("hello"));

    var details = _checker.GetFileMetadata(file);

    Assert.That(details.FileName, Is.EqualTo("input.txt"));
    Assert.That(details.Size, Is.EqualTo(5));
}
```

`FileStorage` is process-wide state, so mark every fixture that uses it `[NonParallelizable]`, as `IbanCheckerTests` and `IbanCheckerFileStorageTests` do. That also overrides an assembly-level `[Parallelizable]` setting.

To test how your code handles the size limit, set `MaxFileSizeBytes`. Creating a larger file then throws `FileSizeLimitExceededException`, as in `BulkValidateRethrowsFileSizeLimitExceeded`. `MaxFileSizeBytes` is a test-only feature of `InMemoryFileStorage` and is not part of `IFileStorage`, which is why the fixture keeps the fake in a field of type `InMemoryFileStorage`:

```csharp
_storage.MaxFileSizeBytes = 10;
Assert.Throws<FileSizeLimitExceededException>(() => _checker.BulkValidate(input));
```

_(Excerpted from the [main README of the External Libraries SDK](https://www.outsystems.com/goto/external-logic-readme), please refer to that document for additional guidance.)_