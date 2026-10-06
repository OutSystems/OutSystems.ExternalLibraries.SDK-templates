using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.IbanChecker.Structures {
    /// <summary>
    /// A single key/value pair of user metadata attached to a file.
    /// Exposed as a structure to your ODC apps and libraries.
    /// </summary>
    [OSStructure(Description = "A single key/value pair of user metadata attached to a file.")]
    public struct MetadataEntry {

        [OSStructureField(DataType = OSDataType.Text, Description = "The metadata key.", IsMandatory = true)]
        /// <summary>
        /// The metadata key.
        /// </summary>
        public string Key;

        [OSStructureField(DataType = OSDataType.Text, Description = "The metadata value.", IsMandatory = true)]
        /// <summary>
        /// The metadata value.
        /// </summary>
        public string Value;

        /// <summary>
        /// Constructs a MetadataEntry structure from a key and a value.
        /// </summary>
        public MetadataEntry(string key, string value) {
            Key = key;
            Value = value;
        }
    }
}
