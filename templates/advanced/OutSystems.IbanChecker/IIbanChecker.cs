using System.Collections.Generic;
using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.IbanChecker {
    /// <summary>
    /// The IIbanChecker interface defines the methods (exposed as server actions)
    /// for parsing and validating IBANs.
    /// </summary>
    [OSInterface(Description = "Enables management and validation of IBANs in OutSystems Developer Cloud (ODC) apps.", IconResourceName = "OutSystems.IbanChecker.resources.iban_checker.png", Name = "IbanChecker")]
    public interface IIbanChecker {
        /// <summary>
        /// Parses an IBAN and returns the corresponding Iban structure.
        /// This method is exposed as a server action to your ODC apps and libraries.
        /// </summary>
        [OSAction(Description = "Parses an IBAN and returns the corresponding Iban structure.", IconResourceName = "OutSystems.IbanChecker.resources.parse.png", ReturnName = "IBAN")]
        Structures.Iban Parse(
            [OSParameter(DataType = OSDataType.Text, Description = "The IBAN as a string")]
            string value);

        /// <summary>
        /// Attempts to parse an IBAN, returning a boolean success indicator and a
        /// new Iban structure. This method is exposed as a server action to your
        /// ODC apps and libraries.
        /// </summary>
        [OSAction(Description = "Attempts to parse a given IBAN, returning a boolean success indicator and a new Iban Structure", IconResourceName = "OutSystems.IbanChecker.resources.try_parse.png", ReturnName = "IBAN", ReturnType = OSDataType.Boolean)]
        bool TryParse(
            [OSParameter(DataType = OSDataType.Text, Description = "The IBAN as a string")] 
            string value, 
            [OSParameter(Description = "Output parameter containing the parsed Iban Structure")] 
            out Structures.Iban? iban);

        /// <summary>
        /// Validates an IBAN against a specific rule and a list of rejected countries.
        /// This method is exposed as a server action to your ODC apps and libraries.
        /// </summary>
        [OSAction(Description = "Validates an IBAN against a specific rule and a list of rejected countries", IconResourceName = "OutSystems.IbanChecker.resources.validate.png", ReturnName = "ValidationResult")]
        Structures.ValidationResult Validate(
            [OSParameter(DataType = OSDataType.Text, Description = "The IBAN to be validated")] 
            string iban, 
            [OSParameter(Description = "Optional list of country codes to be rejected during validation")]
            IEnumerable<string>? rejectedCountries = null);

        /// <summary>
        /// Formats an Iban struct into a string representation based on a specified
        /// format.
        /// </summary>
        [OSAction(Description = "Formats an Iban structure into a string representation based on a specified format", IconResourceName = "OutSystems.IbanChecker.resources.format.png", ReturnName = "FormattedIban", ReturnType = OSDataType.Text)]
        public string Format(
            [OSParameter(Description = "The Iban structure to be formatted")]
            Structures.Iban iban, 
            [OSParameter(DataType = OSDataType.Text, Description = "Optional format string for the output")]
            string? format = null);

        /// <summary>
        /// Validates every IBAN in a text file (one IBAN per line) and returns a CSV file with
        /// the result for each line. Demonstrates file input and file output with the
        /// FileStorage SDK. This method is exposed as a server action to your ODC apps and
        /// libraries.
        /// </summary>
        [OSAction(Description = "Validates every IBAN in a text file (one IBAN per line) and returns a CSV file with the result for each one", IconResourceName = "OutSystems.IbanChecker.resources.bulk_validate.png", ReturnName = "ValidationResultsFile", ReturnType = OSDataType.File)]
        IOSFile BulkValidate(
            [OSParameter(DataType = OSDataType.File, Description = "A text file with one IBAN per line")]
            IOSFile ibansFile,
            [OSParameter(Description = "Optional list of country codes to be rejected during validation")]
            IEnumerable<string>? rejectedCountries = null);

        /// <summary>
        /// Calculates the SHA-256 hash of a file. Demonstrates reading a file as a stream with
        /// the FileStorage SDK. This method is exposed as a server action to your ODC apps and
        /// libraries.
        /// </summary>
        [OSAction(Description = "Calculates the SHA-256 hash of a file, as an uppercase hexadecimal text", IconResourceName = "OutSystems.IbanChecker.resources.calculate_sha256.png", ReturnName = "Sha256Hash", ReturnType = OSDataType.Text)]
        string CalculateSha256(
            [OSParameter(DataType = OSDataType.File, Description = "The file to be hashed")]
            IOSFile file);

        /// <summary>
        /// Returns the name, content type, size and user metadata of a file. Demonstrates the
        /// FileStorage SDK metadata API. This method is exposed as a server action to your ODC
        /// apps and libraries.
        /// </summary>
        [OSAction(Description = "Returns the name, content type, size and user metadata of a file", IconResourceName = "OutSystems.IbanChecker.resources.get_file_metadata.png", ReturnName = "FileDetails")]
        Structures.FileDetails GetFileMetadata(
            [OSParameter(DataType = OSDataType.File, Description = "The file to be described")]
            IOSFile file);
    }
}