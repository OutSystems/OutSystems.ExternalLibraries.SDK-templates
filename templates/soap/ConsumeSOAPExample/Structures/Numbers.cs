using OutSystems.ExternalLibraries.SDK;

namespace ConsumeSOAPExample.Structures {
    /// <summary>
    /// The Numbers struct contains two numbers (NumberA and NumberB) for 
    /// mathematical operations. It's exposed as a structure to your ODC apps
    /// and libraries.
    /// </summary>
    [OSStructure(Description = "Number A & B")]
    public struct Numbers {
        [OSStructureField(DataType = OSDataType.Integer, Description = "Number A.", IsMandatory = true)]
        /// <summary>
        /// Gets Number A, which is one of the operands for mathematical
        /// operations.
        /// </summary>
        public int NumberA;

        [OSStructureField(DataType = OSDataType.Integer, Description = "Number B.", IsMandatory = true)]
        /// <summary>
        /// Gets Number B, which is one of the operands for mathematical
        /// operations.
        /// </summary>
        public int NumberB;
    }
}