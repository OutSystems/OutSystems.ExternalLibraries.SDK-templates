using IbanNet;
using IbanNet.Registry;
using NUnit.Framework;
using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.IbanChecker.UnitTests; 

public class IbanCheckerTests {

    /// <summary>
    /// Tests if the Iban constructor correctly creates the Iban struct for a
    /// sample IBAN string.
    /// </summary>
    [Test]
    public void IbanStructureIsCorrectlyCreatedWhenGivenIban() {
        // Setup: Create a parser using the IbanNet library to parse a sample IBAN
        // string.
        var parser = new IbanParser(IbanRegistry.Default);
        var iban = parser.Parse("NL91 ABNA 0417 1643 00");

        // Act: Create an Iban struct using the parsed IBAN.
        var ibanStruct = new Iban(iban);

        // Assert: Verify the Iban struct is correctly created by checking its
        // components.
        Assert.Multiple(() => {
            Assert.That(ibanStruct.Country, Is.EqualTo(iban.Country.TwoLetterISORegionName));
            Assert.That(ibanStruct.BranchIdentifier, Is.EqualTo(iban.BranchIdentifier).Or.EqualTo(string.Empty));
            Assert.That(ibanStruct.BankIdentifier, Is.EqualTo(iban.BankIdentifier).Or.EqualTo(string.Empty));
            Assert.That(ibanStruct.Bban, Is.EqualTo(iban.Bban));
        });
    }

    /// <summary>
    /// Tests if the IbanChecker class correctly uses the Parse method to parse a
    /// sample IBAN string.
    /// </summary>
    [Test]
    public void IbanCheckerCorrectlyParsesIban() {
        // Setup: Create a parser using the IbanNet library to parse a sample IBAN
        // string.
        var parser = new IbanParser(IbanRegistry.Default);
        var iban = parser.Parse("NL91 ABNA 0417 1643 00");
        var checker = new IbanChecker();

        // Act: Use the Parse method of the IbanChecker class to parse a sample IBAN
        // string.
        var ibanStruct = checker.Parse("NL91 ABNA 0417 1643 00");

        // Assert: Verify the Iban struct is correctly created by checking its
        // components.
        Assert.Multiple(() => {
        Assert.That(ibanStruct, Is.Not.Null);
            Assert.That(ibanStruct?.Country, Is.EqualTo(iban.Country.TwoLetterISORegionName));
            Assert.That(ibanStruct?.BranchIdentifier, Is.EqualTo(iban.BranchIdentifier).Or.EqualTo(string.Empty));
            Assert.That(ibanStruct?.BankIdentifier, Is.EqualTo(iban.BankIdentifier).Or.EqualTo(string.Empty));
            Assert.That(ibanStruct?.Bban, Is.EqualTo(iban.Bban));
        });
    }

    /// <summary>
    /// Verifies the Iban class is decorated with OSStructureAttribute, which is
    /// required to expose the Iban struct as a structure to ODC apps and libraries.
    /// </summary>
    [Test]
    public void IbanStructureHasOSStructureAttribute() {
        Assert.That(Attribute.GetCustomAttribute(typeof(Iban), typeof(OSStructureAttribute)), Is.Not.Null);
    }

    /// <summary>
    /// Verifies the IIbanChecker interface is decorated with OSInterfaceAttribute,
    /// which is required to expose the interface methods as server actions to ODC
    /// apps and libraries.
    /// </summary>
    [Test]
    public void IbanCheckerInterfaceHasOSInterfaceAttribute() {
        Assert.That(Attribute.GetCustomAttribute(typeof(IIbanChecker), typeof(OSInterfaceAttribute)), Is.Not.Null);
    }
}