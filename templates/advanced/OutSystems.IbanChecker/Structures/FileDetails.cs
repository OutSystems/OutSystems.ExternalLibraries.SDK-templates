using System.Collections.Generic;
using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.IbanChecker.Structures {
    /// <summary>
    /// Describes a file kept in ODC file storage: its name, content type, size and the user
    /// metadata attached to it. Exposed as a structure to your ODC apps and libraries.
    /// </summary>
    [OSStructure(Description = "Describes a file kept in ODC file storage: its name, content type, size and user metadata.")]
    public struct FileDetails {

        [OSStructureField(DataType = OSDataType.Text, Description = "The name of the file.", IsMandatory = true)]
        /// <summary>
        /// The name of the file.
        /// </summary>
        public string FileName;

        [OSStructureField(DataType = OSDataType.Text, Description = "The content type (MIME type) of the file.", IsMandatory = true)]
        /// <summary>
        /// The content type (MIME type) of the file.
        /// </summary>
        public string ContentType;

        [OSStructureField(DataType = OSDataType.LongInteger, Description = "The size of the file, in bytes.", IsMandatory = true)]
        /// <summary>
        /// The size of the file, in bytes.
        /// </summary>
        public long Size;

        [OSStructureField(Description = "The user metadata attached to the file when it was created.", IsMandatory = true)]
        /// <summary>
        /// The user metadata attached to the file when it was created.
        /// </summary>
        public List<MetadataEntry> UserData;

        /// <summary>
        /// Constructs a FileDetails structure with an empty list of user metadata.
        /// </summary>
        public FileDetails(string fileName, string contentType, long size) {
            FileName = fileName;
            ContentType = contentType;
            Size = size;
            UserData = new List<MetadataEntry>();
        }
    }
}
