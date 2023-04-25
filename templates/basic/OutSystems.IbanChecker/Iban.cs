using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.IbanChecker {
    /// <summary>
    /// The Iban struct represents an International Bank Account Number (IBAN)
    /// and its components. It's exposed as a structure to your ODC apps and
    /// libraries.
    /// </summary>
    [OSStructure]
    public struct Iban {
        /// <summary>
        /// Gets the two-letter ISO region name of the country from the IBAN.
        /// </summary>
        public string Country;

        /// <summary>
        /// Gets the Basic Bank Account Number (BBAN) part of the IBAN.
        /// </summary>
        public string Bban;

        /// <summary>
        /// Gets the bank identifier, or an empty string if the bank identifier
        /// cannot be extracted.
        /// </summary>
        public string BankIdentifier;

        /// <summary>
        /// Gets the branch identifier, or an empty string if the branch
        /// identifier cannot be extracted.
        /// </summary>

        /// <summary>
        /// Constructs an Iban struct from the IbanNet.Iban object.
        /// </summary>
        public Iban(IbanNet.Iban iban) : this() {
            Country = iban.Country.TwoLetterISORegionName;
            Bban = iban.Bban;
            BankIdentifier = iban.BankIdentifier ?? string.Empty;
            BranchIdentifier = iban.BranchIdentifier ?? string.Empty;
        }
    }

}