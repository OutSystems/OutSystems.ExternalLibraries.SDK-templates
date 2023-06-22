## IBAN (International Bank Account Number) checker: Advanced version 

### Prerequisites

* .NET 6.0 SDK installed.
* An IDE that supports building .NET 6 projects. For example, Visual Studio, Visual Studio Code, and Jet Brains Rider.
* Basic knowledge of C# programming concepts.

### Usage

1. Load the C# project file, `OutSystems.IbanChecker.csproj`, using a supported IDE.

    Files in the project:

     * **IIbanChecker.cs**: Defines a public interface named `IIbanChecker` decorated with the `OSInterface` attribute. The interface has four methods:
    
        * `Parse`: Takes an IBAN string as input and returns an `Iban` struct.
        * `TryParse`: Attempts to parse an IBAN string as input and returns a boolean success indicator along with the parsed `Iban` struct.
        * `Validate`: Takes an IBAN string as input as checks it against a specific rule and a list of rejected countries.
        * `Format`: Takes an `Iban` struct and an optional format string as input and returns a formatted string representation of the IBAN.

        Each method is exposed as a server action to your ODC apps and libraries.

    * **IbanChecker.cs**: Defines a public class named `IbanChecker` that implements the `IIbanChecker` interface. The class is a convenient wrapper for the `IbanNet` library, an [open-source library](https://github.com/skwasjer/IbanNet) that provides functionality for parsing and validating IBANs. The class contains private fields `_parser` and `_validator`, which are instances of the `IIbanParser` and `IIbanValidator` interfaces. The constructor initializes these instances for use in the class methods.

    * **Structures/Iban.cs** Defines a struct named `Iban`, decorated with the `OSStructure` attribute. The struct has four public properties: `Country`, `Bban`, `BankIdentifier`, and `BranchIdentifier`. It's exposed as a structure to your ODC apps and libraries.

    * **Structures/IbanCountry.cs** Defines a struct named `IbanCountry`, decorated with the `OSStructure` attribute. The struct has five public properties: `TwoLetterISORegionName`, `DisplayName`, `NativeName`, `EnglishName`, and `DomesticAccountNumberExample`. It's exposed as a structure to your ODC apps and libraries.

    * **Structures/ValidationResult.cs** Defines a struct named `ValidationResult`, decorated with the `OSStructure` attribute. The struct has three public properties: `AttemptedValue`, `Country`, and `Error`. It's exposed as a structure to your ODC apps and libraries.

    * **CustomRules/RejectedCountriesRule.cs**: Defines a custom IBAN validation rule, `RejectCountryRule`, to reject specified country codes. It also defines an associated error result class, `CountryNotAcceptedError`, for handling rejected countries.

1. Edit the code to meet your use case. If your project requires unit tests, modify the examples found in `../OutSystems.IbanChecker.UnitTests/IbanCheckerTests.cs` accordingly.

1. Run the Powershell script `generate_upload_package.ps1` to generate `ExternalLibrary.zip`. Rename as required.

1. Upload the generated ZIP file to the ODC Portal. See the [External Logic feature documentation](https://www.outsystems.com/goto/external-logic-upload) for guidance on how to do this.

_(Excerpted from the [main README of the External Libraries SDK](https://www.outsystems.com/goto/external-logic-readme), please refer to that document for additional guidance.)_