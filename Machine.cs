using ShUtilities.IO;

[Serializable]
internal class Machine
{
    public string Name { get; set; }
    public string TestEquivalent { get; set; }

    public static IEnumerable<string> LoadNamesFromFile(string path, EnvironmentKind environmentKind) =>
        new XmlSerializer<List<Machine>>()
        .DeserializeFile(path)
        .Select(x => environmentKind == EnvironmentKind.Prod ? x.Name : x.TestEquivalent)
        .Distinct();
}
