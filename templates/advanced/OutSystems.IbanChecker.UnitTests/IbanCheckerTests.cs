using IbanNet;
using IbanNet.Registry;
using NUnit.Framework;
using OutSystems.ExternalLibraries.SDK;
using Iban = OutSystems.IbanChecker.Structures.Iban;

namespace OutSystems.IbanChecker.UnitTests;

public class IbanCheckerTests {

    /// <summary>
    /// Tests if the Iban constructor correctly creates the Iban struct for
    /// a sample IBAN string.
    /// </summary>
    [Test]
    public void IbanStructureIsCorrectlyCreatedWhenGivenIban() {
        // Setup: Create a parser using the IbanNet library to parse a sample IBAN
        // string.
        var parser = new IbanParser(IbanRegistry.Default);
        var iban = parser.Parse("NL91 ABNA 0417 1643 00");

        // Act: Create an Iban struct using the parsed IBAN.
        var ibanStruct = new Iban(iban);

        // Assert: Verify the Iban struct matches the parsed IBAN.
        AssertIbanStructure(ibanStruct, iban);
    }

    /// <summary>
    /// Tests if the IbanCountry constructor correctly creates the
    /// IbanCountry struct for a sample IbanCountry object.
    /// </summary>
    [Test]
    public void IbanCountryStructureIsCorrectlyCreatedWhenGivenIbanCountry() {
        // Setup: Create a parser using the IbanNet library to parse a sample IBAN
        // and retrieve its country.
        var parser = new IbanParser(IbanRegistry.Default);
        var ibanCountry = parser.Parse("NL91 ABNA 0417 1643 00").Country;

        // Act: Create an IbanCountry struct using the IbanCountry object.
        var ibanCountryStruct = new Structures.IbanCountry(ibanCountry);

        // Assert: Verify that the IbanCountry struct matches the IbanCountry
        // object.
        AssertIbanCountryStructure(ibanCountryStruct, ibanCountry);
    }

    /// <summary>
    /// Tests if the ValidationResult constructor correctly creates the
    /// ValidationResult struct for a sample ValidationResult object.
    /// </summary>
    [Test]
    public void ValidationResultStructureIsCorrectlyCreatedWhenGivenValidationResult() {
        // Setup: Create a validator using the IbanNet library to validate a sample
        // IBAN.
        var validator = new IbanValidator();
        var validationResult = validator.Validate("NL91ABNA0417164300");

        // Act: Create a ValidationResult struct using the ValidationResult object.
        var validationResultStruct = new Structures.ValidationResult(validationResult);

        // Assert: Verify the ValidationResult struct matches the ValidationResult
        // object.
        AssertIbanCountryStructure(validationResultStruct.Country ?? default, validationResult.Country ?? new IbanCountry("NL"));
        Assert.Multiple(() => {
            Assert.That(validationResultStruct.Error, Is.EqualTo(validationResult.Error?.ToString()));
            Assert.That(validationResultStruct.AttemptedValue, Is.EqualTo(validationResult.AttemptedValue));
        });
    }

    /// <summary>
    /// Tests if IbanChecker correctly parses a sample valid IBAN string using the
    /// Parse method.
    /// </summary>
    [Test]
    public void IbanCheckerCorrectlyParsesValidIban() {
        // Setup: Instantiate a new IbanParser and IbanChecker and parse a sample
        // valid IBAN string using the IbanParser.
        var parser = new IbanParser(IbanRegistry.Default);
        var iban = parser.Parse("NL91 ABNA 0417 1643 00");
        var checker = new IbanChecker();

        // Act: Parse a sample valid IBAN string using the IbanChecker.
        var ibanStruct = checker.Parse("NL91 ABNA 0417 1643 00");

        // Assert: Verify the parsed Iban struct matches the expected struct from
        // IbanParser.
        AssertIbanStructure(ibanStruct, iban);
    }

