using IbanNet.Registry;
using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.IbanChecker.Structures {
    /// <summary>
    /// Represents the validation result of an International Bank Account Number
    /// (IBAN) check. Exposed as a structure to your ODC apps and libraries.
    /// </summary>
    [OSStructure(Description = "Represents the validation result of an International Bank Account Number (IBAN) check.")]
    public struct ValidationResult {

        [OSStructureField(DataType = OSDataType.Text, Description = "The IBAN for which validation was attempted.", IsMandatory = true)]
        /// <summary>
        /// The IBAN for which validation was attempted.
        /// </summary>
        public string? AttemptedValue;

        [OSStructureField(Description = "The country information that matches the IBAN, if available.", IsMandatory = true)]
        /// <summary>
        /// Gets the country information that matches the IBAN, if available.
        /// </summary>
        public IbanCountry? Country;

        [OSStructureField(DataType = OSDataType.Text, Description = "The error message that occurred during validation, if any.", IsMandatory = true)]
        /// <summary>
        /// Gets the error message that occurred during validation, if any.
        /// </summary>
        public string? Error;

        /// <summary>
        /// Constructs an ValidationResult structure from the IbanNet.ValidationResult object.
        /// </summary>
        public ValidationResult(IbanNet.ValidationResult validationResult) : this() {
            AttemptedValue = validationResult.AttemptedValue;
            if (validationResult.Country != null) {
                Country = new IbanCountry(validationResult.Country);
            }
            Error = validationResult.Error?.ErrorMessage;
        }
    }

}