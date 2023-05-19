using OutSystems.ExternalLibraries.SDK;

namespace ConsumeSOAPExample.Structures {
    // It's exposed as a structure to your ODC apps and libraries.
    [OSStructure(Description = "Number A & B")]
    public struct Numbers {
        // Gets Number A & B to do the sum.
        [OSStructureField(DataType = OSDataType.Integer, Description = "Number A.", IsMandatory = true)]
        public int NumberA;

        [OSStructureField(DataType = OSDataType.Integer, Description = "Number B.", IsMandatory = true)]
        public int NumberB;
    }
}