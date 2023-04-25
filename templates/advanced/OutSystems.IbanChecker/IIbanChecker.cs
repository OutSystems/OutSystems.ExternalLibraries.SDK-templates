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
    }
}