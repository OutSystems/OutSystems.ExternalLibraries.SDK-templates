using System.Collections.Generic;
using System.Linq;
using IbanNet;
using IbanNet.Registry;
using Microsoft.Extensions.Logging;

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
        /// The constructor initializes the IbanChecker class with the specified logger and creates new instances
        /// of the IbanParser and IbanValidator classes from the IbanNet library.
        /// </summary>
        /// <param name="logger">The logger instance used for logging operations within the IbanChecker.</param>
        public IbanChecker(ILogger logger) {
            _validator = new IbanValidator();
            _parser = new IbanParser(_validator);
            _logger = logger;
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
                var validatorWithRejectedCountries = new IbanValidator(new IbanValidatorOptions {
                    Rules = { new CustomRules.RejectCountryRule(rejectedCountries) }
                });
                _logger.LogInformation("Validating IBAN: {IbanValue} with rejected countries: {RejectedCountries}", iban, rejectedCountries);
                return new Structures.ValidationResult(validatorWithRejectedCountries.Validate(iban));
            }
            _logger.LogInformation("Validating IBAN: {IbanValue} with default rules", iban);
            return new Structures.ValidationResult(_validator.Validate(iban));
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
    }
}
