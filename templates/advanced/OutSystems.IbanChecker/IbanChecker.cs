using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IbanNet;
using IbanNet.Registry;
using Microsoft.Extensions.Logging;
using OutSystems.ExternalLibraries.SDK;
using OutSystems.ExternalLibraries.SDK.FileStorage;
using OutSystems.ExternalLibraries.SDK.FileStorage.Exceptions;

namespace OutSystems.IbanChecker {
    /// <summary>
    /// The IbanChecker class implements the IIbanChecker interface, providing
    /// the actual functionality for parsing and validating IBANs according to
    /// the rules defined by the IbanNet library.
    /// </summary>
    public class IbanChecker : IIbanChecker {
        /// <summary>
        /// An instance of IIbanParser from the IbanNet library, used for
        /// parsing IBAN strings.
        /// </summary>
        private IIbanParser _parser;

        /// <summary>
        /// An instance of IIbanValidator from the IbanNet library, used for
        /// validating parsed IBANs.
        /// </summary>
        private IIbanValidator _validator;

        /// <summary>
        /// An instance of ILogger from the Microsoft.Extensions.Logging library, used for
        /// logging.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The FileStorage SDK instance used to read and create files in ODC file storage.
        /// </summary>
        private readonly IFileStorage _fileStorage;

        /// <summary>
        /// The constructor initializes the IbanChecker class with the specified logger and creates new instances
        /// of the IbanParser and IbanValidator classes from the IbanNet library. It also gets the FileStorage
        /// SDK instance used by the file actions.
        /// </summary>
        /// <param name="logger">The logger instance used for logging operations within the IbanChecker.</param>
        public IbanChecker(ILogger logger) {
            _validator = new IbanValidator();
            _parser = new IbanParser(_validator);
            _logger = logger;
            // The recommended way to access ODC file storage from an External Library.
            _fileStorage = FileStorage.GetInstance();
        }

        /// <summary>
        /// The Parse method takes a string value representing an IBAN number and
        /// attempts to parse it using the IIbanParser instance. If the parsing is
        /// successful, it returns a Iban struct.
        /// </summary>
        /// <param name="value">The IBAN string to be parsed.</param>
        /// <returns>An Iban struct representing the parsed IBAN.</returns>
        /// <exception cref="System.Exception">Thrown if the parsing fails.</exception>
        public Structures.Iban Parse(string value) {
            _logger.LogInformation("Parsing IBAN: {IbanValue}", value);
            return new Structures.Iban(_parser.Parse(value));
        }

        /// <summary>
        /// The TryParse method attempts to parse the given string value into an
        /// Iban struct using the IIbanParser instance. Returns a boolean
        /// indicating success and the Iban struct if successful.
        /// </summary>
        /// <param name="value">The IBAN string to be parsed.</param>
        /// <param name="iban">The parsed Iban struct if successful, null otherwise.</param>
        /// <returns>A boolean indicating whether the parsing was successful. </returns>
        public bool TryParse(string value, out Structures.Iban? iban) {
            iban = null;
            IbanNet.Iban? internalIban;
            if (_parser.TryParse(value, out internalIban)) {
                iban = new Structures.Iban(internalIban);
                _logger.LogInformation("Successfully parsed IBAN: {IbanValue}", value);
                return true;
            }
            _logger.LogWarning("Failed to parse IBAN: {IbanValue}", value);
            return false;
        }

        /// <summary>
        /// The Validate method checks the given IBAN string against a specific
        /// rule and a list of rejected countries, returning a ValidationResult
        /// structure. It uses the internal validator instance or a custom validator
        /// if rejected countries are provided.
        /// </summary>
        /// <param name="iban">The IBAN string to be validated.</param>
        /// <param name="rejectedCountries">An optional list of country codes to be rejected during validation.</param>
        /// <returns>A ValidationResult struct containing the validation results.</returns>
        public Structures.ValidationResult Validate(string iban, IEnumerable<string>? rejectedCountries = null) {
            if (rejectedCountries != null && rejectedCountries.Any()) {
                _logger.LogInformation("Validating IBAN: {IbanValue} with rejected countries: {RejectedCountries}", iban, rejectedCountries);
            } else {
                _logger.LogInformation("Validating IBAN: {IbanValue} with default rules", iban);
            }
            return new Structures.ValidationResult(GetValidator(rejectedCountries).Validate(iban));
        }

