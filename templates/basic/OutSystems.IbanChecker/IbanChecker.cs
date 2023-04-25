using IbanNet;

namespace OutSystems.IbanChecker {
    /// <summary>
    /// The IbanChecker class implements the IIbanChecker interface, providing
    /// the actual functionality for parsing and validating IBANs according
    /// to the rules defined by the IbanNet library.
    /// </summary>
    public class IbanChecker : IIbanChecker {
        /// <summary>
        /// An instance of IIbanParser from the IbanNet library is used to
        /// perform the actual IBAN parsing.
        /// </summary>
        private IIbanParser _parser;

        /// <summary>
        /// The constructor initializes the IbanChecker class with a new
        /// instance of the IbanParser class from the IbanNet library, which
        /// is configured with a default IbanValidator.
        /// </summary>
        public IbanChecker() {
            _parser = new IbanParser(new IbanValidator());
        }

        /// <summary>
        /// The Parse method takes a string value representing an IBAN number,
        /// and attempts to parse and validate it using the IIbanParser instance.
        /// If the parsing is successful, it returns an Iban struct. If an
        /// IbanFormatException occurs during parsing, it returns
        /// <see langword="null" />.
        /// </summary>
        /// <param name="value">IBAN in any textual format.</param>
        /// <returns>An Iban struct if the parsing is successful, or
        /// <see langword="null" /> if an IbanFormatException occurs.</returns>
        public Iban? Parse(string value) {
            try {
                return new Iban(_parser.Parse(value));
            } catch (IbanFormatException) {
                return null;
            }
        }
    }
}