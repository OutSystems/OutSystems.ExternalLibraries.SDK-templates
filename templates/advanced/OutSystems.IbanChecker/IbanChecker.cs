using System.Collections.Generic;
using System.Linq;
using IbanNet;
using IbanNet.Registry;

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
        /// The constructor initializes the IbanChecker class with new instances
        /// of the IbanParser and IbanValidator classes from the IbanNet library.
        /// </summary>
        public IbanChecker() {
            _validator = new IbanValidator();
            _parser = new IbanParser(_validator);
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
                return true;
            }
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
                return new Structures.ValidationResult(validatorWithRejectedCountries.Validate(iban));
            }
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
                throw new System.Exception("Invalid country: " + iban.Country.TwoLetterISORegionName);
            }
            var ib = _parser.Parse(ibanBuilder
                .WithCountry(country)
                .WithBankAccountNumber(iban.Bban)
                .Build());
            return ib.ToString(format);
        }
    }
}