    /// <summary>
    /// Tests if IbanChecker correctly tries to parse a sample valid IBAN string
    /// using the TryParse method.
    /// </summary>
    [Test]
    public void IbanCheckerCorrectlyTryParsesValidIban() {

        // Setup: Instantiate a new IbanParser and IbanChecker and parse a sample
        // valid IBAN string using the IbanParser.
        var parser = new IbanParser(IbanRegistry.Default);
        var iban = parser.Parse("NL91 ABNA 0417 1643 00");
        var checker = new IbanChecker();

        // Act: Try to parse a sample valid IBAN string using the IbanChecker and
        // check for success.
        var success = checker.TryParse("NL91 ABNA 0417 1643 00", out var ibanStruct);

        // Assert: Check that the TryParse method returns true and the parsed Iban
        // struct matches the expected struct from IbanParser.
        Assert.Multiple(() => {
            Assert.That(success, Is.True);
            Assert.That(ibanStruct, Is.Not.Null);
        });
        AssertIbanStructure(ibanStruct ?? default, iban);
    }

    // <summary>
    // Tests if IbanChecker correctly tries to parse a sample invalid IBAN string
    // using the TryParse method.
    // The test ensures the TryParse method returns false when parsing an invalid
    // IBAN string.
    // </summary>
    [Test]
    public void IbanCheckerCorrectlyTryParsesInvalidIban() {
        // Setup: Instantiate a new IbanChecker.
        var checker = new IbanChecker();

        // Act: Try to parse a sample invalid IBAN string using the IbanChecker and
        // check for failure.
        var success = checker.TryParse("NL91 ABNA w17 1q3 00", out var ibanStruct);

        // Assert: Check the TryParse method returns false and the output Iban
        // structure is null.
        Assert.Multiple(() => {
            Assert.That(success, Is.False);
            Assert.That(ibanStruct, Is.Null);
        });
    }

    /// <summary>
    /// Tests if IbanChecker correctly validates an IBAN from a sample rejected
    /// country using the Validate method.
    /// This test ensures the Validate method returns an error when validating an
    /// IBAN from a rejected country.
    /// </summary>
    [Test]
    public void IbanCheckerCorrectlyValidatesIbanFromRejectedCountry() {
        // Setup: Instantiate a new IbanChecker.
        var checker = new IbanChecker();

        // Act: Validate an IBAN using the IbanChecker, providing a list of
        // rejected countries.
        var validationResultStruct = checker.Validate("NL91ABNA0417164300", new[] {"NL"} );

        // Assert: Verify ValidationResult contains an error and the country
        // information is correct.
        Assert.Multiple(() => {
            Assert.That(validationResultStruct.AttemptedValue, Is.EqualTo("NL91ABNA0417164300"));
            Assert.That(validationResultStruct.Error, Is.Not.Null);
            Assert.That(validationResultStruct.Country?.TwoLetterISORegionName, Is.EqualTo("NL"));
        });
    }

    /// <summary>
    /// Tests if IbanChecker correctly validates an IBAN from a sample accepted
    /// country using the Validate method.
    /// This test ensures the Validate method does not return an error when
    /// validating an IBAN from an accepted country.
    /// </summary>
    [Test]
    public void IbanCheckerCorrectlyValidatesIbanFromAcceptedCountry() {
        // Setup: Instantiate a new IbanChecker.
        var checker = new IbanChecker();

        // Act: Validate an IBAN using the IbanChecker, providing a list of
        // rejected countries that does not include the IBAN's country code.
        var validationResultStruct = checker.Validate("NL91ABNA0417164300", new[] {"PT"} );

        // Assert: Verify ValidationResult does not contain an error and the
        // country information is correct.
        Assert.Multiple(() => {
            Assert.That(validationResultStruct.AttemptedValue, Is.EqualTo("NL91ABNA0417164300"));
            Assert.That(validationResultStruct.Error, Is.Null);
            Assert.That(validationResultStruct.Country?.TwoLetterISORegionName, Is.EqualTo("NL"));
        });
    }