        /// <summary>
        /// Returns the default validator, or a validator that also rejects the given countries.
        /// </summary>
        /// <param name="rejectedCountries">An optional list of country codes to be rejected during validation.</param>
        private IIbanValidator GetValidator(IEnumerable<string>? rejectedCountries) {
            if (rejectedCountries == null || !rejectedCountries.Any()) {
                return _validator;
            }
            return new IbanValidator(new IbanValidatorOptions {
                Rules = { new CustomRules.RejectCountryRule(rejectedCountries) }
            });
        }

        /// <summary>
        /// The Format method takes a Iban struct and an optional format string,
        /// and returns a formatted string representation of the IBAN. It uses the
        /// IbanNet.Builders.IbanBuilder class to reconstruct the IBAN string based
        /// on the provided format.
        /// </summary>
        /// <param name="iban">The Iban struct to be formatted.</param>
        /// <param name="format">An optional format string for the output IBAN string.</param>
        /// <returns>A formatted string representation of the provided IBAN.</returns>
        /// <exception cref="System.Exception">Thrown if the country code is invalid.</exception>
        public string Format(Structures.Iban iban, string? format = null) {
            var ibanBuilder = new IbanNet.Builders.IbanBuilder();
            IbanRegistry.Default.TryGetValue(iban.Country.TwoLetterISORegionName, out IbanCountry? country);
            if (country == null) {
                var errorMessage = "Invalid country: " + iban.Country.TwoLetterISORegionName;
                _logger.LogError("Failed to format IBAN. {ErrorMessage}.", errorMessage);
                throw new System.Exception(errorMessage);
            }
            var ib = _parser.Parse(ibanBuilder
                .WithCountry(country)
                .WithBankAccountNumber(iban.Bban)
                .Build());
            _logger.LogInformation("Formatting IBAN for country: {CountryCode} with format: {Format}.",
                iban.Country.TwoLetterISORegionName, format ?? "<default>");
            return ib.ToString(format);
        }

        /// <summary>
        /// The BulkValidate method reads a text file with one IBAN per line and creates a CSV file
        /// (Iban,IsValid,Country,Error) with the validation result of each non-blank line. The file
        /// is read and written as streams, so it is never fully held in memory.
        /// </summary>
        /// <param name="ibansFile">A text file with one IBAN per line.</param>
        /// <param name="rejectedCountries">An optional list of country codes to be rejected during validation.</param>
        /// <returns>The CSV file with the validation results.</returns>
        /// <exception cref="FileStorageException">Thrown if the FileStorage SDK fails to read or create a file.</exception>
        public IOSFile BulkValidate(IOSFile ibansFile, IEnumerable<string>? rejectedCountries = null) {
            _logger.LogInformation("Bulk validating IBANs from file {FileKey}", ibansFile.Key);
            try {
                // OSActions are synchronous; block once on the async FileStorage work.
                return BulkValidateAsync(ibansFile, rejectedCountries).GetAwaiter().GetResult();
            } catch (FileSizeLimitExceededException ex) {
                _logger.LogError(ex, "Validation results for file {FileKey} exceed the file size limit. Split the input file.", ibansFile.Key);
                throw;
            } catch (FileStorageException ex) {
                _logger.LogError(ex, "FileStorage error while bulk validating file {FileKey}", ibansFile.Key);
                throw;
            }
        }

