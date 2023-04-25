using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.IbanChecker.Structures {
    /// <summary>
    /// The Iban struct represents an International Bank Account Number (IBAN) and
    /// its components. It's exposed as a structure to your ODC apps and libraries.
    /// </summary>
    [OSStructure(Description = "Represents an IBAN (International Bank Account Number).")]
    public struct Iban {
        [OSStructureField(Description = "The two-letter ISO region name of the country.", IsMandatory = true)]
        /// <summary>
        /// Gets the two-letter ISO region name of the country from the IBAN.
        /// </summary>
        public IbanCountry Country;

        [OSStructureField(DataType = OSDataType.Text, Description = "The BBAN (Basic Bank Account Number) part of the IBAN.", IsMandatory = true)]
        /// <summary>
        /// Gets the Basic Bank Account Number (BBAN) part of the IBAN.
        /// </summary>
        public string Bban;

        [OSStructureField(DataType = OSDataType.Text, Description = "The bank identifier part of the IBAN.", IsMandatory = true)]
        /// <summary>
        /// Gets the bank identifier, or <see langword="null" /> if the bank
        /// identifier cannot be extracted.
        /// </summary>
        public string BankIdentifier;

        [OSStructureField(DataType = OSDataType.Text, Description = "The branch identifier part of the IBAN.", IsMandatory = true)]
        /// <summary>
        /// Gets the branch identifier, or <see langword="null" /> if the branch
        /// identifier cannot be extracted.
        /// </summary>
        public string BranchIdentifier;

        /// <summary>
        /// Constructs an Iban struct from the IbanNet.Iban object.
        /// </summary>
        public Iban(IbanNet.Iban iban) : this() {
            Country = new IbanCountry(iban.Country);
            Bban = iban.Bban;
            BankIdentifier = iban.BankIdentifier ?? string.Empty;
            BranchIdentifier = iban.BranchIdentifier ?? string.Empty;
        }
    }

}