    // This block of assertions checks various properties of the Iban and
    // IbanCountry structs to ensure they are equal.
    private static void AssertIbanStructure(Iban ibanStruct, IbanNet.Iban iban) {
        Assert.Multiple(() => {
            AssertIbanCountryStructure(ibanStruct.Country, iban.Country);
            Assert.That(ibanStruct.BranchIdentifier, Is.EqualTo(iban.BranchIdentifier).Or.EqualTo(string.Empty));
            Assert.That(ibanStruct.BankIdentifier, Is.EqualTo(iban.BankIdentifier).Or.EqualTo(string.Empty));
            Assert.That(ibanStruct.Bban, Is.EqualTo(iban.Bban));
            Assert.That(Attribute.GetCustomAttribute(ibanStruct.GetType(), typeof(OSStructureAttribute)), Is.Not.Null);
            Assert.That(Attribute.GetCustomAttribute(ibanStruct.Country.GetType(), typeof(OSStructureAttribute)), Is.Not.Null);
        });
    }

    // This helper method compares two IbanCountry structs and checks if their
    // properties are equal.
    private static void AssertIbanCountryStructure(Structures.IbanCountry ibanCountryStruct, IbanCountry ibanCountry) {
        Assert.Multiple(() => {
            Assert.That(ibanCountryStruct.TwoLetterISORegionName, Is.EqualTo(ibanCountry.TwoLetterISORegionName));
            Assert.That(ibanCountryStruct.DisplayName, Is.EqualTo(ibanCountry.DisplayName));
            Assert.That(ibanCountryStruct.EnglishName, Is.EqualTo(ibanCountry.EnglishName));
            Assert.That(ibanCountryStruct.NativeName, Is.EqualTo(ibanCountry.NativeName).Or.EqualTo(string.Empty));
            Assert.That(ibanCountryStruct.DomesticAccountNumberExample, Is.EqualTo(ibanCountry.DomesticAccountNumberExample));
        });
    }

    /// <summary>
    /// Verifies the Iban struct is decorated with OSStructureAttribute,
    /// which is required to expose the Iban struct as a structure to ODC
    /// apps and libraries.
    /// </summary>
    [Test]
    public void IbanStructureHasOSStructureAttribute() {
        Assert.That(Attribute.GetCustomAttribute(typeof(Iban), typeof(OSStructureAttribute)), Is.Not.Null);
    }

    /// <summary>
    /// Verifies the IbanCountry struct is decorated with OSStructureAttribute,
    /// which is required to expose the IbanCountry struct as a structure to ODC
    /// apps and libraries.
    /// </summary>
    [Test]
    public void IbanCountryStructureHasOSStructureAttribute() {
        Assert.That(Attribute.GetCustomAttribute(typeof(Structures.IbanCountry), typeof(OSStructureAttribute)), Is.Not.Null);
    }

    /// <summary>
    /// Verifies the ValidationResult class is decorated with OSStructureAttribute,
    /// which is required to expose the ValidationResult struct as a structure to ODC
    /// apps and libraries.
    /// </summary>
    [Test]
    public void ValidationResultStructureHasOSStructureAttribute() {
        Assert.That(Attribute.GetCustomAttribute(typeof(Structures.ValidationResult), typeof(OSStructureAttribute)), Is.Not.Null);
    }

    /// <summary>
    /// Verifies the IIbanChecker interface is decorated with OSInterfaceAttribute,
    /// which is required to expose the IIbanChecker interface as an External Library to ODC
    /// apps and libraries.
    /// </summary>
    [Test]
    public void IbanCheckerInterfaceHasOSInterfaceAttribute() {
        Assert.That(Attribute.GetCustomAttribute(typeof(IIbanChecker), typeof(OSInterfaceAttribute)), Is.Not.Null);
    }
}