        private Task<IOSFile> BulkValidateAsync(IOSFile ibansFile, IEnumerable<string>? rejectedCountries,
            CancellationToken cancellationToken = default) {
            var validator = GetValidator(rejectedCountries);
            // User metadata is stored with the new file and can be read back with GetFileMetadata.
            var userData = new Dictionary<string, string> { ["SourceFileKey"] = ibansFile.Key };
            if (rejectedCountries != null && rejectedCountries.Any()) {
                userData["RejectedCountries"] = string.Join(",", rejectedCountries);
            }

            return _fileStorage.CreateFileFromWriterAsync("validation_results.csv", "text/csv",
                async (destination, token) => {
                    await using var source = await _fileStorage.ReadFileAsStreamAsync(ibansFile, token);
                    using var reader = new StreamReader(source);
                    // The FileStorage SDK owns the destination stream, so leave it open.
                    await using var writer = new StreamWriter(destination, new UTF8Encoding(false), leaveOpen: true);
                    await writer.WriteLineAsync("Iban,IsValid,Country,Error");

                    string? line;
                    while ((line = await reader.ReadLineAsync(token)) is not null) {
                        string iban = line.Trim();
                        if (iban.Length == 0) {
                            continue;
                        }
                        var result = validator.Validate(iban);
                        string isValid = result.IsValid ? "true" : "false";
                        string country = result.Country?.TwoLetterISORegionName ?? string.Empty;
                        string error = result.Error?.ErrorMessage ?? string.Empty;
                        await writer.WriteLineAsync($"{Csv(iban)},{isValid},{Csv(country)},{Csv(error)}");
                    }
                }, userData, cancellationToken);
        }

        /// <summary>
        /// Escapes a value for a CSV field: values containing a comma, quote or line break are
        /// wrapped in quotes, with inner quotes doubled.
        /// </summary>
        private static string Csv(string value) {
            return value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0
                ? value
                : "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        /// <summary>
        /// The CalculateSha256 method reads a file as a stream and returns its SHA-256 hash as an
        /// uppercase hexadecimal text.
        /// </summary>
        /// <param name="file">The file to be hashed.</param>
        /// <returns>The SHA-256 hash of the file content.</returns>
        /// <exception cref="FileStorageException">Thrown if the FileStorage SDK fails to read the file.</exception>
        public string CalculateSha256(IOSFile file) {
            _logger.LogInformation("Calculating SHA-256 for file {FileKey}", file.Key);
            try {
                // OSActions are synchronous; block once on the async FileStorage work.
                return CalculateSha256Async(file).GetAwaiter().GetResult();
            } catch (FileStorageException ex) {
                _logger.LogError(ex, "FileStorage error while hashing file {FileKey}", file.Key);
                throw;
            }
        }

        private async Task<string> CalculateSha256Async(IOSFile file, CancellationToken cancellationToken = default) {
            // Streams the file, so large files are hashed without loading them into memory.
            await using var stream = await _fileStorage.ReadFileAsStreamAsync(file, cancellationToken);
            byte[] hash = await SHA256.HashDataAsync(stream, cancellationToken);
            return Convert.ToHexString(hash);
        }

        /// <summary>
        /// The GetFileMetadata method returns the name, content type, size and user metadata of a
        /// file. The file name is stored by the FileStorage SDK as metadata too, but is returned
        /// as its own field instead of as a user metadata entry.
        /// </summary>
        /// <param name="file">The file to be described.</param>
        /// <returns>A FileDetails structure describing the file.</returns>
        /// <exception cref="FileStorageException">Thrown if the FileStorage SDK fails to read the file metadata.</exception>
        public Structures.FileDetails GetFileMetadata(IOSFile file) {
            _logger.LogInformation("Getting metadata for file {FileKey}", file.Key);
            try {
                // OSActions are synchronous; block once on the async FileStorage work.
                return GetFileMetadataAsync(file).GetAwaiter().GetResult();
            } catch (FileStorageException ex) {
                _logger.LogError(ex, "FileStorage error while getting metadata of file {FileKey}", file.Key);
                throw;
            }
        }

        private async Task<Structures.FileDetails> GetFileMetadataAsync(IOSFile file, CancellationToken cancellationToken = default) {
            var metadata = await _fileStorage.GetFileMetadataAsync(file, cancellationToken);
            var details = new Structures.FileDetails(
                metadata.GetMetadata(IFileMetadata.Keys.FileName) ?? string.Empty, file.ContentType, file.Size);
            foreach (var entry in metadata.UserData.Where(e => e.Key != IFileMetadata.Keys.FileName)) {
                details.UserData.Add(new Structures.MetadataEntry(entry.Key, entry.Value));
            }
            return details;
        }
    }
}
