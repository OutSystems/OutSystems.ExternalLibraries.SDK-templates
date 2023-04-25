using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.IbanChecker.Structures {
    /// <summary>
    /// The IbanCountry struct represents an International Bank Account Number (IBAN)
    /// country details. It's exposed as a structure to your ODC apps and libraries.
    /// </summary>
    [OSStructure(Description = "Represents IBAN (International Bank Account Number) country details.")]
    public struct IbanCountry {
        [OSStructureField(DataType = OSDataType.Text, Description = "The two-letter ISO region name of the country.", IsMandatory = true)]
        /// <summary>
        /// Gets the two-letter ISO region name of the country from the IBAN.
        /// </summary>
        public string TwoLetterISORegionName;

        [OSStructureField(DataType = OSDataType.Text, Description = "The display name of the country.", IsMandatory = true)]
        /// <summary>
        /// Gets the display name of the country.
        /// </summary>
        public string DisplayName;

        [OSStructureField(DataType = OSDataType.Text, Description = "The native name of the country, if available.", IsMandatory = true)]
        /// <summary>
        /// Gets the native name of the country, if available.
        /// </summary>
        public string NativeName;

        [OSStructureField(DataType = OSDataType.Text, Description = "The English name of the country.", IsMandatory = true)]
        /// <summary>
        /// Gets the English name of the country.
        /// </summary>
        public string EnglishName;

        [OSStructureField(DataType = OSDataType.Text, Description = "Gets an example of a domestic account number for the country.", IsMandatory = true)]
        /// <summary>
        /// An example of a domestic account number for the country.
        /// </summary>
        public string? DomesticAccountNumberExample;

        /// <summary>
        /// Constructs an IbanCountry struct from the IbanNet.Registry.IbanCountry object.
        /// </summary>
        public IbanCountry(IbanNet.Registry.IbanCountry country) : this() {
            TwoLetterISORegionName = country.TwoLetterISORegionName;
            DisplayName = country.DisplayName;
            NativeName = country.NativeName ?? string.Empty;
            EnglishName = country.EnglishName;
            DomesticAccountNumberExample = country.DomesticAccountNumberExample;
        }
    }
}
