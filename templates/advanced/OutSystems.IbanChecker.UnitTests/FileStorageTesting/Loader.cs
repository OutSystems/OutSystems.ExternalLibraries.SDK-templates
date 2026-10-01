using OutSystems.IbanChecker.UnitTests.FileStorageTesting;

// TEST PROJECT ONLY. FileStorage.GetInstance() looks in the loaded assemblies for this exact
// type name and, if found, uses CreateInstance() instead of the real ODC file storage.
// Never add this class to the External Library project: the published library would then
// store files in memory instead of in ODC.
namespace OutSystems.ExternalLibraries.SDK.FileStorage;

public static class Loader {
    public static IFileStorage CreateInstance() => new InMemoryFileStorage();
}
