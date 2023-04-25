using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.IbanChecker {
    /// <summary>
    /// The IIbanChecker interface defines the methods (exposed as server actions)
    /// for the IBAN validation functionality.
    /// </summary>
    [OSInterface]
    public interface IIbanChecker {
        /// <summary>
        /// Parses an International Bank Account Number (IBAN).
        /// This method is exposed as a server action to your ODC apps and libraries.
        /// </summary>
        /// <param name="value">IBAN in any textual format</param>
        /// <returns>An Iban struct if the parsing is successful, or <see langword="null" /> 
        // if there is any error.</returns>
        Iban? Parse(string value);
    }
}