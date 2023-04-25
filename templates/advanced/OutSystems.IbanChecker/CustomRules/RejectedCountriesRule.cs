using System.Collections.Generic;
using IbanNet.Validation.Results;
using IbanNet.Validation.Rules;

namespace OutSystems.IbanChecker.CustomRules {
    /// <summary>
    /// Represents a custom error result for rejected country codes.
    /// </summary>
    class CountryNotAcceptedError : ErrorResult {
        /// <summary>
        /// Initializes a new instance of the CountryNotAcceptedError class with the specified country code.
        /// </summary>
        /// <param name="countryCode">The rejected country code.</param>
        public CountryNotAcceptedError(string countryCode)
            : base($"Bank account numbers from country '{countryCode}' not accepted.") {
        }
    }

    /// <summary>
    /// Represents a custom IBAN validation rule that rejects specified country codes.
    /// </summary>
    class RejectCountryRule : IIbanValidationRule {
        private readonly ISet<string> _rejectedCountryCodes;

        /// <summary>
        /// Initializes a new instance of the RejectCountryRule class with the specified rejected country codes.
        /// </summary>
        /// <param name="rejectedCountryCodes">A collection of country codes to reject during validation.</param>
        public RejectCountryRule(IEnumerable<string> rejectedCountryCodes) {
            _rejectedCountryCodes = new HashSet<string>(rejectedCountryCodes);
        }

        /// <summary>
        /// Validates an IBAN according to the custom rule. Returns a ValidationResult indicating the success or failure of the validation.
        /// </summary>
        /// <param name="context">The IbanValidationContext containing the information about the IBAN being validated.</param>
        /// <returns>A ValidationRuleResult indicating the success or failure of the validation.</returns>

        public ValidationRuleResult Validate(ValidationRuleContext context) {
            if (context.Country != null && _rejectedCountryCodes.Contains(context.Country.TwoLetterISORegionName)) {
                return new CountryNotAcceptedError(context.Country.TwoLetterISORegionName);
            }

            return ValidationRuleResult.Success;
        }
    }
}
